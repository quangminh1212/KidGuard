import * as ffi from 'ffi-napi';
import * as ref from 'ref-napi';
import { EventEmitter } from 'events';
import { ContentFilterService } from './ContentFilterService';
import { NotificationService } from './NotificationService';
import { DatabaseManager } from '../database/DatabaseManager';
import { Logger } from '../utils/Logger';
import type { KeystrokeEvent, FilterResult } from '@shared/types';
import { MONITORING } from '@shared/constants';

// Windows API types
const DWORD = ref.types.uint32;
const LONG = ref.types.long;
const WPARAM = ref.types.uint64;
const LPARAM = ref.types.int64;
const LRESULT = ref.types.int64;

// Windows API structures
const POINT = ref.types.void;
const MSG = ref.types.void;
const KBDLLHOOKSTRUCT = ref.types.void;

// Hook types
const WH_KEYBOARD_LL = 13;
const HC_ACTION = 0;
const WM_KEYDOWN = 0x0100;
const WM_SYSKEYDOWN = 0x0104;

export class KeyloggerService extends EventEmitter {
  private user32: any;
  private kernel32: any;
  private hookHandle: any = null;
  private isMonitoring = false;
  private currentChildId: string | null = null;
  private textBuffer: string = '';
  private lastActivity: Date = new Date();
  private logger: Logger;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor(
    private contentFilterService: ContentFilterService,
    private notificationService: NotificationService,
    private databaseManager: DatabaseManager
  ) {
    super();
    this.logger = new Logger();
    this.initializeWindowsAPI();
  }

  private initializeWindowsAPI(): void {
    try {
      // Load Windows API libraries
      this.user32 = ffi.Library('user32', {
        'SetWindowsHookExW': [ref.types.void, [ref.types.int, ref.types.void, ref.types.void, DWORD]],
        'UnhookWindowsHookEx': [ref.types.bool, [ref.types.void]],
        'CallNextHookEx': [LRESULT, [ref.types.void, ref.types.int, WPARAM, LPARAM]],
        'GetMessageW': [ref.types.bool, [ref.types.void, ref.types.void, ref.types.uint, ref.types.uint]],
        'TranslateMessage': [ref.types.bool, [ref.types.void]],
        'DispatchMessageW': [LRESULT, [ref.types.void]],
        'GetForegroundWindow': [ref.types.void, []],
        'GetWindowTextW': [ref.types.int, [ref.types.void, ref.types.void, ref.types.int]],
        'GetClassNameW': [ref.types.int, [ref.types.void, ref.types.void, ref.types.int]],
        'GetWindowThreadProcessId': [DWORD, [ref.types.void, ref.types.void]],
        'GetKeyState': [ref.types.short, [ref.types.int]],
        'ToUnicodeEx': [ref.types.int, [ref.types.uint, ref.types.uint, ref.types.void, ref.types.void, ref.types.int, ref.types.uint, ref.types.void]]
      });

      this.kernel32 = ffi.Library('kernel32', {
        'GetModuleHandleW': [ref.types.void, [ref.types.void]],
        'GetCurrentThreadId': [DWORD, []],
        'OpenProcess': [ref.types.void, [DWORD, ref.types.bool, DWORD]],
        'CloseHandle': [ref.types.bool, [ref.types.void]],
        'QueryFullProcessImageNameW': [ref.types.bool, [ref.types.void, DWORD, ref.types.void, ref.types.void]]
      });

      this.logger.info('Windows API initialized successfully');
    } catch (error) {
      this.logger.error('Failed to initialize Windows API:', error);
      throw new Error('Failed to initialize keylogger service');
    }
  }

  public async startMonitoring(childId: string): Promise<boolean> {
    if (this.isMonitoring) {
      this.logger.warn('Monitoring is already active');
      return false;
    }

    try {
      this.currentChildId = childId;
      this.isMonitoring = true;
      this.textBuffer = '';
      this.lastActivity = new Date();

      // Install low-level keyboard hook
      const hookProc = this.createHookProcedure();
      const moduleHandle = this.kernel32.GetModuleHandleW(null);
      
      this.hookHandle = this.user32.SetWindowsHookExW(
        WH_KEYBOARD_LL,
        hookProc,
        moduleHandle,
        0
      );

      if (!this.hookHandle || this.hookHandle.isNull()) {
        throw new Error('Failed to install keyboard hook');
      }

      // Start monitoring interval for processing buffered text
      this.monitoringInterval = setInterval(() => {
        this.processTextBuffer();
      }, MONITORING.UPDATE_INTERVAL);

      this.logger.info(`Keylogger monitoring started for child: ${childId}`);
      this.emit('monitoring-started', { childId });
      
      return true;
    } catch (error) {
      this.logger.error('Failed to start monitoring:', error);
      this.isMonitoring = false;
      this.currentChildId = null;
      return false;
    }
  }

  public async stopMonitoring(): Promise<boolean> {
    if (!this.isMonitoring) {
      return true;
    }

    try {
      // Remove keyboard hook
      if (this.hookHandle && !this.hookHandle.isNull()) {
        this.user32.UnhookWindowsHookEx(this.hookHandle);
        this.hookHandle = null;
      }

      // Clear monitoring interval
      if (this.monitoringInterval) {
        clearInterval(this.monitoringInterval);
        this.monitoringInterval = null;
      }

      // Process any remaining text in buffer
      await this.processTextBuffer();

      this.isMonitoring = false;
      const childId = this.currentChildId;
      this.currentChildId = null;
      this.textBuffer = '';

      this.logger.info('Keylogger monitoring stopped');
      this.emit('monitoring-stopped', { childId });
      
      return true;
    } catch (error) {
      this.logger.error('Failed to stop monitoring:', error);
      return false;
    }
  }

  private createHookProcedure(): any {
    return ffi.Callback(LRESULT, [ref.types.int, WPARAM, LPARAM], 
      (nCode: number, wParam: number, lParam: any) => {
        try {
          if (nCode >= HC_ACTION && this.isMonitoring) {
            if (wParam === WM_KEYDOWN || wParam === WM_SYSKEYDOWN) {
              this.handleKeyPress(lParam);
            }
          }
        } catch (error) {
          this.logger.error('Error in hook procedure:', error);
        }

        return this.user32.CallNextHookEx(null, nCode, wParam, lParam);
      }
    );
  }

  private handleKeyPress(lParam: any): void {
    try {
      // Extract virtual key code from KBDLLHOOKSTRUCT
      const vkCode = ref.get(lParam, 0, ref.types.uint32);
      
      // Convert virtual key to character
      const character = this.virtualKeyToChar(vkCode);
      
      if (character) {
        this.textBuffer += character;
        this.lastActivity = new Date();

        // Limit buffer size
        if (this.textBuffer.length > MONITORING.KEYLOGGER_BUFFER_SIZE) {
          this.textBuffer = this.textBuffer.slice(-MONITORING.KEYLOGGER_BUFFER_SIZE);
        }

        // Get current window information
        const windowInfo = this.getCurrentWindowInfo();
        
        // Emit keystroke event
        this.emit('keystroke', {
          character,
          timestamp: new Date(),
          windowInfo
        });
      }
    } catch (error) {
      this.logger.error('Error handling key press:', error);
    }
  }

  private virtualKeyToChar(vkCode: number): string | null {
    try {
      // Handle special keys
      switch (vkCode) {
        case 8: return '[BACKSPACE]';
        case 9: return '[TAB]';
        case 13: return '[ENTER]';
        case 27: return '[ESC]';
        case 32: return ' ';
        case 46: return '[DELETE]';
        default:
          // Convert to Unicode character
          const keyboardState = Buffer.alloc(256);
          const unicodeBuffer = Buffer.alloc(4);
          
          const result = this.user32.ToUnicodeEx(
            vkCode,
            0,
            keyboardState,
            unicodeBuffer,
            2,
            0,
            null
          );

          if (result > 0) {
            return unicodeBuffer.toString('utf16le', 0, result * 2);
          }
          
          return null;
      }
    } catch (error) {
      this.logger.error('Error converting virtual key to character:', error);
      return null;
    }
  }

  private getCurrentWindowInfo(): { title: string; className: string; processName: string } {
    try {
      const hwnd = this.user32.GetForegroundWindow();
      
      if (!hwnd || hwnd.isNull()) {
        return { title: '', className: '', processName: '' };
      }

      // Get window title
      const titleBuffer = Buffer.alloc(512);
      this.user32.GetWindowTextW(hwnd, titleBuffer, 256);
      const title = titleBuffer.toString('utf16le').replace(/\0.*$/, '');

      // Get class name
      const classBuffer = Buffer.alloc(512);
      this.user32.GetClassNameW(hwnd, classBuffer, 256);
      const className = classBuffer.toString('utf16le').replace(/\0.*$/, '');

      // Get process name
      const processIdBuffer = Buffer.alloc(4);
      this.user32.GetWindowThreadProcessId(hwnd, processIdBuffer);
      const processId = processIdBuffer.readUInt32LE(0);
      
      const processHandle = this.kernel32.OpenProcess(0x1000, false, processId);
      let processName = '';
      
      if (processHandle && !processHandle.isNull()) {
        const processNameBuffer = Buffer.alloc(1024);
        const sizeBuffer = Buffer.alloc(4);
        sizeBuffer.writeUInt32LE(512, 0);
        
        if (this.kernel32.QueryFullProcessImageNameW(processHandle, 0, processNameBuffer, sizeBuffer)) {
          const fullPath = processNameBuffer.toString('utf16le').replace(/\0.*$/, '');
          processName = fullPath.split('\\').pop() || '';
        }
        
        this.kernel32.CloseHandle(processHandle);
      }

      return { title, className, processName };
    } catch (error) {
      this.logger.error('Error getting window info:', error);
      return { title: '', className: '', processName: '' };
    }
  }

  private async processTextBuffer(): Promise<void> {
    if (!this.textBuffer.trim() || !this.currentChildId) {
      return;
    }

    try {
      // Check content with filter service
      const filterResult: FilterResult = await this.contentFilterService.checkContent(this.textBuffer);
      
      if (filterResult.isFiltered) {
        // Create keystroke event record
        const windowInfo = this.getCurrentWindowInfo();
        const keystrokeEvent: Partial<KeystrokeEvent> = {
          childId: this.currentChildId,
          timestamp: new Date(),
          text: this.textBuffer,
          applicationName: windowInfo.processName,
          windowTitle: windowInfo.title,
          isFiltered: true,
          severity: filterResult.severity
        };

        // Save to database
        await this.databaseManager.saveKeystrokeEvent(keystrokeEvent);

        // Send notification
        await this.notificationService.sendAlert({
          childId: this.currentChildId,
          type: 'inappropriate_content',
          title: 'Inappropriate Content Detected',
          message: `Detected ${filterResult.severity} level content in ${windowInfo.processName}`,
          severity: filterResult.severity === 'critical' ? 'critical' : 'warning',
          timestamp: new Date(),
          isRead: false
        });

        this.logger.warn(`Inappropriate content detected: ${filterResult.severity} level`);
        this.emit('content-filtered', { keystrokeEvent, filterResult });
      }

      // Clear processed text from buffer
      this.textBuffer = '';
    } catch (error) {
      this.logger.error('Error processing text buffer:', error);
    }
  }

  public getStatus(): { isActive: boolean; childId?: string; lastActivity?: Date } {
    return {
      isActive: this.isMonitoring,
      childId: this.currentChildId || undefined,
      lastActivity: this.lastActivity
    };
  }

  public getTextBuffer(): string {
    return this.textBuffer;
  }
}
