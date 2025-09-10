import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  Typography,
  Alert,
  InputAdornment,
  IconButton,
  Container,
  Stack,
  Chip,
  Paper
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Person,
  Lock,
  Security,
  Shield
} from '@mui/icons-material';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../contexts/AuthContext';
import { useNotification } from '../contexts/NotificationContext';
import GradientButton from '../components/common/GradientButton';
import GlassCard from '../components/common/GlassCard';
import FadeInUp from '../components/animations/FadeInUp';

const LoginPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  
  const { login, error, clearError } = useAuth();
  const { showError } = useNotification();

  // Clear error when component mounts or inputs change
  useEffect(() => {
    clearError();
  }, [username, password, clearError]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!username.trim() || !password.trim()) {
      showError('Please enter both username and password');
      return;
    }

    setIsLoading(true);
    
    try {
      const success = await login(username.trim(), password);
      if (!success) {
        showError('Login failed. Please check your credentials.');
      }
    } catch (err) {
      showError('An error occurred during login. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleTogglePassword = () => {
    setShowPassword(!showPassword);
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        position: 'relative',
        overflow: 'hidden',
        
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3), transparent 50%), radial-gradient(circle at 80% 20%, rgba(255, 255, 255, 0.1), transparent 50%)',
        },
        
        '&::after': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.05"%3E%3Ccircle cx="30" cy="30" r="1"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
        },
      }}
    >
      <Container maxWidth="sm" sx={{ position: 'relative', zIndex: 1 }}>
        <FadeInUp delay={0.2}>
          <GlassCard
            sx={{
              p: 4,
              maxWidth: 450,
              mx: 'auto',
              textAlign: 'center',
            }}
          >
            {/* Logo and Header */}
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{ delay: 0.3, type: "spring", stiffness: 200 }}
            >
              <Box
                sx={{
                  width: 80,
                  height: 80,
                  borderRadius: '50%',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: '0 8px 32px rgba(102, 126, 234, 0.4)',
                }}
              >
                <Shield sx={{ fontSize: 40, color: 'white' }} />
              </Box>
            </motion.div>

            <FadeInUp delay={0.4}>
              <Typography
                variant="h4"
                component="h1"
                sx={{
                  mb: 1,
                  fontWeight: 700,
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                  backgroundClip: 'text',
                }}
              >
                ChildGuard
              </Typography>
            </FadeInUp>

            <FadeInUp delay={0.5}>
              <Typography
                variant="body1"
                color="text.secondary"
                sx={{ mb: 4, fontSize: '1.1rem' }}
              >
                Child Protection System
              </Typography>
            </FadeInUp>

            {/* Login Form */}
            <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
              <Stack spacing={3}>
                <FadeInUp delay={0.6}>
                  <TextField
                    fullWidth
                    label="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    disabled={isLoading}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <Person color="action" />
                        </InputAdornment>
                      ),
                    }}
                    sx={{
                      '& .MuiOutlinedInput-root': {
                        background: 'rgba(255, 255, 255, 0.8)',
                        backdropFilter: 'blur(10px)',
                      },
                    }}
                  />
                </FadeInUp>

                <FadeInUp delay={0.7}>
                  <TextField
                    fullWidth
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={isLoading}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <Lock color="action" />
                        </InputAdornment>
                      ),
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={handleTogglePassword}
                            edge="end"
                            disabled={isLoading}
                          >
                            {showPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    sx={{
                      '& .MuiOutlinedInput-root': {
                        background: 'rgba(255, 255, 255, 0.8)',
                        backdropFilter: 'blur(10px)',
                      },
                    }}
                  />
                </FadeInUp>

                <AnimatePresence>
                  {error && (
                    <motion.div
                      initial={{ opacity: 0, y: -10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: -10 }}
                      transition={{ duration: 0.3 }}
                    >
                      <Alert severity="error" sx={{ borderRadius: 2 }}>
                        {error}
                      </Alert>
                    </motion.div>
                  )}
                </AnimatePresence>

                <FadeInUp delay={0.8}>
                  <GradientButton
                    type="submit"
                    fullWidth
                    size="large"
                    disabled={isLoading || !username.trim() || !password.trim()}
                    sx={{ py: 1.5, fontSize: '1rem' }}
                  >
                    {isLoading ? 'Signing In...' : 'Sign In'}
                  </GradientButton>
                </FadeInUp>
              </Stack>
            </Box>

            {/* Default Credentials Info */}
            <FadeInUp delay={0.9}>
              <Paper
                sx={{
                  mt: 4,
                  p: 2,
                  background: 'rgba(255, 255, 255, 0.6)',
                  backdropFilter: 'blur(10px)',
                  border: '1px solid rgba(255, 255, 255, 0.3)',
                  borderRadius: 2,
                }}
              >
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Default Login Credentials:
                </Typography>
                <Stack direction="row" spacing={1} justifyContent="center">
                  <Chip
                    label="admin"
                    size="small"
                    sx={{
                      background: 'rgba(102, 126, 234, 0.1)',
                      color: 'primary.main',
                      fontWeight: 600,
                    }}
                  />
                  <Chip
                    label="admin123"
                    size="small"
                    sx={{
                      background: 'rgba(102, 126, 234, 0.1)',
                      color: 'primary.main',
                      fontWeight: 600,
                    }}
                  />
                </Stack>
                <Typography variant="caption" color="warning.main" sx={{ mt: 1, display: 'block' }}>
                  ⚠️ Change password after first login
                </Typography>
              </Paper>
            </FadeInUp>

            {/* Security Badge */}
            <FadeInUp delay={1.0}>
              <Box sx={{ mt: 3, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
                <Security sx={{ fontSize: 16, color: 'text.secondary' }} />
                <Typography variant="caption" color="text.secondary">
                  Secured with AES-256 encryption
                </Typography>
              </Box>
            </FadeInUp>
          </GlassCard>
        </FadeInUp>
      </Container>
    </Box>
  );
};

export default LoginPage;
