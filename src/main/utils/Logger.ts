import * as winston from 'winston';
import * as path from 'path';
import { app } from 'electron';
import * as fs from 'fs';

export class Logger {
  private logger: winston.Logger;
  private logDir: string;

  constructor() {
    this.logDir = path.join(app.getPath('userData'), 'logs');
    this.ensureLogDirectory();
    this.initializeLogger();
  }

  private ensureLogDirectory(): void {
    if (!fs.existsSync(this.logDir)) {
      fs.mkdirSync(this.logDir, { recursive: true });
    }
  }

  private initializeLogger(): void {
    // Define log format
    const logFormat = winston.format.combine(
      winston.format.timestamp({
        format: 'YYYY-MM-DD HH:mm:ss'
      }),
      winston.format.errors({ stack: true }),
      winston.format.printf(({ level, message, timestamp, stack }) => {
        if (stack) {
          return `${timestamp} [${level.toUpperCase()}]: ${message}\n${stack}`;
        }
        return `${timestamp} [${level.toUpperCase()}]: ${message}`;
      })
    );

    // Create logger instance
    this.logger = winston.createLogger({
      level: process.env.NODE_ENV === 'development' ? 'debug' : 'info',
      format: logFormat,
      transports: [
        // Console transport for development
        new winston.transports.Console({
          format: winston.format.combine(
            winston.format.colorize(),
            logFormat
          )
        }),

        // File transport for all logs
        new winston.transports.File({
          filename: path.join(this.logDir, 'childguard.log'),
          maxsize: 10 * 1024 * 1024, // 10MB
          maxFiles: 5,
          tailable: true
        }),

        // Separate file for errors
        new winston.transports.File({
          filename: path.join(this.logDir, 'error.log'),
          level: 'error',
          maxsize: 10 * 1024 * 1024, // 10MB
          maxFiles: 3,
          tailable: true
        }),

        // Security events log
        new winston.transports.File({
          filename: path.join(this.logDir, 'security.log'),
          level: 'warn',
          maxsize: 5 * 1024 * 1024, // 5MB
          maxFiles: 10,
          tailable: true
        })
      ],

      // Handle uncaught exceptions
      exceptionHandlers: [
        new winston.transports.File({
          filename: path.join(this.logDir, 'exceptions.log')
        })
      ],

      // Handle unhandled promise rejections
      rejectionHandlers: [
        new winston.transports.File({
          filename: path.join(this.logDir, 'rejections.log')
        })
      ]
    });

    // Log startup message
    this.info('Logger initialized successfully');
  }

  public debug(message: string, meta?: any): void {
    this.logger.debug(message, meta);
  }

  public info(message: string, meta?: any): void {
    this.logger.info(message, meta);
  }

  public warn(message: string, meta?: any): void {
    this.logger.warn(message, meta);
  }

  public error(message: string, error?: any): void {
    if (error instanceof Error) {
      this.logger.error(message, { error: error.message, stack: error.stack });
    } else {
      this.logger.error(message, { error });
    }
  }

  public security(message: string, meta?: any): void {
    this.logger.warn(`[SECURITY] ${message}`, meta);
  }

  public audit(action: string, userId?: string, details?: any): void {
    this.logger.info(`[AUDIT] ${action}`, {
      userId,
      timestamp: new Date().toISOString(),
      details
    });
  }

  public performance(operation: string, duration: number, meta?: any): void {
    this.logger.info(`[PERFORMANCE] ${operation} completed in ${duration}ms`, meta);
  }

  public getLogPath(): string {
    return this.logDir;
  }

  public async getRecentLogs(lines: number = 100): Promise<string[]> {
    const logFile = path.join(this.logDir, 'childguard.log');
    
    try {
      const content = fs.readFileSync(logFile, 'utf8');
      const logLines = content.split('\n').filter(line => line.trim());
      return logLines.slice(-lines);
    } catch (error) {
      this.error('Failed to read log file:', error);
      return [];
    }
  }

  public async getErrorLogs(lines: number = 50): Promise<string[]> {
    const errorLogFile = path.join(this.logDir, 'error.log');
    
    try {
      const content = fs.readFileSync(errorLogFile, 'utf8');
      const logLines = content.split('\n').filter(line => line.trim());
      return logLines.slice(-lines);
    } catch (error) {
      return [];
    }
  }

  public async clearLogs(): Promise<void> {
    try {
      const logFiles = ['childguard.log', 'error.log', 'security.log'];
      
      for (const file of logFiles) {
        const filePath = path.join(this.logDir, file);
        if (fs.existsSync(filePath)) {
          fs.writeFileSync(filePath, '');
        }
      }
      
      this.info('Log files cleared');
    } catch (error) {
      this.error('Failed to clear log files:', error);
    }
  }

  public createChildLogger(context: string): winston.Logger {
    return this.logger.child({ context });
  }
}
