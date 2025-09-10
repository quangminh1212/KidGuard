import React, { createContext, useContext, useReducer, ReactNode, useCallback } from 'react';
import { Snackbar, Alert, AlertColor } from '@mui/material';

interface Notification {
  id: string;
  message: string;
  severity: AlertColor;
  duration?: number;
  action?: React.ReactNode;
}

interface NotificationState {
  notifications: Notification[];
}

type NotificationAction =
  | { type: 'ADD_NOTIFICATION'; payload: Notification }
  | { type: 'REMOVE_NOTIFICATION'; payload: string };

interface NotificationContextType {
  showNotification: (message: string, severity?: AlertColor, duration?: number, action?: React.ReactNode) => void;
  showSuccess: (message: string, duration?: number) => void;
  showError: (message: string, duration?: number) => void;
  showWarning: (message: string, duration?: number) => void;
  showInfo: (message: string, duration?: number) => void;
  removeNotification: (id: string) => void;
}

const initialState: NotificationState = {
  notifications: []
};

const notificationReducer = (state: NotificationState, action: NotificationAction): NotificationState => {
  switch (action.type) {
    case 'ADD_NOTIFICATION':
      return {
        ...state,
        notifications: [...state.notifications, action.payload]
      };

    case 'REMOVE_NOTIFICATION':
      return {
        ...state,
        notifications: state.notifications.filter(notification => notification.id !== action.payload)
      };

    default:
      return state;
  }
};

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const useNotification = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(notificationReducer, initialState);

  const generateId = (): string => {
    return `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  };

  const showNotification = useCallback((
    message: string,
    severity: AlertColor = 'info',
    duration: number = 6000,
    action?: React.ReactNode
  ): void => {
    const id = generateId();
    const notification: Notification = {
      id,
      message,
      severity,
      duration,
      action
    };

    dispatch({ type: 'ADD_NOTIFICATION', payload: notification });

    // Auto-remove notification after duration
    if (duration > 0) {
      setTimeout(() => {
        dispatch({ type: 'REMOVE_NOTIFICATION', payload: id });
      }, duration);
    }
  }, []);

  const showSuccess = useCallback((message: string, duration: number = 4000): void => {
    showNotification(message, 'success', duration);
  }, [showNotification]);

  const showError = useCallback((message: string, duration: number = 8000): void => {
    showNotification(message, 'error', duration);
  }, [showNotification]);

  const showWarning = useCallback((message: string, duration: number = 6000): void => {
    showNotification(message, 'warning', duration);
  }, [showNotification]);

  const showInfo = useCallback((message: string, duration: number = 6000): void => {
    showNotification(message, 'info', duration);
  }, [showNotification]);

  const removeNotification = useCallback((id: string): void => {
    dispatch({ type: 'REMOVE_NOTIFICATION', payload: id });
  }, []);

  const contextValue: NotificationContextType = {
    showNotification,
    showSuccess,
    showError,
    showWarning,
    showInfo,
    removeNotification
  };

  return (
    <NotificationContext.Provider value={contextValue}>
      {children}
      
      {/* Render notifications */}
      {state.notifications.map((notification, index) => (
        <Snackbar
          key={notification.id}
          open={true}
          anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
          style={{
            top: `${80 + index * 70}px` // Stack notifications vertically
          }}
          onClose={() => removeNotification(notification.id)}
        >
          <Alert
            onClose={() => removeNotification(notification.id)}
            severity={notification.severity}
            variant="filled"
            action={notification.action}
            sx={{
              minWidth: '300px',
              maxWidth: '500px',
              boxShadow: '0 4px 16px rgba(0,0,0,0.15)',
              '& .MuiAlert-message': {
                fontSize: '0.875rem',
                lineHeight: 1.4
              }
            }}
          >
            {notification.message}
          </Alert>
        </Snackbar>
      ))}
    </NotificationContext.Provider>
  );
};
