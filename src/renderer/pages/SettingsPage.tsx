import React from 'react';
import { 
  Box, 
  Typography, 
  Card, 
  CardContent, 
  Grid, 
  Switch, 
  FormControlLabel,
  Divider,
  Button,
  TextField
} from '@mui/material';
import { Settings, Security, Notifications, Backup } from '@mui/icons-material';

const SettingsPage: React.FC = () => {
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Settings
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Configure application settings and preferences.
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* General Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Settings sx={{ mr: 1, verticalAlign: 'middle' }} />
                General Settings
              </Typography>
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Auto-start with Windows"
              />
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Minimize to system tray"
              />
              
              <FormControlLabel
                control={<Switch />}
                label="Show desktop notifications"
              />
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Enable automatic updates"
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Security Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Security sx={{ mr: 1, verticalAlign: 'middle' }} />
                Security Settings
              </Typography>
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Require password for settings"
              />
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Enable data encryption"
              />
              
              <FormControlLabel
                control={<Switch />}
                label="Two-factor authentication"
              />
              
              <Divider sx={{ my: 2 }} />
              
              <Button variant="outlined" fullWidth sx={{ mb: 1 }}>
                Change Password
              </Button>
              
              <Button variant="outlined" fullWidth>
                Export Security Logs
              </Button>
            </CardContent>
          </Card>
        </Grid>

        {/* Notification Settings */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Notifications sx={{ mr: 1, verticalAlign: 'middle' }} />
                Notification Settings
              </Typography>
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Real-time alerts"
              />
              
              <FormControlLabel
                control={<Switch />}
                label="Email notifications"
              />
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Daily summary reports"
              />
              
              <FormControlLabel
                control={<Switch />}
                label="Weekly reports"
              />
              
              <Divider sx={{ my: 2 }} />
              
              <TextField
                fullWidth
                label="Email Address"
                type="email"
                variant="outlined"
                size="small"
                sx={{ mb: 2 }}
              />
              
              <Button variant="outlined" fullWidth>
                Test Email Configuration
              </Button>
            </CardContent>
          </Card>
        </Grid>

        {/* Backup & Data */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <Backup sx={{ mr: 1, verticalAlign: 'middle' }} />
                Backup & Data
              </Typography>
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Automatic backups"
              />
              
              <FormControlLabel
                control={<Switch defaultChecked />}
                label="Compress backup files"
              />
              
              <Divider sx={{ my: 2 }} />
              
              <Button variant="outlined" fullWidth sx={{ mb: 1 }}>
                Create Backup Now
              </Button>
              
              <Button variant="outlined" fullWidth sx={{ mb: 1 }}>
                Restore from Backup
              </Button>
              
              <Button variant="outlined" color="error" fullWidth>
                Clear All Data
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default SettingsPage;
