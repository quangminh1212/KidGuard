import React, { useState } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Box, CircularProgress, Typography, IconButton } from '@mui/material';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, GlobalStyles } from '@mui/material';
import { Brightness4, Brightness7 } from '@mui/icons-material';
import { useAuth } from './contexts/AuthContext';
import { lightTheme, darkTheme } from './theme';
import LoadingSpinner from './components/animations/LoadingSpinner';
import LoginPage from './pages/LoginPage';
import DashboardLayout from './components/layout/DashboardLayout';
import DashboardPage from './pages/DashboardPage';
import MonitoringPage from './pages/MonitoringPage';
import ChildrenPage from './pages/ChildrenPage';
import AlertsPage from './pages/AlertsPage';
import ReportsPage from './pages/ReportsPage';
import SettingsPage from './pages/SettingsPage';
import UITestPage from './pages/UITestPage';

// Global styles
const globalStyles = (
  <GlobalStyles
    styles={(theme) => ({
      '*': {
        boxSizing: 'border-box',
      },
      html: {
        WebkitFontSmoothing: 'antialiased',
        MozOsxFontSmoothing: 'grayscale',
      },
      body: {
        margin: 0,
        padding: 0,
        fontFamily: theme.typography.fontFamily,
        background: theme.palette.mode === 'light'
          ? 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)'
          : 'linear-gradient(135deg, #0c0c0c 0%, #1a1a2e 100%)',
        minHeight: '100vh',
      },
      '#root': {
        minHeight: '100vh',
      },
      '::-webkit-scrollbar': {
        width: 8,
      },
      '::-webkit-scrollbar-track': {
        background: 'rgba(0, 0, 0, 0.1)',
        borderRadius: 4,
      },
      '::-webkit-scrollbar-thumb': {
        background: 'rgba(0, 0, 0, 0.2)',
        borderRadius: 4,
        '&:hover': {
          background: 'rgba(0, 0, 0, 0.3)',
        },
      },
    })}
  />
);

// Loading component
const LoadingScreen: React.FC = () => (
  <Box
    sx={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      height: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      color: 'white',
      position: 'relative',
      overflow: 'hidden',

      '&::before': {
        content: '""',
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: 'radial-gradient(circle at 30% 20%, rgba(255, 255, 255, 0.1), transparent 50%)',
      },
    }}
  >
    <Box
      sx={{
        width: 100,
        height: 100,
        borderRadius: '50%',
        background: 'rgba(255, 255, 255, 0.15)',
        backdropFilter: 'blur(20px)',
        border: '1px solid rgba(255, 255, 255, 0.2)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        mb: 4,
        fontSize: '36px',
        fontWeight: 'bold',
        color: 'white',
        position: 'relative',
        zIndex: 1,
      }}
    >
      🛡️
    </Box>
    <LoadingSpinner size={50} color="white" />
    <Typography variant="h4" sx={{ mb: 1, fontWeight: 600, mt: 3 }}>
      ChildGuard
    </Typography>
    <Typography variant="body1" sx={{ opacity: 0.9, textAlign: 'center', maxWidth: 300 }}>
      Initializing Child Protection System...
    </Typography>
  </Box>
);

// Protected route component
interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

// Public route component (redirects to dashboard if authenticated)
const PublicRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

const App: React.FC = () => {
  const { isLoading } = useAuth();
  const [darkMode, setDarkMode] = useState(false);

  const toggleDarkMode = () => {
    setDarkMode(!darkMode);
  };

  // Show loading screen while checking authentication
  if (isLoading) {
    return (
      <ThemeProvider theme={darkMode ? darkTheme : lightTheme}>
        <CssBaseline />
        {globalStyles}
        <LoadingScreen />
      </ThemeProvider>
    );
  }

  return (
    <ThemeProvider theme={darkMode ? darkTheme : lightTheme}>
      <CssBaseline />
      {globalStyles}

      {/* Theme toggle button */}
      <IconButton
        onClick={toggleDarkMode}
        sx={{
          position: 'fixed',
          top: 16,
          right: 16,
          zIndex: 9999,
          background: 'rgba(255, 255, 255, 0.1)',
          backdropFilter: 'blur(20px)',
          border: '1px solid rgba(255, 255, 255, 0.2)',
          color: 'text.primary',
          '&:hover': {
            background: 'rgba(255, 255, 255, 0.2)',
          },
        }}
      >
        {darkMode ? <Brightness7 /> : <Brightness4 />}
      </IconButton>

      <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column' }}>
        <Routes>
          {/* Public routes */}
          <Route
            path="/login"
            element={
              <PublicRoute>
                <LoginPage />
              </PublicRoute>
            }
          />

          {/* Protected routes */}
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            {/* Dashboard routes */}
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="monitoring" element={<MonitoringPage />} />
            <Route path="children" element={<ChildrenPage />} />
            <Route path="alerts" element={<AlertsPage />} />
            <Route path="reports" element={<ReportsPage />} />
            <Route path="settings" element={<SettingsPage />} />
            <Route path="ui-test" element={<UITestPage />} />
          </Route>

          {/* Catch all route */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Box>
    </ThemeProvider>
  );
};

export default App;
