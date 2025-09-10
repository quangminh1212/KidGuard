// Application Constants

export const APP_NAME = 'ChildGuard';
export const APP_VERSION = '1.0.0';
export const APP_DESCRIPTION = 'Advanced Child Protection System';

// IPC Channels
export const IPC_CHANNELS = {
  // Authentication
  AUTH_LOGIN: 'auth:login',
  AUTH_LOGOUT: 'auth:logout',
  AUTH_CHECK: 'auth:check',
  
  // User Management
  USER_CREATE: 'user:create',
  USER_UPDATE: 'user:update',
  USER_DELETE: 'user:delete',
  USER_GET: 'user:get',
  USER_LIST: 'user:list',
  
  // Child Management
  CHILD_CREATE: 'child:create',
  CHILD_UPDATE: 'child:update',
  CHILD_DELETE: 'child:delete',
  CHILD_GET: 'child:get',
  CHILD_LIST: 'child:list',
  
  // Monitoring
  MONITORING_START: 'monitoring:start',
  MONITORING_STOP: 'monitoring:stop',
  MONITORING_STATUS: 'monitoring:status',
  MONITORING_SETTINGS_GET: 'monitoring:settings:get',
  MONITORING_SETTINGS_UPDATE: 'monitoring:settings:update',
  
  // Keylogger
  KEYLOGGER_START: 'keylogger:start',
  KEYLOGGER_STOP: 'keylogger:stop',
  KEYLOGGER_EVENT: 'keylogger:event',
  
  // Content Filter
  FILTER_CHECK: 'filter:check',
  FILTER_RULES_GET: 'filter:rules:get',
  FILTER_RULES_UPDATE: 'filter:rules:update',
  
  // Alerts
  ALERT_CREATE: 'alert:create',
  ALERT_LIST: 'alert:list',
  ALERT_MARK_READ: 'alert:mark-read',
  ALERT_DELETE: 'alert:delete',
  
  // Reports
  REPORT_DAILY: 'report:daily',
  REPORT_WEEKLY: 'report:weekly',
  REPORT_MONTHLY: 'report:monthly',
  REPORT_EXPORT: 'report:export',
  
  // System
  SYSTEM_STATS: 'system:stats',
  SYSTEM_CONFIG: 'system:config',
  SYSTEM_UPDATE_CONFIG: 'system:update-config',
  SYSTEM_BACKUP: 'system:backup',
  SYSTEM_RESTORE: 'system:restore',
  
  // Notifications
  NOTIFICATION_SEND: 'notification:send',
  NOTIFICATION_SETTINGS: 'notification:settings',
  
  // Window Management
  WINDOW_MINIMIZE: 'window:minimize',
  WINDOW_MAXIMIZE: 'window:maximize',
  WINDOW_CLOSE: 'window:close',
  WINDOW_HIDE: 'window:hide',
  WINDOW_SHOW: 'window:show'
} as const;

// Database Constants
export const DATABASE = {
  NAME: 'childguard.db',
  VERSION: 1,
  ENCRYPTION_ALGORITHM: 'aes-256-gcm',
  BACKUP_INTERVAL: 24 * 60 * 60 * 1000, // 24 hours in milliseconds
  MAX_BACKUP_FILES: 7,
  DEFAULT_RETENTION_DAYS: 90
} as const;

// Security Constants
export const SECURITY = {
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_REGEX: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/,
  SESSION_TIMEOUT: 30 * 60 * 1000, // 30 minutes in milliseconds
  MAX_LOGIN_ATTEMPTS: 5,
  LOCKOUT_DURATION: 15 * 60 * 1000, // 15 minutes in milliseconds
  SALT_ROUNDS: 12
} as const;

// Monitoring Constants
export const MONITORING = {
  KEYLOGGER_BUFFER_SIZE: 1000,
  UPDATE_INTERVAL: 1000, // 1 second in milliseconds
  MAX_TEXT_LENGTH: 10000,
  CONTEXT_WINDOW_SIZE: 50, // characters before and after detected content
  MIN_WORD_LENGTH: 3,
  MAX_LOG_FILE_SIZE: 100 * 1024 * 1024 // 100MB in bytes
} as const;

// Filter Categories
export const FILTER_CATEGORIES = {
  PROFANITY: 'profanity',
  VIOLENCE: 'violence',
  ADULT: 'adult',
  CYBERBULLYING: 'cyberbullying',
  DRUGS: 'drugs',
  HATE_SPEECH: 'hate_speech',
  SELF_HARM: 'self_harm',
  OTHER: 'other'
} as const;

// Severity Levels
export const SEVERITY_LEVELS = {
  LOW: 'low',
  MEDIUM: 'medium',
  HIGH: 'high',
  CRITICAL: 'critical'
} as const;

// Notification Types
export const NOTIFICATION_TYPES = {
  INFO: 'info',
  WARNING: 'warning',
  ERROR: 'error',
  CRITICAL: 'critical'
} as const;

// Time Constants
export const TIME = {
  MINUTE: 60 * 1000,
  HOUR: 60 * 60 * 1000,
  DAY: 24 * 60 * 60 * 1000,
  WEEK: 7 * 24 * 60 * 60 * 1000,
  MONTH: 30 * 24 * 60 * 60 * 1000
} as const;

// File Paths
export const PATHS = {
  USER_DATA: 'userData',
  LOGS: 'logs',
  BACKUPS: 'backups',
  TEMP: 'temp',
  CONFIG: 'config.json',
  DATABASE: 'childguard.db'
} as const;

// Default Settings
export const DEFAULT_SETTINGS = {
  MONITORING: {
    keyloggerEnabled: true,
    contentFilterEnabled: true,
    timeRestrictionsEnabled: false,
    applicationBlockingEnabled: false,
    filterSensitivity: 'medium' as const,
    notificationSettings: {
      realTimeAlerts: true,
      emailNotifications: false,
      dailyReports: true,
      weeklyReports: false
    }
  },
  CHILD: {
    restrictionLevel: 'medium' as const,
    allowedHours: {
      start: '08:00',
      end: '20:00'
    }
  }
} as const;

// Error Messages
export const ERROR_MESSAGES = {
  INVALID_CREDENTIALS: 'Invalid username or password',
  USER_NOT_FOUND: 'User not found',
  USER_ALREADY_EXISTS: 'User already exists',
  CHILD_NOT_FOUND: 'Child profile not found',
  UNAUTHORIZED: 'Unauthorized access',
  INVALID_INPUT: 'Invalid input provided',
  DATABASE_ERROR: 'Database operation failed',
  KEYLOGGER_ERROR: 'Keylogger service error',
  FILTER_ERROR: 'Content filter error',
  NOTIFICATION_ERROR: 'Notification service error',
  SYSTEM_ERROR: 'System error occurred'
} as const;

// Success Messages
export const SUCCESS_MESSAGES = {
  USER_CREATED: 'User created successfully',
  USER_UPDATED: 'User updated successfully',
  USER_DELETED: 'User deleted successfully',
  CHILD_CREATED: 'Child profile created successfully',
  CHILD_UPDATED: 'Child profile updated successfully',
  CHILD_DELETED: 'Child profile deleted successfully',
  SETTINGS_UPDATED: 'Settings updated successfully',
  MONITORING_STARTED: 'Monitoring started successfully',
  MONITORING_STOPPED: 'Monitoring stopped successfully',
  BACKUP_CREATED: 'Backup created successfully',
  DATA_EXPORTED: 'Data exported successfully'
} as const;

// Regular Expressions for Content Filtering
export const FILTER_PATTERNS = {
  EMAIL: /\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b/g,
  PHONE: /(\+\d{1,3}[- ]?)?\d{10}/g,
  URL: /https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/g,
  CREDIT_CARD: /\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b/g
} as const;

export type IPCChannel = typeof IPC_CHANNELS[keyof typeof IPC_CHANNELS];
export type FilterCategory = typeof FILTER_CATEGORIES[keyof typeof FILTER_CATEGORIES];
export type SeverityLevel = typeof SEVERITY_LEVELS[keyof typeof SEVERITY_LEVELS];
export type NotificationType = typeof NOTIFICATION_TYPES[keyof typeof NOTIFICATION_TYPES];
