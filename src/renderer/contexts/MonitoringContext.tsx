import React, { createContext, useContext, useReducer, useEffect, ReactNode, useCallback } from 'react';
import type { Child, MonitoringSettings, SystemStats } from '@shared/types';
import { useNotification } from './NotificationContext';

interface MonitoringState {
  isActive: boolean;
  currentChild: Child | null;
  children: Child[];
  settings: MonitoringSettings | null;
  stats: SystemStats | null;
  isLoading: boolean;
  error: string | null;
}

type MonitoringAction =
  | { type: 'MONITORING_START' }
  | { type: 'MONITORING_SUCCESS'; payload: { isActive: boolean; child?: Child } }
  | { type: 'MONITORING_STOP' }
  | { type: 'MONITORING_ERROR'; payload: string }
  | { type: 'SET_CHILDREN'; payload: Child[] }
  | { type: 'SET_CURRENT_CHILD'; payload: Child | null }
  | { type: 'SET_SETTINGS'; payload: MonitoringSettings }
  | { type: 'SET_STATS'; payload: SystemStats }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'CLEAR_ERROR' };

interface MonitoringContextType extends MonitoringState {
  startMonitoring: (childId: string) => Promise<boolean>;
  stopMonitoring: () => Promise<boolean>;
  loadChildren: (parentId?: string) => Promise<void>;
  loadSettings: (childId: string) => Promise<void>;
  updateSettings: (childId: string, settings: Partial<MonitoringSettings>) => Promise<boolean>;
  loadStats: () => Promise<void>;
  refreshStatus: () => Promise<void>;
  clearError: () => void;
}

const initialState: MonitoringState = {
  isActive: false,
  currentChild: null,
  children: [],
  settings: null,
  stats: null,
  isLoading: false,
  error: null
};

const monitoringReducer = (state: MonitoringState, action: MonitoringAction): MonitoringState => {
  switch (action.type) {
    case 'MONITORING_START':
      return {
        ...state,
        isLoading: true,
        error: null
      };

    case 'MONITORING_SUCCESS':
      return {
        ...state,
        isActive: action.payload.isActive,
        currentChild: action.payload.child || state.currentChild,
        isLoading: false,
        error: null
      };

    case 'MONITORING_STOP':
      return {
        ...state,
        isActive: false,
        currentChild: null,
        isLoading: false,
        error: null
      };

    case 'MONITORING_ERROR':
      return {
        ...state,
        isLoading: false,
        error: action.payload
      };

    case 'SET_CHILDREN':
      return {
        ...state,
        children: action.payload
      };

    case 'SET_CURRENT_CHILD':
      return {
        ...state,
        currentChild: action.payload
      };

    case 'SET_SETTINGS':
      return {
        ...state,
        settings: action.payload
      };

    case 'SET_STATS':
      return {
        ...state,
        stats: action.payload
      };

    case 'SET_LOADING':
      return {
        ...state,
        isLoading: action.payload
      };

    case 'CLEAR_ERROR':
      return {
        ...state,
        error: null
      };

    default:
      return state;
  }
};

const MonitoringContext = createContext<MonitoringContextType | undefined>(undefined);

export const useMonitoring = (): MonitoringContextType => {
  const context = useContext(MonitoringContext);
  if (!context) {
    throw new Error('useMonitoring must be used within a MonitoringProvider');
  }
  return context;
};

interface MonitoringProviderProps {
  children: ReactNode;
}

export const MonitoringProvider: React.FC<MonitoringProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(monitoringReducer, initialState);
  const { showError, showSuccess, showWarning } = useNotification();

  // Load initial data
  useEffect(() => {
    refreshStatus();
    loadStats();
    
    // Set up periodic stats refresh
    const statsInterval = setInterval(loadStats, 30000); // Every 30 seconds
    
    return () => {
      clearInterval(statsInterval);
    };
  }, []);

  const startMonitoring = useCallback(async (childId: string): Promise<boolean> => {
    try {
      dispatch({ type: 'MONITORING_START' });

      if (!window.electronAPI) {
        throw new Error('Electron API not available');
      }

      const response = await window.electronAPI.monitoring.start(childId);

      if (response.success) {
        const child = state.children.find(c => c.id === childId) || null;
        dispatch({ 
          type: 'MONITORING_SUCCESS', 
          payload: { isActive: true, child } 
        });
        
        showSuccess(`Monitoring started for ${child?.name || 'child'}`);
        return true;
      } else {
        dispatch({ type: 'MONITORING_ERROR', payload: response.error || 'Failed to start monitoring' });
        showError(response.error || 'Failed to start monitoring');
        return false;
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      dispatch({ type: 'MONITORING_ERROR', payload: errorMessage });
      showError(errorMessage);
      return false;
    }
  }, [state.children, showSuccess, showError]);

  const stopMonitoring = useCallback(async (): Promise<boolean> => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });

      if (!window.electronAPI) {
        throw new Error('Electron API not available');
      }

      const response = await window.electronAPI.monitoring.stop();

      if (response.success) {
        dispatch({ type: 'MONITORING_STOP' });
        showWarning('Monitoring stopped');
        return true;
      } else {
        dispatch({ type: 'MONITORING_ERROR', payload: response.error || 'Failed to stop monitoring' });
        showError(response.error || 'Failed to stop monitoring');
        return false;
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      dispatch({ type: 'MONITORING_ERROR', payload: errorMessage });
      showError(errorMessage);
      return false;
    }
  }, [showWarning, showError]);

  const loadChildren = useCallback(async (parentId?: string): Promise<void> => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });

      if (!window.electronAPI) {
        throw new Error('Electron API not available');
      }

      const response = await window.electronAPI.children.list(parentId);

      if (response.success && response.data) {
        dispatch({ type: 'SET_CHILDREN', payload: response.data });
      } else {
        dispatch({ type: 'MONITORING_ERROR', payload: response.error || 'Failed to load children' });
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      dispatch({ type: 'MONITORING_ERROR', payload: errorMessage });
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, []);

  const loadSettings = useCallback(async (childId: string): Promise<void> => {
    try {
      if (!window.electronAPI) {
        throw new Error('Electron API not available');
      }

      const response = await window.electronAPI.monitoring.getSettings(childId);

      if (response.success && response.data) {
        dispatch({ type: 'SET_SETTINGS', payload: response.data });
      } else {
        dispatch({ type: 'MONITORING_ERROR', payload: response.error || 'Failed to load settings' });
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      dispatch({ type: 'MONITORING_ERROR', payload: errorMessage });
    }
  }, []);

  const updateSettings = useCallback(async (
    childId: string, 
    settings: Partial<MonitoringSettings>
  ): Promise<boolean> => {
    try {
      if (!window.electronAPI) {
        throw new Error('Electron API not available');
      }

      const response = await window.electronAPI.monitoring.updateSettings(childId, settings);

      if (response.success && response.data) {
        dispatch({ type: 'SET_SETTINGS', payload: response.data });
        showSuccess('Settings updated successfully');
        return true;
      } else {
        showError(response.error || 'Failed to update settings');
        return false;
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      showError(errorMessage);
      return false;
    }
  }, [showSuccess, showError]);

  const loadStats = useCallback(async (): Promise<void> => {
    try {
      if (!window.electronAPI) {
        return;
      }

      const response = await window.electronAPI.system.getStats();

      if (response.success && response.data) {
        dispatch({ type: 'SET_STATS', payload: response.data });
      }
    } catch (error) {
      console.error('Failed to load stats:', error);
    }
  }, []);

  const refreshStatus = useCallback(async (): Promise<void> => {
    try {
      if (!window.electronAPI) {
        return;
      }

      const response = await window.electronAPI.monitoring.getStatus();

      if (response.success && response.data) {
        const { isActive, childId } = response.data;
        const child = childId ? state.children.find(c => c.id === childId) || null : null;
        
        dispatch({ 
          type: 'MONITORING_SUCCESS', 
          payload: { isActive, child } 
        });
      }
    } catch (error) {
      console.error('Failed to refresh status:', error);
    }
  }, [state.children]);

  const clearError = useCallback((): void => {
    dispatch({ type: 'CLEAR_ERROR' });
  }, []);

  const contextValue: MonitoringContextType = {
    ...state,
    startMonitoring,
    stopMonitoring,
    loadChildren,
    loadSettings,
    updateSettings,
    loadStats,
    refreshStatus,
    clearError
  };

  return (
    <MonitoringContext.Provider value={contextValue}>
      {children}
    </MonitoringContext.Provider>
  );
};
