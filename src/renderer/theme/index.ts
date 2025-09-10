import { createTheme, ThemeOptions } from '@mui/material/styles';
import { alpha } from '@mui/material/styles';

// Custom color palette
const colors = {
  primary: {
    50: '#e3f2fd',
    100: '#bbdefb',
    200: '#90caf9',
    300: '#64b5f6',
    400: '#42a5f5',
    500: '#2196f3',
    600: '#1e88e5',
    700: '#1976d2',
    800: '#1565c0',
    900: '#0d47a1',
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    gradientHover: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
  },
  secondary: {
    50: '#e8f5e8',
    100: '#c8e6c9',
    200: '#a5d6a7',
    300: '#81c784',
    400: '#66bb6a',
    500: '#4caf50',
    600: '#43a047',
    700: '#388e3c',
    800: '#2e7d32',
    900: '#1b5e20',
    gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
  },
  warning: {
    50: '#fff3e0',
    100: '#ffe0b2',
    200: '#ffcc80',
    300: '#ffb74d',
    400: '#ffa726',
    500: '#ff9800',
    600: '#fb8c00',
    700: '#f57c00',
    800: '#ef6c00',
    900: '#e65100',
    gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
  },
  error: {
    50: '#ffebee',
    100: '#ffcdd2',
    200: '#ef9a9a',
    300: '#e57373',
    400: '#ef5350',
    500: '#f44336',
    600: '#e53935',
    700: '#d32f2f',
    800: '#c62828',
    900: '#b71c1c',
    gradient: 'linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%)',
  },
  success: {
    50: '#e8f5e8',
    100: '#c8e6c9',
    200: '#a5d6a7',
    300: '#81c784',
    400: '#66bb6a',
    500: '#4caf50',
    600: '#43a047',
    700: '#388e3c',
    800: '#2e7d32',
    900: '#1b5e20',
    gradient: 'linear-gradient(135deg, #56ab2f 0%, #a8e6cf 100%)',
  },
  neutral: {
    50: '#fafafa',
    100: '#f5f5f5',
    200: '#eeeeee',
    300: '#e0e0e0',
    400: '#bdbdbd',
    500: '#9e9e9e',
    600: '#757575',
    700: '#616161',
    800: '#424242',
    900: '#212121',
  },
  background: {
    default: '#fafbfc',
    paper: '#ffffff',
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    glassmorphism: 'rgba(255, 255, 255, 0.25)',
  }
};

// Typography configuration
const typography = {
  fontFamily: [
    'Inter',
    'Roboto',
    '-apple-system',
    'BlinkMacSystemFont',
    '"Segoe UI"',
    '"Helvetica Neue"',
    'Arial',
    'sans-serif',
  ].join(','),
  h1: {
    fontSize: '2.5rem',
    fontWeight: 700,
    lineHeight: 1.2,
    letterSpacing: '-0.02em',
  },
  h2: {
    fontSize: '2rem',
    fontWeight: 600,
    lineHeight: 1.3,
    letterSpacing: '-0.01em',
  },
  h3: {
    fontSize: '1.75rem',
    fontWeight: 600,
    lineHeight: 1.3,
  },
  h4: {
    fontSize: '1.5rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  h5: {
    fontSize: '1.25rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  h6: {
    fontSize: '1.125rem',
    fontWeight: 600,
    lineHeight: 1.4,
  },
  body1: {
    fontSize: '1rem',
    lineHeight: 1.6,
  },
  body2: {
    fontSize: '0.875rem',
    lineHeight: 1.6,
  },
  button: {
    fontSize: '0.875rem',
    fontWeight: 600,
    textTransform: 'none' as const,
    letterSpacing: '0.02em',
  },
  caption: {
    fontSize: '0.75rem',
    lineHeight: 1.4,
  },
};

// Shadow system
const shadows = [
  'none',
  '0px 2px 4px rgba(0, 0, 0, 0.05)',
  '0px 4px 8px rgba(0, 0, 0, 0.08)',
  '0px 8px 16px rgba(0, 0, 0, 0.1)',
  '0px 12px 24px rgba(0, 0, 0, 0.12)',
  '0px 16px 32px rgba(0, 0, 0, 0.15)',
  '0px 20px 40px rgba(0, 0, 0, 0.18)',
  '0px 24px 48px rgba(0, 0, 0, 0.2)',
  '0px 32px 64px rgba(0, 0, 0, 0.25)',
  // ... continue with more shadows
] as any;

// Component overrides
const components = {
  MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        padding: '12px 24px',
        fontSize: '0.875rem',
        fontWeight: 600,
        textTransform: 'none' as const,
        boxShadow: 'none',
        transition: 'all 0.2s ease-in-out',
        '&:hover': {
          boxShadow: '0px 4px 12px rgba(0, 0, 0, 0.15)',
          transform: 'translateY(-1px)',
        },
      },
      contained: {
        background: colors.primary.gradient,
        color: '#ffffff',
        '&:hover': {
          background: colors.primary.gradientHover,
        },
      },
      outlined: {
        borderWidth: 2,
        '&:hover': {
          borderWidth: 2,
        },
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 16,
        boxShadow: '0px 4px 20px rgba(0, 0, 0, 0.08)',
        border: '1px solid rgba(255, 255, 255, 0.2)',
        backdrop: 'blur(20px)',
        transition: 'all 0.3s ease-in-out',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: '0px 8px 30px rgba(0, 0, 0, 0.12)',
        },
      },
    },
  },
  MuiPaper: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        backgroundImage: 'none',
      },
      elevation1: {
        boxShadow: '0px 2px 8px rgba(0, 0, 0, 0.06)',
      },
    },
  },
  MuiTextField: {
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: 12,
          transition: 'all 0.2s ease-in-out',
          '&:hover': {
            '& .MuiOutlinedInput-notchedOutline': {
              borderColor: colors.primary[400],
            },
          },
          '&.Mui-focused': {
            '& .MuiOutlinedInput-notchedOutline': {
              borderWidth: 2,
              borderColor: colors.primary[500],
            },
          },
        },
      },
    },
  },
  MuiChip: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        fontWeight: 500,
      },
      filled: {
        background: colors.primary.gradient,
        color: '#ffffff',
      },
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        background: 'rgba(255, 255, 255, 0.95)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(0, 0, 0, 0.08)',
        boxShadow: 'none',
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        background: 'rgba(255, 255, 255, 0.95)',
        backdropFilter: 'blur(20px)',
        borderRight: '1px solid rgba(0, 0, 0, 0.08)',
      },
    },
  },
  MuiListItemButton: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        margin: '4px 8px',
        transition: 'all 0.2s ease-in-out',
        '&:hover': {
          background: alpha(colors.primary[500], 0.08),
          transform: 'translateX(4px)',
        },
        '&.Mui-selected': {
          background: colors.primary.gradient,
          color: '#ffffff',
          '&:hover': {
            background: colors.primary.gradientHover,
          },
        },
      },
    },
  },
};

// Create light theme
const lightTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: colors.primary[500],
      light: colors.primary[300],
      dark: colors.primary[700],
      contrastText: '#ffffff',
    },
    secondary: {
      main: colors.secondary[500],
      light: colors.secondary[300],
      dark: colors.secondary[700],
      contrastText: '#ffffff',
    },
    error: {
      main: colors.error[500],
      light: colors.error[300],
      dark: colors.error[700],
    },
    warning: {
      main: colors.warning[500],
      light: colors.warning[300],
      dark: colors.warning[700],
    },
    success: {
      main: colors.success[500],
      light: colors.success[300],
      dark: colors.success[700],
    },
    background: {
      default: colors.background.default,
      paper: colors.background.paper,
    },
    text: {
      primary: colors.neutral[900],
      secondary: colors.neutral[600],
    },
  },
  typography,
  shadows,
  components,
  shape: {
    borderRadius: 12,
  },
} as ThemeOptions);

// Create dark theme
const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: colors.primary[400],
      light: colors.primary[200],
      dark: colors.primary[600],
      contrastText: '#ffffff',
    },
    secondary: {
      main: colors.secondary[400],
      light: colors.secondary[200],
      dark: colors.secondary[600],
      contrastText: '#ffffff',
    },
    background: {
      default: '#0a0e1a',
      paper: '#1a1f2e',
    },
    text: {
      primary: '#ffffff',
      secondary: colors.neutral[400],
    },
  },
  typography,
  shadows,
  components: {
    ...components,
    MuiCard: {
      styleOverrides: {
        root: {
          ...components.MuiCard.styleOverrides.root,
          background: 'rgba(26, 31, 46, 0.8)',
          border: '1px solid rgba(255, 255, 255, 0.1)',
        },
      },
    },
  },
  shape: {
    borderRadius: 12,
  },
} as ThemeOptions);

export { lightTheme, darkTheme, colors };
export default lightTheme;
