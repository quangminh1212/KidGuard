import * as Database from 'better-sqlite3';
import * as path from 'path';
import * as fs from 'fs';
import * as crypto from 'crypto';
import { app } from 'electron';
import { Logger } from '../utils/Logger';
import type { 
  User, 
  Child, 
  KeystrokeEvent, 
  InappropriateContent, 
  Alert, 
  MonitoringSettings,
  SystemStats 
} from '@shared/types';
import { DATABASE } from '@shared/constants';

export class DatabaseManager {
  private db: Database.Database | null = null;
  private logger: Logger;
  private dbPath: string;
  private encryptionKey: string;

  constructor() {
    this.logger = new Logger();
    this.dbPath = path.join(app.getPath('userData'), DATABASE.NAME);
    this.encryptionKey = this.generateEncryptionKey();
  }

  public async initialize(): Promise<void> {
    try {
      // Ensure user data directory exists
      const userDataPath = app.getPath('userData');
      if (!fs.existsSync(userDataPath)) {
        fs.mkdirSync(userDataPath, { recursive: true });
      }

      // Initialize database
      this.db = new Database(this.dbPath);
      this.db.pragma('journal_mode = WAL');
      this.db.pragma('foreign_keys = ON');

      // Create tables
      await this.createTables();
      
      // Create default admin user if none exists
      await this.createDefaultUser();

      this.logger.info('Database initialized successfully');
    } catch (error) {
      this.logger.error('Failed to initialize database:', error);
      throw error;
    }
  }

  private generateEncryptionKey(): string {
    // In production, this should be derived from user password or stored securely
    return crypto.randomBytes(32).toString('hex');
  }

  private encrypt(text: string): string {
    const cipher = crypto.createCipher(DATABASE.ENCRYPTION_ALGORITHM, this.encryptionKey);
    let encrypted = cipher.update(text, 'utf8', 'hex');
    encrypted += cipher.final('hex');
    return encrypted;
  }

  private decrypt(encryptedText: string): string {
    const decipher = crypto.createDecipher(DATABASE.ENCRYPTION_ALGORITHM, this.encryptionKey);
    let decrypted = decipher.update(encryptedText, 'hex', 'utf8');
    decrypted += decipher.final('utf8');
    return decrypted;
  }

  private async createTables(): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    const tables = [
      // Users table
      `CREATE TABLE IF NOT EXISTS users (
        id TEXT PRIMARY KEY,
        username TEXT UNIQUE NOT NULL,
        email TEXT UNIQUE NOT NULL,
        password_hash TEXT NOT NULL,
        role TEXT NOT NULL CHECK (role IN ('parent', 'child')),
        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        last_login DATETIME,
        is_active BOOLEAN DEFAULT 1
      )`,

      // Children table
      `CREATE TABLE IF NOT EXISTS children (
        id TEXT PRIMARY KEY,
        name TEXT NOT NULL,
        age INTEGER NOT NULL,
        parent_id TEXT NOT NULL,
        monitoring_enabled BOOLEAN DEFAULT 1,
        restriction_level TEXT DEFAULT 'medium' CHECK (restriction_level IN ('low', 'medium', 'high')),
        allowed_hours_start TEXT DEFAULT '08:00',
        allowed_hours_end TEXT DEFAULT '20:00',
        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (parent_id) REFERENCES users(id) ON DELETE CASCADE
      )`,

      // Keystroke events table
      `CREATE TABLE IF NOT EXISTS keystroke_events (
        id TEXT PRIMARY KEY,
        child_id TEXT NOT NULL,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
        text_encrypted TEXT NOT NULL,
        application_name TEXT,
        window_title TEXT,
        is_filtered BOOLEAN DEFAULT 0,
        severity TEXT CHECK (severity IN ('low', 'medium', 'high', 'critical')),
        FOREIGN KEY (child_id) REFERENCES children(id) ON DELETE CASCADE
      )`,

      // Inappropriate content table
      `CREATE TABLE IF NOT EXISTS inappropriate_content (
        id TEXT PRIMARY KEY,
        keystroke_event_id TEXT NOT NULL,
        detected_words TEXT NOT NULL,
        context_encrypted TEXT NOT NULL,
        severity TEXT NOT NULL CHECK (severity IN ('low', 'medium', 'high', 'critical')),
        category TEXT NOT NULL,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
        is_reviewed BOOLEAN DEFAULT 0,
        parent_notified BOOLEAN DEFAULT 0,
        FOREIGN KEY (keystroke_event_id) REFERENCES keystroke_events(id) ON DELETE CASCADE
      )`,

      // Alerts table
      `CREATE TABLE IF NOT EXISTS alerts (
        id TEXT PRIMARY KEY,
        child_id TEXT NOT NULL,
        type TEXT NOT NULL,
        title TEXT NOT NULL,
        message TEXT NOT NULL,
        severity TEXT NOT NULL CHECK (severity IN ('info', 'warning', 'error', 'critical')),
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
        is_read BOOLEAN DEFAULT 0,
        action_taken TEXT,
        FOREIGN KEY (child_id) REFERENCES children(id) ON DELETE CASCADE
      )`,

      // Monitoring settings table
      `CREATE TABLE IF NOT EXISTS monitoring_settings (
        child_id TEXT PRIMARY KEY,
        keylogger_enabled BOOLEAN DEFAULT 1,
        content_filter_enabled BOOLEAN DEFAULT 1,
        time_restrictions_enabled BOOLEAN DEFAULT 0,
        application_blocking_enabled BOOLEAN DEFAULT 0,
        real_time_alerts BOOLEAN DEFAULT 1,
        email_notifications BOOLEAN DEFAULT 0,
        daily_reports BOOLEAN DEFAULT 1,
        weekly_reports BOOLEAN DEFAULT 0,
        filter_sensitivity TEXT DEFAULT 'medium' CHECK (filter_sensitivity IN ('low', 'medium', 'high')),
        blocked_applications TEXT DEFAULT '[]',
        allowed_websites TEXT DEFAULT '[]',
        blocked_websites TEXT DEFAULT '[]',
        FOREIGN KEY (child_id) REFERENCES children(id) ON DELETE CASCADE
      )`,

      // System stats table
      `CREATE TABLE IF NOT EXISTS system_stats (
        id INTEGER PRIMARY KEY,
        date DATE DEFAULT CURRENT_DATE,
        total_keystrokes INTEGER DEFAULT 0,
        alerts_count INTEGER DEFAULT 0,
        blocked_attempts INTEGER DEFAULT 0,
        active_monitoring_time INTEGER DEFAULT 0,
        last_update DATETIME DEFAULT CURRENT_TIMESTAMP
      )`
    ];

    for (const tableSQL of tables) {
      this.db.exec(tableSQL);
    }

    // Create indexes for better performance
    const indexes = [
      'CREATE INDEX IF NOT EXISTS idx_keystroke_events_child_timestamp ON keystroke_events(child_id, timestamp)',
      'CREATE INDEX IF NOT EXISTS idx_alerts_child_timestamp ON alerts(child_id, timestamp)',
      'CREATE INDEX IF NOT EXISTS idx_inappropriate_content_timestamp ON inappropriate_content(timestamp)',
      'CREATE INDEX IF NOT EXISTS idx_system_stats_date ON system_stats(date)'
    ];

    for (const indexSQL of indexes) {
      this.db.exec(indexSQL);
    }

    this.logger.info('Database tables created successfully');
  }

  private async createDefaultUser(): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    const existingUsers = this.db.prepare('SELECT COUNT(*) as count FROM users').get() as { count: number };
    
    if (existingUsers.count === 0) {
      const bcrypt = require('bcrypt');
      const defaultPassword = 'admin123';
      const hashedPassword = await bcrypt.hash(defaultPassword, 12);

      const userId = crypto.randomUUID();
      this.db.prepare(`
        INSERT INTO users (id, username, email, password_hash, role)
        VALUES (?, ?, ?, ?, ?)
      `).run(userId, 'admin', 'admin@childguard.com', hashedPassword, 'parent');

      this.logger.info('Default admin user created (username: admin, password: admin123)');
    }
  }

  // User management methods
  public async createUser(userData: Partial<User>): Promise<User> {
    if (!this.db) throw new Error('Database not initialized');

    const bcrypt = require('bcrypt');
    const userId = crypto.randomUUID();
    const hashedPassword = await bcrypt.hash(userData.password, 12);

    const user: User = {
      id: userId,
      username: userData.username!,
      email: userData.email!,
      role: userData.role!,
      createdAt: new Date(),
      isActive: true
    };

    this.db.prepare(`
      INSERT INTO users (id, username, email, password_hash, role, created_at, is_active)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `).run(user.id, user.username, user.email, hashedPassword, user.role, user.createdAt.toISOString(), user.isActive);

    this.logger.info(`User created: ${user.username}`);
    return user;
  }

  public async getUserByCredentials(username: string, password: string): Promise<User | null> {
    if (!this.db) throw new Error('Database not initialized');

    const bcrypt = require('bcrypt');
    const userRow = this.db.prepare('SELECT * FROM users WHERE username = ? AND is_active = 1').get(username) as any;

    if (!userRow) return null;

    const isValidPassword = await bcrypt.compare(password, userRow.password_hash);
    if (!isValidPassword) return null;

    // Update last login
    this.db.prepare('UPDATE users SET last_login = ? WHERE id = ?').run(new Date().toISOString(), userRow.id);

    return {
      id: userRow.id,
      username: userRow.username,
      email: userRow.email,
      role: userRow.role,
      createdAt: new Date(userRow.created_at),
      lastLogin: new Date(),
      isActive: userRow.is_active === 1
    };
  }

  // Child management methods
  public async createChild(childData: Partial<Child>): Promise<Child> {
    if (!this.db) throw new Error('Database not initialized');

    const childId = crypto.randomUUID();
    const child: Child = {
      id: childId,
      name: childData.name!,
      age: childData.age!,
      parentId: childData.parentId!,
      monitoringEnabled: childData.monitoringEnabled ?? true,
      restrictionLevel: childData.restrictionLevel ?? 'medium',
      allowedHours: childData.allowedHours ?? { start: '08:00', end: '20:00' },
      createdAt: new Date()
    };

    this.db.prepare(`
      INSERT INTO children (id, name, age, parent_id, monitoring_enabled, restriction_level, allowed_hours_start, allowed_hours_end, created_at)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `).run(
      child.id, child.name, child.age, child.parentId, child.monitoringEnabled,
      child.restrictionLevel, child.allowedHours.start, child.allowedHours.end, child.createdAt.toISOString()
    );

    // Create default monitoring settings
    this.db.prepare(`
      INSERT INTO monitoring_settings (child_id) VALUES (?)
    `).run(child.id);

    this.logger.info(`Child profile created: ${child.name}`);
    return child;
  }

  public async getChildrenByParent(parentId: string): Promise<Child[]> {
    if (!this.db) throw new Error('Database not initialized');

    const rows = this.db.prepare('SELECT * FROM children WHERE parent_id = ?').all(parentId) as any[];
    
    return rows.map(row => ({
      id: row.id,
      name: row.name,
      age: row.age,
      parentId: row.parent_id,
      monitoringEnabled: row.monitoring_enabled === 1,
      restrictionLevel: row.restriction_level,
      allowedHours: {
        start: row.allowed_hours_start,
        end: row.allowed_hours_end
      },
      createdAt: new Date(row.created_at)
    }));
  }

  // Keystroke event methods
  public async saveKeystrokeEvent(eventData: Partial<KeystrokeEvent>): Promise<KeystrokeEvent> {
    if (!this.db) throw new Error('Database not initialized');

    const eventId = crypto.randomUUID();
    const encryptedText = this.encrypt(eventData.text!);

    const event: KeystrokeEvent = {
      id: eventId,
      childId: eventData.childId!,
      timestamp: eventData.timestamp || new Date(),
      text: eventData.text!,
      applicationName: eventData.applicationName || '',
      windowTitle: eventData.windowTitle || '',
      isFiltered: eventData.isFiltered || false,
      severity: eventData.severity
    };

    this.db.prepare(`
      INSERT INTO keystroke_events (id, child_id, timestamp, text_encrypted, application_name, window_title, is_filtered, severity)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    `).run(
      event.id, event.childId, event.timestamp.toISOString(), encryptedText,
      event.applicationName, event.windowTitle, event.isFiltered, event.severity
    );

    this.logger.info(`Keystroke event saved: ${event.id}`);
    return event;
  }

  // Alert methods
  public async createAlert(alertData: Partial<Alert>): Promise<Alert> {
    if (!this.db) throw new Error('Database not initialized');

    const alertId = crypto.randomUUID();
    const alert: Alert = {
      id: alertId,
      childId: alertData.childId!,
      type: alertData.type!,
      title: alertData.title!,
      message: alertData.message!,
      severity: alertData.severity!,
      timestamp: alertData.timestamp || new Date(),
      isRead: false
    };

    this.db.prepare(`
      INSERT INTO alerts (id, child_id, type, title, message, severity, timestamp, is_read)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    `).run(
      alert.id, alert.childId, alert.type, alert.title, alert.message,
      alert.severity, alert.timestamp.toISOString(), alert.isRead
    );

    this.logger.info(`Alert created: ${alert.type} - ${alert.severity}`);
    return alert;
  }

  public async getSystemStats(): Promise<SystemStats> {
    if (!this.db) throw new Error('Database not initialized');

    const today = new Date().toISOString().split('T')[0];
    const stats = this.db.prepare(`
      SELECT 
        COALESCE(SUM(total_keystrokes), 0) as totalKeystrokesToday,
        COALESCE(SUM(alerts_count), 0) as alertsToday,
        COALESCE(SUM(blocked_attempts), 0) as blockedAttemptsToday,
        COALESCE(SUM(active_monitoring_time), 0) as activeMonitoringTime
      FROM system_stats 
      WHERE date = ?
    `).get(today) as any;

    return {
      totalKeystrokesToday: stats.totalKeystrokesToday || 0,
      alertsToday: stats.alertsToday || 0,
      blockedAttemptsToday: stats.blockedAttemptsToday || 0,
      activeMonitoringTime: stats.activeMonitoringTime || 0,
      lastUpdateTime: new Date()
    };
  }

  public async close(): Promise<void> {
    if (this.db) {
      this.db.close();
      this.db = null;
      this.logger.info('Database connection closed');
    }
  }
}
