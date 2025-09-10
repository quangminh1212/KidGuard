// Core Types for ChildGuard Application

export interface User {
  id: string;
  username: string;
  email: string;
  role: 'parent' | 'child';
  createdAt: Date;
  lastLogin?: Date;
  isActive: boolean;
}

export interface Child {
  id: string;
  name: string;
  age: number;
  parentId: string;
  monitoringEnabled: boolean;
  restrictionLevel: 'low' | 'medium' | 'high';
  allowedHours: {
    start: string; // HH:MM format
    end: string;   // HH:MM format
  };
  createdAt: Date;
}

export interface KeystrokeEvent {
  id: string;
  childId: string;
  timestamp: Date;
  text: string;
  applicationName: string;
  windowTitle: string;
  isFiltered: boolean;
  severity?: 'low' | 'medium' | 'high' | 'critical';
}

export interface InappropriateContent {
  id: string;
  keystrokeEventId: string;
  detectedWords: string[];
  context: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  category: 'profanity' | 'violence' | 'adult' | 'cyberbullying' | 'drugs' | 'other';
  timestamp: Date;
  isReviewed: boolean;
  parentNotified: boolean;
}

export interface Alert {
  id: string;
  childId: string;
  type: 'inappropriate_content' | 'time_violation' | 'blocked_application' | 'system_event';
  title: string;
  message: string;
  severity: 'info' | 'warning' | 'error' | 'critical';
  timestamp: Date;
  isRead: boolean;
  actionTaken?: string;
}

export interface MonitoringSettings {
  childId: string;
  keyloggerEnabled: boolean;
  contentFilterEnabled: boolean;
  timeRestrictionsEnabled: boolean;
  applicationBlockingEnabled: boolean;
  notificationSettings: {
    realTimeAlerts: boolean;
    emailNotifications: boolean;
    dailyReports: boolean;
    weeklyReports: boolean;
  };
  filterSensitivity: 'low' | 'medium' | 'high';
  blockedApplications: string[];
  allowedWebsites: string[];
  blockedWebsites: string[];
}

export interface SystemStats {
  totalKeystrokesToday: number;
  alertsToday: number;
  blockedAttemptsToday: number;
  activeMonitoringTime: number; // in minutes
  lastUpdateTime: Date;
}

export interface DatabaseConfig {
  path: string;
  encryptionKey: string;
  backupEnabled: boolean;
  retentionDays: number;
}

export interface AppConfig {
  version: string;
  environment: 'development' | 'production';
  autoStart: boolean;
  minimizeToTray: boolean;
  database: DatabaseConfig;
  security: {
    sessionTimeout: number; // in minutes
    maxLoginAttempts: number;
    passwordMinLength: number;
  };
  monitoring: {
    updateInterval: number; // in milliseconds
    bufferSize: number;
    maxLogFileSize: number; // in MB
  };
}

// IPC Communication Types
export interface IPCMessage<T = any> {
  channel: string;
  data: T;
  requestId?: string;
}

export interface IPCResponse<T = any> {
  success: boolean;
  data?: T;
  error?: string;
  requestId?: string;
}

// API Response Types
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

// Filter Engine Types
export interface FilterRule {
  id: string;
  pattern: string;
  category: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  isRegex: boolean;
  isActive: boolean;
  language: string;
  createdAt: Date;
}

export interface FilterResult {
  isFiltered: boolean;
  matchedRules: FilterRule[];
  severity: 'low' | 'medium' | 'high' | 'critical';
  confidence: number; // 0-1
}

// Notification Types
export interface NotificationPayload {
  title: string;
  body: string;
  icon?: string;
  urgency?: 'low' | 'normal' | 'critical';
  actions?: Array<{
    type: string;
    text: string;
  }>;
}

// Export all types for easy importing
export type {
  User,
  Child,
  KeystrokeEvent,
  InappropriateContent,
  Alert,
  MonitoringSettings,
  SystemStats,
  DatabaseConfig,
  AppConfig,
  IPCMessage,
  IPCResponse,
  ApiResponse,
  FilterRule,
  FilterResult,
  NotificationPayload
};
