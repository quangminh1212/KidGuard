import React from 'react';
import { Box, Typography, Card, CardContent, List, ListItem, ListItemIcon, ListItemText, Chip } from '@mui/material';
import { Warning, Info, Error, CheckCircle } from '@mui/icons-material';

const AlertsPage: React.FC = () => {
  const mockAlerts = [
    {
      id: 1,
      type: 'inappropriate_content',
      title: 'Inappropriate Content Detected',
      message: 'Detected high-level inappropriate content in browser',
      severity: 'high',
      timestamp: new Date(),
      isRead: false
    },
    {
      id: 2,
      type: 'time_violation',
      title: 'Time Restriction Violation',
      message: 'Computer usage outside allowed hours',
      severity: 'medium',
      timestamp: new Date(Date.now() - 3600000),
      isRead: true
    }
  ];

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'critical': return <Error color="error" />;
      case 'high': return <Warning color="error" />;
      case 'medium': return <Warning color="warning" />;
      case 'low': return <Info color="info" />;
      default: return <CheckCircle color="success" />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return 'error';
      case 'high': return 'error';
      case 'medium': return 'warning';
      case 'low': return 'info';
      default: return 'success';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Security Alerts
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Monitor and review security alerts and notifications.
        </Typography>
      </Box>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Recent Alerts
          </Typography>
          
          <List>
            {mockAlerts.map((alert) => (
              <ListItem key={alert.id} sx={{ 
                border: '1px solid',
                borderColor: 'divider',
                borderRadius: 1,
                mb: 1,
                backgroundColor: alert.isRead ? 'background.paper' : 'action.hover'
              }}>
                <ListItemIcon>
                  {getSeverityIcon(alert.severity)}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {alert.title}
                      <Chip 
                        label={alert.severity} 
                        size="small" 
                        color={getSeverityColor(alert.severity) as any}
                      />
                      {!alert.isRead && (
                        <Chip label="New" size="small" color="primary" />
                      )}
                    </Box>
                  }
                  secondary={
                    <Box>
                      <Typography variant="body2" color="text.secondary">
                        {alert.message}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {alert.timestamp.toLocaleString()}
                      </Typography>
                    </Box>
                  }
                />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
    </Box>
  );
};

export default AlertsPage;
