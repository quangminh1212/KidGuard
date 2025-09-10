import React, { useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  Chip,
  LinearProgress,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
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
  Security
} from '@mui/icons-material';
import { useMonitoring } from '../contexts/MonitoringContext';
import { useNotification } from '../contexts/NotificationContext';

// Stats Card Component
interface StatsCardProps {
  title: string;
  value: string | number;
  icon: React.ReactElement;
  color: 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success';
  subtitle?: string;
}

const StatsCard: React.FC<StatsCardProps> = ({ title, value, icon, color, subtitle }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <Box
          sx={{
            p: 1,
            borderRadius: 1,
            backgroundColor: `${color}.light`,
            color: `${color}.contrastText`,
            mr: 2
          }}
        >
          {icon}
        </Box>
        <Box>
          <Typography variant="h4" component="div" fontWeight="bold">
            {value}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {title}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
      </Box>
    </CardContent>
  </Card>
);

// Quick Actions Component
const QuickActions: React.FC = () => {
  const { isActive, currentChild, children, startMonitoring, stopMonitoring } = useMonitoring();
  const { showSuccess, showError } = useNotification();

  const handleStartMonitoring = async () => {
    if (children.length === 0) {
      showError('No children profiles found. Please add a child profile first.');
      return;
    }

    // Use first child if no current child selected
    const childId = currentChild?.id || children[0]?.id;
    if (childId) {
      const success = await startMonitoring(childId);
      if (success) {
        showSuccess('Monitoring started successfully');
      }
    }
  };

  const handleStopMonitoring = async () => {
    const success = await stopMonitoring();
    if (success) {
      showSuccess('Monitoring stopped');
    }
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Quick Actions
        </Typography>
        
        <Box sx={{ mb: 2 }}>
          <Chip
            icon={<MonitorHeart />}
            label={isActive ? 'Monitoring Active' : 'Monitoring Inactive'}
            color={isActive ? 'success' : 'default'}
            variant={isActive ? 'filled' : 'outlined'}
          />
          {currentChild && (
            <Chip
              icon={<ChildCare />}
              label={`Monitoring: ${currentChild.name}`}
              color="primary"
              variant="outlined"
              sx={{ ml: 1 }}
            />
          )}
        </Box>

        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          {!isActive ? (
            <Button
              variant="contained"
              startIcon={<PlayArrow />}
              onClick={handleStartMonitoring}
              disabled={children.length === 0}
            >
              Start Monitoring
            </Button>
          ) : (
            <Button
              variant="outlined"
              startIcon={<Stop />}
              onClick={handleStopMonitoring}
              color="error"
            >
              Stop Monitoring
            </Button>
          )}
        </Box>

        {children.length === 0 && (
          <Alert severity="info" sx={{ mt: 2 }}>
            No children profiles found. Add a child profile to start monitoring.
          </Alert>
        )}
      </CardContent>
    </Card>
  );
};

// Recent Activity Component
const RecentActivity: React.FC = () => {
  const recentActivities = [
    {
      id: 1,
      type: 'monitoring_started',
      message: 'Monitoring started for Emma',
      time: '2 minutes ago',
      icon: <PlayArrow color="success" />
    },
    {
      id: 2,
      type: 'alert',
      message: 'Inappropriate content detected',
      time: '15 minutes ago',
      icon: <Warning color="error" />
    },
    {
      id: 3,
      type: 'system',
      message: 'Daily report generated',
      time: '1 hour ago',
      icon: <TrendingUp color="info" />
    }
  ];

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Recent Activity
        </Typography>
        
        <List dense>
          {recentActivities.map((activity, index) => (
            <React.Fragment key={activity.id}>
              <ListItem>
                <ListItemIcon>
                  {activity.icon}
                </ListItemIcon>
                <ListItemText
                  primary={activity.message}
                  secondary={activity.time}
                />
              </ListItem>
              {index < recentActivities.length - 1 && <Divider />}
            </React.Fragment>
          ))}
        </List>
      </CardContent>
    </Card>
  );
};

const DashboardPage: React.FC = () => {
  const { stats, loadStats, children, loadChildren } = useMonitoring();

  useEffect(() => {
    loadStats();
    loadChildren();
  }, [loadStats, loadChildren]);

  return (
    <Box sx={{ p: 3 }}>
      {/* Welcome Section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Dashboard
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Welcome to ChildGuard. Monitor and protect your children's digital activities.
        </Typography>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatsCard
            title="Keystrokes Today"
            value={stats?.totalKeystrokesToday || 0}
            icon={<Schedule />}
            color="primary"
            subtitle="Total activity"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatsCard
            title="Alerts Today"
            value={stats?.alertsToday || 0}
            icon={<Warning />}
            color="error"
            subtitle="Requires attention"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatsCard
            title="Children Profiles"
            value={children.length}
            icon={<ChildCare />}
            color="success"
            subtitle="Active profiles"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatsCard
            title="Active Time"
            value={`${Math.floor((stats?.activeMonitoringTime || 0) / 60)}h`}
            icon={<MonitorHeart />}
            color="info"
            subtitle="Monitoring time"
          />
        </Grid>
      </Grid>

      {/* Main Content */}
      <Grid container spacing={3}>
        {/* Quick Actions */}
        <Grid item xs={12} md={6}>
          <QuickActions />
        </Grid>

        {/* Recent Activity */}
        <Grid item xs={12} md={6}>
          <RecentActivity />
        </Grid>

        {/* System Status */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                System Status
              </Typography>
              
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6} md={3}>
                  <Box sx={{ textAlign: 'center', p: 2 }}>
                    <Security color="success" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="body2" color="text.secondary">
                      Security Engine
                    </Typography>
                    <Chip label="Active" color="success" size="small" />
                  </Box>
                </Grid>
                
                <Grid item xs={12} sm={6} md={3}>
                  <Box sx={{ textAlign: 'center', p: 2 }}>
                    <MonitorHeart color="success" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="body2" color="text.secondary">
                      Content Filter
                    </Typography>
                    <Chip label="Active" color="success" size="small" />
                  </Box>
                </Grid>
                
                <Grid item xs={12} sm={6} md={3}>
                  <Box sx={{ textAlign: 'center', p: 2 }}>
                    <TrendingUp color="success" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="body2" color="text.secondary">
                      Analytics
                    </Typography>
                    <Chip label="Active" color="success" size="small" />
                  </Box>
                </Grid>
                
                <Grid item xs={12} sm={6} md={3}>
                  <Box sx={{ textAlign: 'center', p: 2 }}>
                    <Warning color="success" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="body2" color="text.secondary">
                      Alert System
                    </Typography>
                    <Chip label="Active" color="success" size="small" />
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DashboardPage;
