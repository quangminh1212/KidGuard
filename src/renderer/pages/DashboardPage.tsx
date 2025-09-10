import React, { useEffect, useState } from 'react';
import {
  Box,
  Grid,
  Typography,
  Chip,
  LinearProgress,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Stack,
  Paper,
  Alert
} from '@mui/material';
import {
  MonitorHeart,
  ChildCare,
  Warning,
  TrendingUp,
  PlayArrow,
  Stop,
  Schedule,
  Security,
  Assessment,
  Shield,
  Computer,
  Notifications
} from '@mui/icons-material';
import { motion } from 'framer-motion';
import { useMonitoring } from '../contexts/MonitoringContext';
import { useNotification } from '../contexts/NotificationContext';
import StatCard from '../components/common/StatCard';
import GlassCard from '../components/common/GlassCard';
import GradientButton from '../components/common/GradientButton';
import FadeInUp from '../components/animations/FadeInUp';
import AnimatedCard from '../components/animations/AnimatedCard';

const DashboardPage: React.FC = () => {
  const {
    isActive,
    currentChild,
    children,
    startMonitoring,
    stopMonitoring,
    loadChildren
  } = useMonitoring();
  
  const { showSuccess, showError } = useNotification();
  const [stats, setStats] = useState({
    totalKeystrokes: 1247,
    alertsToday: 3,
    activeTime: '4h 32m',
    securityLevel: 'High'
  });

  const [recentAlerts] = useState([
    {
      id: 1,
      type: 'inappropriate_content',
      message: 'Inappropriate content detected',
      time: '2 minutes ago',
      severity: 'high'
    },
    {
      id: 2,
      type: 'time_violation',
      message: 'Usage outside allowed hours',
      time: '1 hour ago',
      severity: 'medium'
    },
    {
      id: 3,
      type: 'application_blocked',
      message: 'Blocked application access',
      time: '3 hours ago',
      severity: 'low'
    }
  ]);

  useEffect(() => {
    loadChildren();
  }, [loadChildren]);

  const handleQuickStart = async () => {
    if (children.length === 0) {
      showError('No children profiles found. Please add a child first.');
      return;
    }

    if (isActive) {
      const success = await stopMonitoring();
      if (success) {
        showSuccess('Monitoring stopped');
      }
    } else {
      const success = await startMonitoring(children[0].id);
      if (success) {
        showSuccess('Monitoring started');
      }
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'high': return 'error';
      case 'medium': return 'warning';
      case 'low': return 'info';
      default: return 'default';
    }
  };

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1,
      },
    },
  };

  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
    >
      <Box sx={{ p: 3 }}>
        {/* Header */}
        <FadeInUp delay={0.1}>
          <Box sx={{ mb: 4 }}>
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
              Dashboard
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Welcome back! Here's your child protection overview.
            </Typography>
          </Box>
        </FadeInUp>

        {/* Status Alert */}
        {isActive && currentChild && (
          <FadeInUp delay={0.2}>
            <Alert
              severity="success"
              sx={{
                mb: 3,
                borderRadius: 2,
                background: 'rgba(76, 175, 80, 0.1)',
                border: '1px solid rgba(76, 175, 80, 0.2)',
              }}
            >
              <Typography variant="body2">
                <strong>Monitoring Active:</strong> Currently monitoring {currentChild.name}'s activities
              </Typography>
            </Alert>
          </FadeInUp>
        )}

        {/* Stats Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.3}>
              <StatCard
                title="Keystrokes Today"
                value={stats.totalKeystrokes.toLocaleString()}
                icon={Computer}
                color="primary"
                change={{ value: '+12%', positive: true }}
              />
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.4}>
              <StatCard
                title="Alerts Today"
                value={stats.alertsToday}
                icon={Warning}
                color="warning"
                change={{ value: '-25%', positive: true }}
              />
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.5}>
              <StatCard
                title="Active Time"
                value={stats.activeTime}
                icon={Schedule}
                color="secondary"
                change={{ value: '+8%', positive: false }}
              />
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.6}>
              <StatCard
                title="Security Level"
                value={stats.securityLevel}
                icon={Shield}
                color="success"
              />
            </FadeInUp>
          </Grid>
        </Grid>

        <Grid container spacing={3}>
          {/* Quick Actions */}
          <Grid item xs={12} md={6}>
            <FadeInUp delay={0.7}>
              <GlassCard>
                <Box sx={{ p: 3 }}>
                  <Typography variant="h6" sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <MonitorHeart color="primary" />
                    Quick Actions
                  </Typography>
                  
                  <Stack spacing={2}>
                    <GradientButton
                      fullWidth
                      size="large"
                      startIcon={isActive ? <Stop /> : <PlayArrow />}
                      onClick={handleQuickStart}
                      color={isActive ? 'error' : 'primary'}
                      sx={{ py: 1.5 }}
                    >
                      {isActive ? 'Stop Monitoring' : 'Start Monitoring'}
                    </GradientButton>
                    
                    <Box sx={{ display: 'flex', gap: 2 }}>
                      <GradientButton
                        variant="outlined"
                        startIcon={<ChildCare />}
                        sx={{ flex: 1 }}
                      >
                        Manage Children
                      </GradientButton>
                      
                      <GradientButton
                        variant="outlined"
                        startIcon={<Assessment />}
                        sx={{ flex: 1 }}
                      >
                        View Reports
                      </GradientButton>
                    </Box>
                  </Stack>

                  {/* Current Status */}
                  <Paper
                    sx={{
                      mt: 3,
                      p: 2,
                      background: 'rgba(102, 126, 234, 0.05)',
                      border: '1px solid rgba(102, 126, 234, 0.1)',
                      borderRadius: 2,
                    }}
                  >
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                      Current Status:
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Chip
                        icon={<MonitorHeart />}
                        label={isActive ? 'Active' : 'Inactive'}
                        color={isActive ? 'success' : 'default'}
                        size="small"
                      />
                      {currentChild && (
                        <Chip
                          icon={<ChildCare />}
                          label={currentChild.name}
                          color="primary"
                          variant="outlined"
                          size="small"
                        />
                      )}
                    </Box>
                  </Paper>
                </Box>
              </GlassCard>
            </FadeInUp>
          </Grid>

          {/* Recent Alerts */}
          <Grid item xs={12} md={6}>
            <FadeInUp delay={0.8}>
              <GlassCard>
                <Box sx={{ p: 3 }}>
                  <Typography variant="h6" sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Notifications color="primary" />
                    Recent Alerts
                  </Typography>
                  
                  <List sx={{ p: 0 }}>
                    {recentAlerts.map((alert, index) => (
                      <motion.div
                        key={alert.id}
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: 0.9 + index * 0.1 }}
                      >
                        <ListItem
                          sx={{
                            px: 0,
                            py: 1,
                            borderRadius: 1,
                            '&:hover': {
                              background: 'rgba(102, 126, 234, 0.05)',
                            },
                          }}
                        >
                          <ListItemIcon>
                            <Warning color={getSeverityColor(alert.severity) as any} />
                          </ListItemIcon>
                          <ListItemText
                            primary={alert.message}
                            secondary={alert.time}
                            primaryTypographyProps={{ fontSize: '0.875rem' }}
                            secondaryTypographyProps={{ fontSize: '0.75rem' }}
                          />
                          <Chip
                            label={alert.severity}
                            size="small"
                            color={getSeverityColor(alert.severity) as any}
                            sx={{ ml: 1 }}
                          />
                        </ListItem>
                        {index < recentAlerts.length - 1 && <Divider />}
                      </motion.div>
                    ))}
                  </List>
                  
                  <Box sx={{ mt: 2, textAlign: 'center' }}>
                    <GradientButton variant="outlined" size="small">
                      View All Alerts
                    </GradientButton>
                  </Box>
                </Box>
              </GlassCard>
            </FadeInUp>
          </Grid>
        </Grid>
      </Box>
    </motion.div>
  );
};

export default DashboardPage;
