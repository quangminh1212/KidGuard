import { Notification, shell } from 'electron';
import * as nodemailer from 'nodemailer';
import { EventEmitter } from 'events';
import { Logger } from '../utils/Logger';
import type { Alert, NotificationPayload } from '@shared/types';
import { NOTIFICATION_TYPES } from '@shared/constants';

interface EmailConfig {
  enabled: boolean;
  host: string;
  port: number;
  secure: boolean;
  auth: {
    user: string;
    pass: string;
  };
  from: string;
  to: string[];
}

export class NotificationService extends EventEmitter {
  private logger: Logger;
  private emailTransporter: nodemailer.Transporter | null = null;
  private emailConfig: EmailConfig | null = null;
  private isInitialized = false;

  constructor() {
    super();
    this.logger = new Logger();
  }

  public async initialize(): Promise<void> {
    try {
      // Load email configuration (would typically come from settings)
      await this.loadEmailConfig();
      
      if (this.emailConfig?.enabled) {
        await this.initializeEmailTransporter();
      }

      this.isInitialized = true;
      this.logger.info('Notification service initialized successfully');
    } catch (error) {
      this.logger.error('Failed to initialize notification service:', error);
      throw error;
    }
  }

  private async loadEmailConfig(): Promise<void> {
    // Default email configuration - would be loaded from user settings
    this.emailConfig = {
      enabled: false, // Disabled by default
      host: 'smtp.gmail.com',
      port: 587,
      secure: false,
      auth: {
        user: '',
        pass: ''
      },
      from: 'childguard@example.com',
      to: []
    };
  }

  private async initializeEmailTransporter(): Promise<void> {
    if (!this.emailConfig) {
      throw new Error('Email configuration not loaded');
    }

    try {
      this.emailTransporter = nodemailer.createTransporter({
        host: this.emailConfig.host,
        port: this.emailConfig.port,
        secure: this.emailConfig.secure,
        auth: {
          user: this.emailConfig.auth.user,
          pass: this.emailConfig.auth.pass
        }
      });

      // Verify connection
      await this.emailTransporter.verify();
      this.logger.info('Email transporter initialized and verified');
    } catch (error) {
      this.logger.error('Failed to initialize email transporter:', error);
      this.emailTransporter = null;
    }
  }

  public async showNotification(payload: NotificationPayload): Promise<void> {
    if (!this.isInitialized) {
      throw new Error('Notification service not initialized');
    }

    try {
      // Check if notifications are supported
      if (!Notification.isSupported()) {
        this.logger.warn('System notifications are not supported');
        return;
      }

      const notification = new Notification({
        title: payload.title,
        body: payload.body,
        icon: payload.icon || this.getDefaultIcon(),
        urgency: payload.urgency || 'normal',
        silent: false,
        timeoutType: 'default'
      });

      // Handle notification click
      notification.on('click', () => {
        this.emit('notification-clicked', payload);
        shell.beep(); // Optional sound
      });

      // Handle notification actions
      if (payload.actions) {
        notification.on('action', (event, index) => {
          const action = payload.actions![index];
          this.emit('notification-action', { action, payload });
        });
      }

      notification.show();
      this.logger.info(`Notification shown: ${payload.title}`);
    } catch (error) {
      this.logger.error('Failed to show notification:', error);
    }
  }

  public async sendAlert(alert: Partial<Alert>): Promise<void> {
    if (!this.isInitialized) {
      throw new Error('Notification service not initialized');
    }

    try {
      // Show system notification
      await this.showNotification({
        title: alert.title || 'ChildGuard Alert',
        body: alert.message || 'An alert has been triggered',
        urgency: this.mapSeverityToUrgency(alert.severity),
        icon: this.getAlertIcon(alert.type)
      });

      // Send email notification if enabled and configured
      if (this.emailConfig?.enabled && this.emailTransporter && this.emailConfig.to.length > 0) {
        await this.sendEmailAlert(alert);
      }

      this.emit('alert-sent', alert);
      this.logger.info(`Alert sent: ${alert.type} - ${alert.severity}`);
    } catch (error) {
      this.logger.error('Failed to send alert:', error);
    }
  }

  private async sendEmailAlert(alert: Partial<Alert>): Promise<void> {
    if (!this.emailTransporter || !this.emailConfig) {
      return;
    }

    try {
      const emailSubject = `ChildGuard Alert: ${alert.title}`;
      const emailBody = this.generateEmailBody(alert);

      const mailOptions = {
        from: this.emailConfig.from,
        to: this.emailConfig.to,
        subject: emailSubject,
        html: emailBody
      };

      await this.emailTransporter.sendMail(mailOptions);
      this.logger.info('Email alert sent successfully');
    } catch (error) {
      this.logger.error('Failed to send email alert:', error);
    }
  }

  private generateEmailBody(alert: Partial<Alert>): string {
    const timestamp = alert.timestamp ? alert.timestamp.toLocaleString() : new Date().toLocaleString();
    
    return `
      <!DOCTYPE html>
      <html>
      <head>
        <style>
          body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
          .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
          .header { background-color: #1976d2; color: white; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
          .alert-info { background-color: #f8f9fa; padding: 15px; border-left: 4px solid #dc3545; margin: 15px 0; }
          .severity-${alert.severity} { border-left-color: ${this.getSeverityColor(alert.severity)}; }
          .footer { margin-top: 20px; padding-top: 15px; border-top: 1px solid #eee; font-size: 12px; color: #666; }
        </style>
      </head>
      <body>
        <div class="container">
          <div class="header">
            <h2>ChildGuard Security Alert</h2>
          </div>
          
          <div class="alert-info severity-${alert.severity}">
            <h3>${alert.title}</h3>
            <p><strong>Message:</strong> ${alert.message}</p>
            <p><strong>Type:</strong> ${alert.type}</p>
            <p><strong>Severity:</strong> ${alert.severity}</p>
            <p><strong>Time:</strong> ${timestamp}</p>
            ${alert.childId ? `<p><strong>Child ID:</strong> ${alert.childId}</p>` : ''}
          </div>

          <div class="footer">
            <p>This alert was generated by ChildGuard Child Protection System.</p>
            <p>Please review the alert and take appropriate action if necessary.</p>
          </div>
        </div>
      </body>
      </html>
    `;
  }

  private mapSeverityToUrgency(severity?: string): 'low' | 'normal' | 'critical' {
    switch (severity) {
      case 'critical':
        return 'critical';
      case 'high':
      case 'error':
        return 'critical';
      case 'medium':
      case 'warning':
        return 'normal';
      default:
        return 'low';
    }
  }

  private getSeverityColor(severity?: string): string {
    switch (severity) {
      case 'critical': return '#dc3545';
      case 'error': return '#dc3545';
      case 'warning': return '#ffc107';
      case 'info': return '#17a2b8';
      default: return '#6c757d';
    }
  }

  private getDefaultIcon(): string {
    return process.platform === 'win32' 
      ? 'assets/icon.ico' 
      : 'assets/icon.png';
  }

  private getAlertIcon(alertType?: string): string {
    switch (alertType) {
      case 'inappropriate_content':
        return 'assets/warning-icon.png';
      case 'time_violation':
        return 'assets/time-icon.png';
      case 'blocked_application':
        return 'assets/block-icon.png';
      default:
        return this.getDefaultIcon();
    }
  }

  public async updateEmailConfig(config: Partial<EmailConfig>): Promise<void> {
    if (this.emailConfig) {
      this.emailConfig = { ...this.emailConfig, ...config };
      
      if (config.enabled && this.emailConfig.enabled) {
        await this.initializeEmailTransporter();
      } else if (!config.enabled) {
        this.emailTransporter = null;
      }
      
      this.logger.info('Email configuration updated');
    }
  }

  public async testEmailConfiguration(): Promise<boolean> {
    if (!this.emailTransporter) {
      return false;
    }

    try {
      await this.emailTransporter.verify();
      
      // Send test email
      if (this.emailConfig?.to.length) {
        await this.emailTransporter.sendMail({
          from: this.emailConfig.from,
          to: this.emailConfig.to[0],
          subject: 'ChildGuard Email Test',
          text: 'This is a test email from ChildGuard. Email notifications are working correctly.'
        });
      }
      
      return true;
    } catch (error) {
      this.logger.error('Email configuration test failed:', error);
      return false;
    }
  }

  public getEmailConfig(): EmailConfig | null {
    return this.emailConfig ? { ...this.emailConfig } : null;
  }

  public async sendDailyReport(childId: string, reportData: any): Promise<void> {
    if (!this.emailConfig?.enabled || !this.emailTransporter) {
      return;
    }

    try {
      const emailSubject = `ChildGuard Daily Report - ${new Date().toDateString()}`;
      const emailBody = this.generateDailyReportEmail(childId, reportData);

      await this.emailTransporter.sendMail({
        from: this.emailConfig.from,
        to: this.emailConfig.to,
        subject: emailSubject,
        html: emailBody
      });

      this.logger.info(`Daily report sent for child: ${childId}`);
    } catch (error) {
      this.logger.error('Failed to send daily report:', error);
    }
  }

  private generateDailyReportEmail(childId: string, reportData: any): string {
    return `
      <!DOCTYPE html>
      <html>
      <head>
        <style>
          body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
          .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; }
          .header { background-color: #1976d2; color: white; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
          .stats { display: flex; justify-content: space-between; margin: 20px 0; }
          .stat-item { text-align: center; padding: 10px; background-color: #f8f9fa; border-radius: 4px; }
        </style>
      </head>
      <body>
        <div class="container">
          <div class="header">
            <h2>Daily Activity Report</h2>
            <p>Child ID: ${childId} | Date: ${new Date().toDateString()}</p>
          </div>
          
          <div class="stats">
            <div class="stat-item">
              <h3>${reportData.totalKeystrokes || 0}</h3>
              <p>Total Keystrokes</p>
            </div>
            <div class="stat-item">
              <h3>${reportData.alertsCount || 0}</h3>
              <p>Alerts Generated</p>
            </div>
            <div class="stat-item">
              <h3>${reportData.activeTime || 0}m</h3>
              <p>Active Time</p>
            </div>
          </div>

          <p>This is an automated daily report from ChildGuard.</p>
        </div>
      </body>
      </html>
    `;
  }
}
