import React, { useEffect, useState } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  Switch,
  FormControlLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  Alert,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon
} from '@mui/material';
import {
  PlayArrow,
  Stop,
  MonitorHeart,
  ChildCare,
  Security,
  FilterList,
  Schedule,
  Computer
} from '@mui/icons-material';
import { useMonitoring } from '../contexts/MonitoringContext';
import { useNotification } from '../contexts/NotificationContext';

const MonitoringPage: React.FC = () => {
  const {
    isActive,
    currentChild,
    children,
    settings,
    startMonitoring,
    stopMonitoring,
    loadChildren,
    loadSettings,
    updateSettings
  } = useMonitoring();
  
  const { showSuccess, showError } = useNotification();
  const [selectedChildId, setSelectedChildId] = useState<string>('');

  useEffect(() => {
    loadChildren();
  }, [loadChildren]);

  useEffect(() => {
    if (currentChild) {
      setSelectedChildId(currentChild.id);
      loadSettings(currentChild.id);
    }
  }, [currentChild, loadSettings]);

  const handleStartMonitoring = async () => {
    if (!selectedChildId) {
      showError('Please select a child to monitor');
      return;
    }

    const success = await startMonitoring(selectedChildId);
    if (success) {
      showSuccess('Monitoring started successfully');
    }
  };

  const handleStopMonitoring = async () => {
    const success = await stopMonitoring();
    if (success) {
      showSuccess('Monitoring stopped');
    }
  };

  const handleSettingChange = async (setting: string, value: any) => {
    if (!currentChild) return;

    const success = await updateSettings(currentChild.id, {
      [setting]: value
    });

    if (success) {
      showSuccess('Settings updated successfully');
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Monitoring Control
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Start, stop, and configure real-time monitoring for your children.
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* Monitoring Control */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <MonitorHeart sx={{ mr: 1, verticalAlign: 'middle' }} />
                Monitoring Control
              </Typography>

              {/* Status */}
              <Box sx={{ mb: 3 }}>
                <Chip
                  icon={<MonitorHeart />}
                  label={isActive ? 'Monitoring Active' : 'Monitoring Inactive'}
                  color={isActive ? 'success' : 'default'}
                  variant={isActive ? 'filled' : 'outlined'}
                  size="large"
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

              {/* Child Selection */}
              {!isActive && (
                <FormControl fullWidth sx={{ mb: 3 }}>
                  <InputLabel>Select Child</InputLabel>
                  <Select
                    value={selectedChildId}
                    onChange={(e) => setSelectedChildId(e.target.value)}
                    label="Select Child"
                  >
                    {children.map((child) => (
                      <MenuItem key={child.id} value={child.id}>
                        {child.name} (Age: {child.age})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}

              {/* Control Buttons */}
              <Box sx={{ display: 'flex', gap: 2 }}>
                {!isActive ? (
                  <Button
                    variant="contained"
                    startIcon={<PlayArrow />}
                    onClick={handleStartMonitoring}
                    disabled={!selectedChildId || children.length === 0}
                    size="large"
                  >
                    Start Monitoring
                  </Button>
                ) : (
                  <Button
                    variant="outlined"
                    startIcon={<Stop />}
                    onClick={handleStopMonitoring}
                    color="error"
                    size="large"
                  >
                    Stop Monitoring
                  </Button>
                )}
              </Box>

              {children.length === 0 && (
                <Alert severity="info" sx={{ mt: 2 }}>
                  No children profiles found. Please add a child profile first.
                </Alert>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Current Status */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Computer sx={{ mr: 1, verticalAlign: 'middle' }} />
                Current Status
              </Typography>

              <List>
                <ListItem>
                  <ListItemIcon>
                    <MonitorHeart color={isActive ? 'success' : 'disabled'} />
                  </ListItemIcon>
                  <ListItemText
                    primary="Keylogger Service"
                    secondary={isActive ? 'Active and monitoring' : 'Inactive'}
                  />
                </ListItem>

                <ListItem>
                  <ListItemIcon>
                    <FilterList color={settings?.contentFilterEnabled ? 'success' : 'disabled'} />
                  </ListItemIcon>
                  <ListItemText
                    primary="Content Filter"
                    secondary={settings?.contentFilterEnabled ? 'Filtering enabled' : 'Filtering disabled'}
                  />
                </ListItem>

                <ListItem>
                  <ListItemIcon>
                    <Security color={settings?.realTimeAlerts ? 'success' : 'disabled'} />
                  </ListItemIcon>
                  <ListItemText
                    primary="Real-time Alerts"
                    secondary={settings?.realTimeAlerts ? 'Alerts enabled' : 'Alerts disabled'}
                  />
                </ListItem>

                <ListItem>
                  <ListItemIcon>
                    <Schedule color={settings?.timeRestrictionsEnabled ? 'success' : 'disabled'} />
                  </ListItemIcon>
                  <ListItemText
                    primary="Time Restrictions"
                    secondary={settings?.timeRestrictionsEnabled ? 'Restrictions active' : 'No restrictions'}
                  />
                </ListItem>
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Monitoring Settings */}
        {currentChild && settings && (
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Monitoring Settings for {currentChild.name}
                </Typography>

                <Grid container spacing={3}>
                  {/* Basic Settings */}
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle1" gutterBottom>
                      Basic Monitoring
                    </Typography>

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.keyloggerEnabled}
                          onChange={(e) => handleSettingChange('keyloggerEnabled', e.target.checked)}
                        />
                      }
                      label="Keylogger Monitoring"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.contentFilterEnabled}
                          onChange={(e) => handleSettingChange('contentFilterEnabled', e.target.checked)}
                        />
                      }
                      label="Content Filtering"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.timeRestrictionsEnabled}
                          onChange={(e) => handleSettingChange('timeRestrictionsEnabled', e.target.checked)}
                        />
                      }
                      label="Time Restrictions"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.applicationBlockingEnabled}
                          onChange={(e) => handleSettingChange('applicationBlockingEnabled', e.target.checked)}
                        />
                      }
                      label="Application Blocking"
                    />
                  </Grid>

                  {/* Notification Settings */}
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle1" gutterBottom>
                      Notifications
                    </Typography>

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.notificationSettings.realTimeAlerts}
                          onChange={(e) => handleSettingChange('notificationSettings', {
                            ...settings.notificationSettings,
                            realTimeAlerts: e.target.checked
                          })}
                        />
                      }
                      label="Real-time Alerts"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.notificationSettings.emailNotifications}
                          onChange={(e) => handleSettingChange('notificationSettings', {
                            ...settings.notificationSettings,
                            emailNotifications: e.target.checked
                          })}
                        />
                      }
                      label="Email Notifications"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.notificationSettings.dailyReports}
                          onChange={(e) => handleSettingChange('notificationSettings', {
                            ...settings.notificationSettings,
                            dailyReports: e.target.checked
                          })}
                        />
                      }
                      label="Daily Reports"
                    />

                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.notificationSettings.weeklyReports}
                          onChange={(e) => handleSettingChange('notificationSettings', {
                            ...settings.notificationSettings,
                            weeklyReports: e.target.checked
                          })}
                        />
                      }
                      label="Weekly Reports"
                    />
                  </Grid>

                  {/* Filter Sensitivity */}
                  <Grid item xs={12}>
                    <Divider sx={{ my: 2 }} />
                    <Typography variant="subtitle1" gutterBottom>
                      Content Filter Sensitivity
                    </Typography>

                    <FormControl sx={{ minWidth: 200 }}>
                      <InputLabel>Filter Sensitivity</InputLabel>
                      <Select
                        value={settings.filterSensitivity}
                        onChange={(e) => handleSettingChange('filterSensitivity', e.target.value)}
                        label="Filter Sensitivity"
                      >
                        <MenuItem value="low">Low - Basic filtering</MenuItem>
                        <MenuItem value="medium">Medium - Balanced filtering</MenuItem>
                        <MenuItem value="high">High - Strict filtering</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Box>
  );
};

export default MonitoringPage;
