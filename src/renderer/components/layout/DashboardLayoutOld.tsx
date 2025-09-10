import React, { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  List,
  Typography,
  Divider,
  IconButton,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Avatar,
  Menu,
  MenuItem,
  Badge,
  Tooltip
} from '@mui/material';
import {
  Menu as MenuIcon,
  Dashboard,
  MonitorHeart,
  ChildCare,
  Warning,
  Assessment,
  Settings,
  AccountCircle,
  Logout,
  Security,
  Notifications
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { useMonitoring } from '../../contexts/MonitoringContext';

const drawerWidth = 240;

interface NavigationItem {
  text: string;
  icon: React.ReactElement;
  path: string;
  badge?: number;
}

const DashboardLayout: React.FC = () => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const { isActive, stats } = useMonitoring();

  const navigationItems: NavigationItem[] = [
    {
      text: 'Dashboard',
      icon: <Dashboard />,
      path: '/dashboard'
    },
    {
      text: 'Monitoring',
      icon: <MonitorHeart color={isActive ? 'success' : 'disabled'} />,
      path: '/monitoring'
    },
    {
      text: 'Children',
      icon: <ChildCare />,
      path: '/children'
    },
    {
      text: 'Alerts',
      icon: <Warning />,
      path: '/alerts',
      badge: stats?.alertsToday || 0
    },
    {
      text: 'Reports',
      icon: <Assessment />,
      path: '/reports'
    },
    {
      text: 'Settings',
      icon: <Settings />,
      path: '/settings'
    }
  ];

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = async () => {
    handleMenuClose();
    await logout();
  };

  const handleNavigate = (path: string) => {
    navigate(path);
    setMobileOpen(false);
  };

  const drawer = (
    <Box>
      {/* Logo and Title */}
      <Box sx={{ p: 2, textAlign: 'center' }}>
        <Box
          sx={{
            width: 48,
            height: 48,
            borderRadius: '50%',
            backgroundColor: 'primary.main',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 8px',
            color: 'white'
          }}
        >
          <Security />
        </Box>
        <Typography variant="h6" fontWeight="bold" color="primary">
          ChildGuard
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Child Protection System
        </Typography>
      </Box>

      <Divider />

      {/* Navigation Items */}
      <List>
        {navigationItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => handleNavigate(item.path)}
              sx={{
                mx: 1,
                borderRadius: 1,
                '&.Mui-selected': {
                  backgroundColor: 'primary.main',
                  color: 'white',
                  '&:hover': {
                    backgroundColor: 'primary.dark'
                  },
                  '& .MuiListItemIcon-root': {
                    color: 'white'
                  }
                }
              }}
            >
              <ListItemIcon>
                {item.badge && item.badge > 0 ? (
                  <Badge badgeContent={item.badge} color="error">
                    {item.icon}
                  </Badge>
                ) : (
                  item.icon
                )}
              </ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Divider sx={{ mt: 'auto' }} />

      {/* Monitoring Status */}
      <Box sx={{ p: 2 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            p: 1,
            borderRadius: 1,
            backgroundColor: isActive ? 'success.light' : 'grey.100',
            color: isActive ? 'success.contrastText' : 'text.secondary'
          }}
        >
          <Box
            sx={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              backgroundColor: isActive ? 'success.main' : 'grey.400',
              mr: 1
            }}
          />
          <Typography variant="caption">
            {isActive ? 'Monitoring Active' : 'Monitoring Inactive'}
          </Typography>
        </Box>
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', height: '100vh' }}>
      {/* App Bar */}
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          backgroundColor: 'white',
          color: 'text.primary',
          boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {navigationItems.find(item => item.path === location.pathname)?.text || 'Dashboard'}
          </Typography>

          {/* Notifications */}
          <Tooltip title="Notifications">
            <IconButton color="inherit" sx={{ mr: 1 }}>
              <Badge badgeContent={stats?.alertsToday || 0} color="error">
                <Notifications />
              </Badge>
            </IconButton>
          </Tooltip>

          {/* User Menu */}
          <Tooltip title="Account">
            <IconButton
              onClick={handleMenuClick}
              color="inherit"
            >
              <Avatar sx={{ width: 32, height: 32, backgroundColor: 'primary.main' }}>
                {user?.username?.charAt(0).toUpperCase() || 'U'}
              </Avatar>
            </IconButton>
          </Tooltip>

          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
            anchorOrigin={{
              vertical: 'bottom',
              horizontal: 'right'
            }}
            transformOrigin={{
              vertical: 'top',
              horizontal: 'right'
            }}
          >
            <MenuItem disabled>
              <AccountCircle sx={{ mr: 1 }} />
              {user?.username || 'User'}
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <Logout sx={{ mr: 1 }} />
              Logout
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      {/* Drawer */}
      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      >
        {/* Mobile drawer */}
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true // Better open performance on mobile
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth
            }
          }}
        >
          {drawer}
        </Drawer>

        {/* Desktop drawer */}
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
              borderRight: '1px solid',
              borderColor: 'divider'
            }
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          mt: '64px', // AppBar height
          height: 'calc(100vh - 64px)',
          overflow: 'auto',
          backgroundColor: 'background.default'
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default DashboardLayout;
