import { contextBridge, ipcRenderer } from 'electron';
import { IPC_CHANNELS } from '@shared/constants';
import type { 
  User, 
  Child, 
  MonitoringSettings, 
  Alert, 
  SystemStats,
  AppConfig,
  IPCResponse 
} from '@shared/types';

// Define the API that will be exposed to the renderer process
const electronAPI = {
  // Authentication
  auth: {
    login: (credentials: { username: string; password: string }): Promise<IPCResponse<User>> =>
      ipcRenderer.invoke(IPC_CHANNELS.AUTH_LOGIN, credentials),
    
    logout: (): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.AUTH_LOGOUT),
    
    checkAuth: (): Promise<IPCResponse<User | null>> =>
      ipcRenderer.invoke(IPC_CHANNELS.AUTH_CHECK)
  },

  // User Management
  users: {
    create: (userData: Partial<User>): Promise<IPCResponse<User>> =>
      ipcRenderer.invoke(IPC_CHANNELS.USER_CREATE, userData),
    
    update: (id: string, userData: Partial<User>): Promise<IPCResponse<User>> =>
      ipcRenderer.invoke(IPC_CHANNELS.USER_UPDATE, id, userData),
    
    delete: (id: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.USER_DELETE, id),
    
    get: (id: string): Promise<IPCResponse<User>> =>
      ipcRenderer.invoke(IPC_CHANNELS.USER_GET, id),
    
    list: (): Promise<IPCResponse<User[]>> =>
      ipcRenderer.invoke(IPC_CHANNELS.USER_LIST)
  },

  // Child Management
  children: {
    create: (childData: Partial<Child>): Promise<IPCResponse<Child>> =>
      ipcRenderer.invoke(IPC_CHANNELS.CHILD_CREATE, childData),
    
    update: (id: string, childData: Partial<Child>): Promise<IPCResponse<Child>> =>
      ipcRenderer.invoke(IPC_CHANNELS.CHILD_UPDATE, id, childData),
    
    delete: (id: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.CHILD_DELETE, id),
    
    get: (id: string): Promise<IPCResponse<Child>> =>
      ipcRenderer.invoke(IPC_CHANNELS.CHILD_GET, id),
    
    list: (parentId?: string): Promise<IPCResponse<Child[]>> =>
      ipcRenderer.invoke(IPC_CHANNELS.CHILD_LIST, parentId)
  },

  // Monitoring
  monitoring: {
    start: (childId: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.MONITORING_START, childId),
    
    stop: (): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.MONITORING_STOP),
    
    getStatus: (): Promise<IPCResponse<{ isActive: boolean; childId?: string }>> =>
      ipcRenderer.invoke(IPC_CHANNELS.MONITORING_STATUS),
    
    getSettings: (childId: string): Promise<IPCResponse<MonitoringSettings>> =>
      ipcRenderer.invoke(IPC_CHANNELS.MONITORING_SETTINGS_GET, childId),
    
    updateSettings: (childId: string, settings: Partial<MonitoringSettings>): Promise<IPCResponse<MonitoringSettings>> =>
      ipcRenderer.invoke(IPC_CHANNELS.MONITORING_SETTINGS_UPDATE, childId, settings)
  },

  // Alerts
  alerts: {
    list: (childId?: string, limit?: number): Promise<IPCResponse<Alert[]>> =>
      ipcRenderer.invoke(IPC_CHANNELS.ALERT_LIST, childId, limit),
    
    markAsRead: (alertId: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.ALERT_MARK_READ, alertId),
    
    delete: (alertId: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.ALERT_DELETE, alertId)
  },

  // Reports
  reports: {
    daily: (childId: string, date: string): Promise<IPCResponse<any>> =>
      ipcRenderer.invoke(IPC_CHANNELS.REPORT_DAILY, childId, date),
    
    weekly: (childId: string, startDate: string): Promise<IPCResponse<any>> =>
      ipcRenderer.invoke(IPC_CHANNELS.REPORT_WEEKLY, childId, startDate),
    
    monthly: (childId: string, month: string, year: string): Promise<IPCResponse<any>> =>
      ipcRenderer.invoke(IPC_CHANNELS.REPORT_MONTHLY, childId, month, year),
    
    export: (type: 'daily' | 'weekly' | 'monthly', params: any): Promise<IPCResponse<string>> =>
      ipcRenderer.invoke(IPC_CHANNELS.REPORT_EXPORT, type, params)
  },

  // System
  system: {
    getStats: (): Promise<IPCResponse<SystemStats>> =>
      ipcRenderer.invoke(IPC_CHANNELS.SYSTEM_STATS),
    
    getConfig: (): Promise<IPCResponse<AppConfig>> =>
      ipcRenderer.invoke(IPC_CHANNELS.SYSTEM_CONFIG),
    
    updateConfig: (config: Partial<AppConfig>): Promise<IPCResponse<AppConfig>> =>
      ipcRenderer.invoke(IPC_CHANNELS.SYSTEM_UPDATE_CONFIG, config),
    
    backup: (): Promise<IPCResponse<string>> =>
      ipcRenderer.invoke(IPC_CHANNELS.SYSTEM_BACKUP),
    
    restore: (backupPath: string): Promise<IPCResponse<void>> =>
      ipcRenderer.invoke(IPC_CHANNELS.SYSTEM_RESTORE, backupPath)
  },

  // Window Management
  window: {
    minimize: (): Promise<void> =>
      ipcRenderer.invoke(IPC_CHANNELS.WINDOW_MINIMIZE),
    
    maximize: (): Promise<void> =>
      ipcRenderer.invoke(IPC_CHANNELS.WINDOW_MAXIMIZE),
    
    close: (): Promise<void> =>
      ipcRenderer.invoke(IPC_CHANNELS.WINDOW_CLOSE),
    
    hide: (): Promise<void> =>
      ipcRenderer.invoke(IPC_CHANNELS.WINDOW_HIDE),
    
    show: (): Promise<void> =>
      ipcRenderer.invoke(IPC_CHANNELS.WINDOW_SHOW)
  },

  // Event Listeners
  on: (channel: string, callback: (...args: any[]) => void) => {
    // Validate channel to prevent security issues
    const validChannels = Object.values(IPC_CHANNELS);
    if (validChannels.includes(channel as any)) {
      ipcRenderer.on(channel, callback);
    }
  },

  off: (channel: string, callback: (...args: any[]) => void) => {
    const validChannels = Object.values(IPC_CHANNELS);
    if (validChannels.includes(channel as any)) {
      ipcRenderer.removeListener(channel, callback);
    }
  },

  // Utility functions
  utils: {
    getVersion: () => process.versions.electron,
    getPlatform: () => process.platform,
    getArch: () => process.arch
  }
};

// Expose the API to the renderer process
contextBridge.exposeInMainWorld('electronAPI', electronAPI);

// Type declaration for TypeScript support in renderer
declare global {
  interface Window {
    electronAPI: typeof electronAPI;
  }
}
