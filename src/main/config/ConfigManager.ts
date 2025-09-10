import * as path from 'path';
import * as fs from 'fs';
import { app } from 'electron';
import { EventEmitter } from 'events';
import { Logger } from '../utils/Logger';
import type { AppConfig } from '@shared/types';
import { PATHS, DEFAULT_SETTINGS } from '@shared/constants';

export class ConfigManager extends EventEmitter {
  private config: AppConfig;
  private configPath: string;
  private logger: Logger;

  constructor() {
    super();
    this.logger = new Logger();
    this.configPath = path.join(app.getPath('userData'), PATHS.CONFIG);
    this.config = this.getDefaultConfig();
    this.loadConfig();
  }

  private getDefaultConfig(): AppConfig {
    return {
      version: '1.0.0',
      environment: process.env.NODE_ENV === 'development' ? 'development' : 'production',
      autoStart: false,
      minimizeToTray: true,
      database: {
        path: path.join(app.getPath('userData'), PATHS.DATABASE),
        encryptionKey: '',
        backupEnabled: true,
        retentionDays: 90
      },
      security: {
        sessionTimeout: 30, // minutes
        maxLoginAttempts: 5,
        passwordMinLength: 8
      },
      monitoring: {
        updateInterval: 1000, // milliseconds
        bufferSize: 1000,
        maxLogFileSize: 100 // MB
      }
    };
  }

  private loadConfig(): void {
    try {
      if (fs.existsSync(this.configPath)) {
        const configData = fs.readFileSync(this.configPath, 'utf8');
        const loadedConfig = JSON.parse(configData);
        
        // Merge with default config to ensure all properties exist
        this.config = this.mergeConfig(this.getDefaultConfig(), loadedConfig);
        
        this.logger.info('Configuration loaded successfully');
      } else {
        // Create default config file
        this.saveConfig();
        this.logger.info('Default configuration created');
      }
    } catch (error) {
      this.logger.error('Failed to load configuration:', error);
      this.config = this.getDefaultConfig();
    }
  }

  private mergeConfig(defaultConfig: AppConfig, loadedConfig: any): AppConfig {
    const merged = { ...defaultConfig };

    // Safely merge nested objects
    if (loadedConfig.database) {
      merged.database = { ...defaultConfig.database, ...loadedConfig.database };
    }

    if (loadedConfig.security) {
      merged.security = { ...defaultConfig.security, ...loadedConfig.security };
    }

    if (loadedConfig.monitoring) {
      merged.monitoring = { ...defaultConfig.monitoring, ...loadedConfig.monitoring };
    }

    // Merge top-level properties
    Object.keys(loadedConfig).forEach(key => {
      if (key !== 'database' && key !== 'security' && key !== 'monitoring') {
        (merged as any)[key] = loadedConfig[key];
      }
    });

    return merged;
  }

  public saveConfig(): void {
    try {
      const configData = JSON.stringify(this.config, null, 2);
      fs.writeFileSync(this.configPath, configData, 'utf8');
      
      this.logger.info('Configuration saved successfully');
      this.emit('config-saved', this.config);
    } catch (error) {
      this.logger.error('Failed to save configuration:', error);
      throw error;
    }
  }

  public getConfig(): AppConfig {
    return { ...this.config };
  }

  public updateConfig(updates: Partial<AppConfig>): void {
    try {
      const oldConfig = { ...this.config };
      
      // Update configuration
      this.config = this.mergeConfig(this.config, updates);
      
      // Save to file
      this.saveConfig();
      
      this.logger.info('Configuration updated');
      this.emit('config-updated', { oldConfig, newConfig: this.config });
    } catch (error) {
      this.logger.error('Failed to update configuration:', error);
      throw error;
    }
  }

  public resetToDefaults(): void {
    try {
      this.config = this.getDefaultConfig();
      this.saveConfig();
      
      this.logger.info('Configuration reset to defaults');
      this.emit('config-reset', this.config);
    } catch (error) {
      this.logger.error('Failed to reset configuration:', error);
      throw error;
    }
  }

  // Specific configuration getters
  public getDatabaseConfig() {
    return { ...this.config.database };
  }

  public getSecurityConfig() {
    return { ...this.config.security };
  }

  public getMonitoringConfig() {
    return { ...this.config.monitoring };
  }

  // Specific configuration setters
  public updateDatabaseConfig(updates: Partial<AppConfig['database']>): void {
    this.updateConfig({
      database: { ...this.config.database, ...updates }
    });
  }

  public updateSecurityConfig(updates: Partial<AppConfig['security']>): void {
    this.updateConfig({
      security: { ...this.config.security, ...updates }
    });
  }

  public updateMonitoringConfig(updates: Partial<AppConfig['monitoring']>): void {
    this.updateConfig({
      monitoring: { ...this.config.monitoring, ...updates }
    });
  }

  // Validation methods
  public validateConfig(config: Partial<AppConfig>): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    // Validate security settings
    if (config.security) {
      if (config.security.sessionTimeout && config.security.sessionTimeout < 5) {
        errors.push('Session timeout must be at least 5 minutes');
      }

      if (config.security.maxLoginAttempts && config.security.maxLoginAttempts < 3) {
        errors.push('Maximum login attempts must be at least 3');
      }

      if (config.security.passwordMinLength && config.security.passwordMinLength < 6) {
        errors.push('Password minimum length must be at least 6 characters');
      }
    }

    // Validate monitoring settings
    if (config.monitoring) {
      if (config.monitoring.updateInterval && config.monitoring.updateInterval < 100) {
        errors.push('Update interval must be at least 100 milliseconds');
      }

      if (config.monitoring.bufferSize && config.monitoring.bufferSize < 100) {
        errors.push('Buffer size must be at least 100');
      }

      if (config.monitoring.maxLogFileSize && config.monitoring.maxLogFileSize < 1) {
        errors.push('Maximum log file size must be at least 1 MB');
      }
    }

    // Validate database settings
    if (config.database) {
      if (config.database.retentionDays && config.database.retentionDays < 1) {
        errors.push('Data retention period must be at least 1 day');
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  public exportConfig(): string {
    return JSON.stringify(this.config, null, 2);
  }

  public importConfig(configJson: string): void {
    try {
      const importedConfig = JSON.parse(configJson);
      
      // Validate imported configuration
      const validation = this.validateConfig(importedConfig);
      if (!validation.isValid) {
        throw new Error(`Invalid configuration: ${validation.errors.join(', ')}`);
      }

      // Update configuration
      this.updateConfig(importedConfig);
      
      this.logger.info('Configuration imported successfully');
    } catch (error) {
      this.logger.error('Failed to import configuration:', error);
      throw error;
    }
  }

  public getConfigPath(): string {
    return this.configPath;
  }

  public backupConfig(): string {
    try {
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
      const backupPath = path.join(
        app.getPath('userData'), 
        'backups', 
        `config-backup-${timestamp}.json`
      );

      // Ensure backup directory exists
      const backupDir = path.dirname(backupPath);
      if (!fs.existsSync(backupDir)) {
        fs.mkdirSync(backupDir, { recursive: true });
      }

      // Create backup
      fs.writeFileSync(backupPath, this.exportConfig(), 'utf8');
      
      this.logger.info(`Configuration backed up to: ${backupPath}`);
      return backupPath;
    } catch (error) {
      this.logger.error('Failed to backup configuration:', error);
      throw error;
    }
  }

  public restoreConfig(backupPath: string): void {
    try {
      if (!fs.existsSync(backupPath)) {
        throw new Error('Backup file not found');
      }

      const backupData = fs.readFileSync(backupPath, 'utf8');
      this.importConfig(backupData);
      
      this.logger.info(`Configuration restored from: ${backupPath}`);
    } catch (error) {
      this.logger.error('Failed to restore configuration:', error);
      throw error;
    }
  }
}
