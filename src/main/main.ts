import { app, BrowserWindow, ipcMain, Menu, Tray, nativeImage, dialog } from 'electron';
import { autoUpdater } from 'electron-updater';
import * as path from 'path';
import * as isDev from 'electron-is-dev';
import { DatabaseManager } from './database/DatabaseManager';
import { KeyloggerService } from './services/KeyloggerService';
import { ContentFilterService } from './services/ContentFilterService';
import { NotificationService } from './services/NotificationService';
import { AuthService } from './services/AuthService';
import { ConfigManager } from './config/ConfigManager';
import { Logger } from './utils/Logger';
import { IPC_CHANNELS } from '@shared/constants';

class ChildGuardApp {
  private mainWindow: BrowserWindow | null = null;
  private tray: Tray | null = null;
  private databaseManager: DatabaseManager;
  private keyloggerService: KeyloggerService;
  private contentFilterService: ContentFilterService;
  private notificationService: NotificationService;
  private authService: AuthService;
  private configManager: ConfigManager;
  private logger: Logger;
  private isQuitting = false;

  constructor() {
    this.logger = new Logger();
    this.configManager = new ConfigManager();
    this.databaseManager = new DatabaseManager();
    this.authService = new AuthService(this.databaseManager);
    this.contentFilterService = new ContentFilterService();
    this.notificationService = new NotificationService();
    this.keyloggerService = new KeyloggerService(
      this.contentFilterService,
      this.notificationService,
      this.databaseManager
    );

    this.initializeApp();
  }

  private async initializeApp(): Promise<void> {
    try {
      // Initialize database
      await this.databaseManager.initialize();
      
      // Initialize services
      await this.contentFilterService.initialize();
      await this.notificationService.initialize();
      
      this.logger.info('ChildGuard application initialized successfully');
    } catch (error) {
      this.logger.error('Failed to initialize application:', error);
      app.quit();
    }
  }

  public async createMainWindow(): Promise<void> {
    // Create the browser window
    this.mainWindow = new BrowserWindow({
      width: 1200,
      height: 800,
      minWidth: 800,
      minHeight: 600,
      show: false,
      icon: path.join(__dirname, '../assets/icon.png'),
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        enableRemoteModule: false,
        preload: path.join(__dirname, 'preload.js')
      },
      titleBarStyle: 'default',
      autoHideMenuBar: true
    });

    // Load the app
    if (isDev) {
      this.mainWindow.loadURL('http://localhost:3000');
      this.mainWindow.webContents.openDevTools();
    } else {
      this.mainWindow.loadFile(path.join(__dirname, 'index.html'));
    }

    // Show window when ready
    this.mainWindow.once('ready-to-show', () => {
      this.mainWindow?.show();
      
      if (isDev) {
        this.mainWindow?.webContents.openDevTools();
      }
    });

    // Handle window closed
    this.mainWindow.on('closed', () => {
      this.mainWindow = null;
    });

    // Handle window close event
    this.mainWindow.on('close', (event) => {
      if (!this.isQuitting) {
        event.preventDefault();
        this.mainWindow?.hide();
        
        this.notificationService.showNotification({
          title: 'ChildGuard',
          body: 'Application was minimized to tray',
          urgency: 'low'
        });
      }
    });

    this.setupIpcHandlers();
  }

  private createTray(): void {
    const iconPath = path.join(__dirname, '../assets/tray-icon.png');
    const trayIcon = nativeImage.createFromPath(iconPath);
    
    this.tray = new Tray(trayIcon);
    
    const contextMenu = Menu.buildFromTemplate([
      {
        label: 'Show ChildGuard',
        click: () => {
          this.mainWindow?.show();
        }
      },
      {
        label: 'Start Monitoring',
        click: () => {
          this.keyloggerService.startMonitoring();
        }
      },
      {
        label: 'Stop Monitoring',
        click: () => {
          this.keyloggerService.stopMonitoring();
        }
      },
      { type: 'separator' },
      {
        label: 'Quit',
        click: () => {
          this.isQuitting = true;
          app.quit();
        }
      }
    ]);

    this.tray.setToolTip('ChildGuard - Child Protection System');
    this.tray.setContextMenu(contextMenu);

    this.tray.on('double-click', () => {
      this.mainWindow?.show();
    });
  }

  private setupIpcHandlers(): void {
    // Authentication handlers
    ipcMain.handle(IPC_CHANNELS.AUTH_LOGIN, async (event, credentials) => {
      return await this.authService.login(credentials);
    });

    ipcMain.handle(IPC_CHANNELS.AUTH_LOGOUT, async () => {
      return await this.authService.logout();
    });

    ipcMain.handle(IPC_CHANNELS.AUTH_CHECK, async () => {
      return await this.authService.checkAuth();
    });

    // Monitoring handlers
    ipcMain.handle(IPC_CHANNELS.MONITORING_START, async (event, childId) => {
      return await this.keyloggerService.startMonitoring(childId);
    });

    ipcMain.handle(IPC_CHANNELS.MONITORING_STOP, async () => {
      return await this.keyloggerService.stopMonitoring();
    });

    ipcMain.handle(IPC_CHANNELS.MONITORING_STATUS, async () => {
      return this.keyloggerService.getStatus();
    });

    // System handlers
    ipcMain.handle(IPC_CHANNELS.SYSTEM_STATS, async () => {
      return await this.databaseManager.getSystemStats();
    });

    ipcMain.handle(IPC_CHANNELS.SYSTEM_CONFIG, async () => {
      return this.configManager.getConfig();
    });

    // Window management
    ipcMain.handle(IPC_CHANNELS.WINDOW_MINIMIZE, () => {
      this.mainWindow?.minimize();
    });

    ipcMain.handle(IPC_CHANNELS.WINDOW_MAXIMIZE, () => {
      if (this.mainWindow?.isMaximized()) {
        this.mainWindow.unmaximize();
      } else {
        this.mainWindow?.maximize();
      }
    });

    ipcMain.handle(IPC_CHANNELS.WINDOW_CLOSE, () => {
      this.mainWindow?.close();
    });
  }

  private setupAppEvents(): void {
    app.whenReady().then(() => {
      this.createMainWindow();
      this.createTray();
      
      // Auto-updater setup
      if (!isDev) {
        autoUpdater.checkForUpdatesAndNotify();
      }
    });

    app.on('window-all-closed', () => {
      // On macOS, keep app running even when all windows are closed
      if (process.platform !== 'darwin') {
        app.quit();
      }
    });

    app.on('activate', () => {
      if (BrowserWindow.getAllWindows().length === 0) {
        this.createMainWindow();
      }
    });

    app.on('before-quit', () => {
      this.isQuitting = true;
    });

    app.on('will-quit', async (event) => {
      event.preventDefault();
      
      try {
        await this.keyloggerService.stopMonitoring();
        await this.databaseManager.close();
        this.logger.info('Application shutdown completed');
        app.exit(0);
      } catch (error) {
        this.logger.error('Error during shutdown:', error);
        app.exit(1);
      }
    });
  }

  public run(): void {
    // Ensure single instance
    const gotTheLock = app.requestSingleInstanceLock();

    if (!gotTheLock) {
      app.quit();
      return;
    }

    app.on('second-instance', () => {
      // Someone tried to run a second instance, focus our window instead
      if (this.mainWindow) {
        if (this.mainWindow.isMinimized()) {
          this.mainWindow.restore();
        }
        this.mainWindow.focus();
      }
    });

    this.setupAppEvents();
  }
}

// Create and run the application
const childGuardApp = new ChildGuardApp();
childGuardApp.run();
