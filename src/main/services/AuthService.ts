import { EventEmitter } from 'events';
import * as crypto from 'crypto';
import { DatabaseManager } from '../database/DatabaseManager';
import { Logger } from '../utils/Logger';
import type { User, IPCResponse } from '@shared/types';
import { SECURITY, ERROR_MESSAGES, SUCCESS_MESSAGES } from '@shared/constants';

interface LoginAttempt {
  username: string;
  timestamp: Date;
  success: boolean;
  ipAddress?: string;
}

interface Session {
  userId: string;
  sessionId: string;
  createdAt: Date;
  lastActivity: Date;
  isActive: boolean;
}

export class AuthService extends EventEmitter {
  private logger: Logger;
  private currentSession: Session | null = null;
  private loginAttempts: Map<string, LoginAttempt[]> = new Map();
  private lockedAccounts: Map<string, Date> = new Map();
  private sessionTimeout: NodeJS.Timeout | null = null;

  constructor(private databaseManager: DatabaseManager) {
    super();
    this.logger = new Logger();
  }

  public async login(credentials: { username: string; password: string }): Promise<IPCResponse<User>> {
    try {
      const { username, password } = credentials;

      // Validate input
      if (!username || !password) {
        return {
          success: false,
          error: ERROR_MESSAGES.INVALID_INPUT
        };
      }

      // Check if account is locked
      if (this.isAccountLocked(username)) {
        const lockTime = this.lockedAccounts.get(username);
        const remainingTime = lockTime ? Math.ceil((lockTime.getTime() - Date.now()) / 1000 / 60) : 0;
        
        return {
          success: false,
          error: `Account locked. Try again in ${remainingTime} minutes.`
        };
      }

      // Attempt authentication
      const user = await this.databaseManager.getUserByCredentials(username, password);

      if (!user) {
        // Record failed attempt
        this.recordLoginAttempt(username, false);
        
        // Check if account should be locked
        if (this.shouldLockAccount(username)) {
          this.lockAccount(username);
        }

        return {
          success: false,
          error: ERROR_MESSAGES.INVALID_CREDENTIALS
        };
      }

      // Check if user is active
      if (!user.isActive) {
        return {
          success: false,
          error: 'Account is disabled. Please contact administrator.'
        };
      }

      // Record successful attempt
      this.recordLoginAttempt(username, true);
      
      // Clear any existing locks for this user
      this.lockedAccounts.delete(username);

      // Create session
      const session = this.createSession(user.id);
      this.currentSession = session;

      // Set session timeout
      this.setSessionTimeout();

      this.logger.info(`User logged in successfully: ${username}`);
      this.emit('user-logged-in', { user, session });

      return {
        success: true,
        data: user,
        message: SUCCESS_MESSAGES.USER_CREATED
      };

    } catch (error) {
      this.logger.error('Login error:', error);
      return {
        success: false,
        error: ERROR_MESSAGES.SYSTEM_ERROR
      };
    }
  }

  public async logout(): Promise<IPCResponse<void>> {
    try {
      if (this.currentSession) {
        const sessionId = this.currentSession.sessionId;
        this.currentSession.isActive = false;
        this.currentSession = null;

        // Clear session timeout
        if (this.sessionTimeout) {
          clearTimeout(this.sessionTimeout);
          this.sessionTimeout = null;
        }

        this.logger.info(`User logged out: ${sessionId}`);
        this.emit('user-logged-out', { sessionId });
      }

      return {
        success: true,
        message: 'Logged out successfully'
      };

    } catch (error) {
      this.logger.error('Logout error:', error);
      return {
        success: false,
        error: ERROR_MESSAGES.SYSTEM_ERROR
      };
    }
  }

  public async checkAuth(): Promise<IPCResponse<User | null>> {
    try {
      if (!this.currentSession || !this.currentSession.isActive) {
        return {
          success: true,
          data: null
        };
      }

      // Check if session has expired
      const now = new Date();
      const sessionAge = now.getTime() - this.currentSession.lastActivity.getTime();
      
      if (sessionAge > SECURITY.SESSION_TIMEOUT) {
        await this.logout();
        return {
          success: true,
          data: null
        };
      }

      // Update last activity
      this.currentSession.lastActivity = now;
      this.setSessionTimeout();

      // Get current user data
      const user = await this.getCurrentUser();
      
      return {
        success: true,
        data: user
      };

    } catch (error) {
      this.logger.error('Auth check error:', error);
      return {
        success: false,
        error: ERROR_MESSAGES.SYSTEM_ERROR
      };
    }
  }

  private createSession(userId: string): Session {
    const sessionId = crypto.randomUUID();
    const now = new Date();

    return {
      userId,
      sessionId,
      createdAt: now,
      lastActivity: now,
      isActive: true
    };
  }

  private setSessionTimeout(): void {
    // Clear existing timeout
    if (this.sessionTimeout) {
      clearTimeout(this.sessionTimeout);
    }

    // Set new timeout
    this.sessionTimeout = setTimeout(async () => {
      this.logger.info('Session expired due to inactivity');
      await this.logout();
    }, SECURITY.SESSION_TIMEOUT);
  }

  private recordLoginAttempt(username: string, success: boolean): void {
    const attempt: LoginAttempt = {
      username,
      timestamp: new Date(),
      success
    };

    if (!this.loginAttempts.has(username)) {
      this.loginAttempts.set(username, []);
    }

    const attempts = this.loginAttempts.get(username)!;
    attempts.push(attempt);

    // Keep only recent attempts (last hour)
    const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000);
    const recentAttempts = attempts.filter(a => a.timestamp > oneHourAgo);
    this.loginAttempts.set(username, recentAttempts);
  }

  private shouldLockAccount(username: string): boolean {
    const attempts = this.loginAttempts.get(username) || [];
    const recentFailedAttempts = attempts.filter(a => !a.success);
    
    return recentFailedAttempts.length >= SECURITY.MAX_LOGIN_ATTEMPTS;
  }

  private lockAccount(username: string): void {
    const lockUntil = new Date(Date.now() + SECURITY.LOCKOUT_DURATION);
    this.lockedAccounts.set(username, lockUntil);
    
    this.logger.warn(`Account locked due to failed login attempts: ${username}`);
    this.emit('account-locked', { username, lockUntil });
  }

  private isAccountLocked(username: string): boolean {
    const lockTime = this.lockedAccounts.get(username);
    
    if (!lockTime) {
      return false;
    }

    if (Date.now() > lockTime.getTime()) {
      // Lock has expired
      this.lockedAccounts.delete(username);
      return false;
    }

    return true;
  }

  private async getCurrentUser(): Promise<User | null> {
    if (!this.currentSession) {
      return null;
    }

    // In a real implementation, you would fetch the user from the database
    // For now, we'll return a basic user object
    // This should be replaced with actual database query
    return {
      id: this.currentSession.userId,
      username: 'current_user',
      email: 'user@example.com',
      role: 'parent',
      createdAt: new Date(),
      isActive: true
    };
  }

  public getCurrentSession(): Session | null {
    return this.currentSession;
  }

  public getLoginAttempts(username: string): LoginAttempt[] {
    return this.loginAttempts.get(username) || [];
  }

  public isUserLoggedIn(): boolean {
    return this.currentSession !== null && this.currentSession.isActive;
  }

  public async changePassword(currentPassword: string, newPassword: string): Promise<IPCResponse<void>> {
    try {
      if (!this.currentSession) {
        return {
          success: false,
          error: ERROR_MESSAGES.UNAUTHORIZED
        };
      }

      // Validate new password
      if (!this.isValidPassword(newPassword)) {
        return {
          success: false,
          error: 'Password does not meet security requirements'
        };
      }

      // Verify current password
      const user = await this.getCurrentUser();
      if (!user) {
        return {
          success: false,
          error: ERROR_MESSAGES.USER_NOT_FOUND
        };
      }

      // In a real implementation, verify current password and update with new one
      // This would involve database operations

      this.logger.info(`Password changed for user: ${user.username}`);
      this.emit('password-changed', { userId: user.id });

      return {
        success: true,
        message: 'Password changed successfully'
      };

    } catch (error) {
      this.logger.error('Change password error:', error);
      return {
        success: false,
        error: ERROR_MESSAGES.SYSTEM_ERROR
      };
    }
  }

  private isValidPassword(password: string): boolean {
    if (password.length < SECURITY.PASSWORD_MIN_LENGTH) {
      return false;
    }

    return SECURITY.PASSWORD_REGEX.test(password);
  }

  public async validateSession(sessionId: string): Promise<boolean> {
    if (!this.currentSession) {
      return false;
    }

    return this.currentSession.sessionId === sessionId && this.currentSession.isActive;
  }

  public getSecurityInfo(): {
    isLoggedIn: boolean;
    sessionAge?: number;
    lastActivity?: Date;
    failedAttempts: number;
  } {
    const currentUser = this.currentSession?.userId;
    const failedAttempts = currentUser ? 
      this.getLoginAttempts(currentUser).filter(a => !a.success).length : 0;

    return {
      isLoggedIn: this.isUserLoggedIn(),
      sessionAge: this.currentSession ? 
        Date.now() - this.currentSession.createdAt.getTime() : undefined,
      lastActivity: this.currentSession?.lastActivity,
      failedAttempts
    };
  }
}
