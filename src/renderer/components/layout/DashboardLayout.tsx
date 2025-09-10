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
  Tooltip,
  Paper,
  Chip,
  Stack
} from '@mui/material';
import {
  Menu as MenuIcon,
  Dashboard,
  MonitorHeart,
  ChildCare,
  Notifications,
  Assessment,
  Settings,
  Logout,
  AccountCircle,
  Security,
  Shield,
  Close,
  Palette
} from '@mui/icons-material';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../../contexts/AuthContext';
import { useNotification } from '../../contexts/NotificationContext';

const drawerWidth = 280;

const DashboardLayout: React.FC = () => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const { showSuccess } = useNotification();

  const menuItems = [
    { text: 'Dashboard', icon: Dashboard, path: '/dashboard' },
    { text: 'Monitoring', icon: MonitorHeart, path: '/monitoring' },
    { text: 'Children', icon: ChildCare, path: '/children' },
    { text: 'Alerts', icon: Notifications, path: '/alerts', badge: 3 },
    { text: 'Reports', icon: Assessment, path: '/reports' },
    { text: 'Settings', icon: Settings, path: '/settings' },
    { text: 'UI Test', icon: Palette, path: '/ui-test' },
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
    showSuccess('Logged out successfully');
    navigate('/login');
  };

  const isActive = (path: string) => location.pathname === path;

  const sidebarVariants = {
    hidden: { x: -drawerWidth },
    visible: { 
      x: 0,
      transition: {
        type: "spring",
        stiffness: 300,
        damping: 30
      }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, x: -20 },
    visible: (i: number) => ({
      opacity: 1,
      x: 0,
      transition: {
        delay: i * 0.1,
        duration: 0.3
      }
    })
  };

  const drawer = (
    <motion.div
      variants={sidebarVariants}
      initial="hidden"
      animate="visible"
    >
      <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        {/* Logo Section */}
        <Box
          sx={{
            p: 3,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            textAlign: 'center',
          }}
        >
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ delay: 0.2, type: "spring", stiffness: 200 }}
          >
            <Shield sx={{ fontSize: 40, mb: 1 }} />
          </motion.div>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            ChildGuard
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.9 }}>
            Child Protection System
          </Typography>
        </Box>

        {/* Navigation Menu */}
        <Box sx={{ flex: 1, p: 2 }}>
          <List sx={{ p: 0 }}>
            {menuItems.map((item, index) => (
              <motion.div
                key={item.text}
                custom={index}
                variants={itemVariants}
                initial="hidden"
                animate="visible"
              >
                <ListItem disablePadding sx={{ mb: 0.5 }}>
                  <ListItemButton
                    onClick={() => navigate(item.path)}
                    selected={isActive(item.path)}
                    sx={{
                      borderRadius: 2,
                      mx: 1,
                      transition: 'all 0.3s ease',
                      '&:hover': {
                        background: 'rgba(102, 126, 234, 0.08)',
                        transform: 'translateX(4px)',
                      },
                      '&.Mui-selected': {
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                        color: 'white',
                        '&:hover': {
                          background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
                        },
                        '& .MuiListItemIcon-root': {
                          color: 'white',
                        },
                      },
                    }}
                  >
                    <ListItemIcon
                      sx={{
                        minWidth: 40,
                        color: isActive(item.path) ? 'white' : 'text.secondary',
                      }}
                    >
                      {item.badge ? (
                        <Badge badgeContent={item.badge} color="error">
                          <item.icon />
                        </Badge>
                      ) : (
                        <item.icon />
                      )}
                    </ListItemIcon>
                    <ListItemText
                      primary={item.text}
                      primaryTypographyProps={{
                        fontSize: '0.875rem',
                        fontWeight: isActive(item.path) ? 600 : 500,
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              </motion.div>
            ))}
          </List>
        </Box>

        {/* User Info */}
        <Box sx={{ p: 2 }}>
          <Paper
            sx={{
              p: 2,
              background: 'rgba(102, 126, 234, 0.05)',
              border: '1px solid rgba(102, 126, 234, 0.1)',
              borderRadius: 2,
            }}
          >
            <Stack direction="row" spacing={2} alignItems="center">
              <Avatar
                sx={{
                  width: 40,
                  height: 40,
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                }}
              >
                <AccountCircle />
              </Avatar>
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {user?.username || 'Admin'}
                </Typography>
                <Chip
                  label="Administrator"
                  size="small"
                  sx={{
                    height: 20,
                    fontSize: '0.7rem',
                    background: 'rgba(102, 126, 234, 0.1)',
                    color: 'primary.main',
                  }}
                />
              </Box>
            </Stack>
          </Paper>
        </Box>
      </Box>
    </motion.div>
  );

  return (
    <Box sx={{ display: 'flex', height: '100vh' }}>
      {/* App Bar */}
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          background: 'rgba(255, 255, 255, 0.95)',
          backdropFilter: 'blur(20px)',
          borderBottom: '1px solid rgba(0, 0, 0, 0.08)',
          boxShadow: 'none',
          color: 'text.primary',
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
          
          <Box sx={{ flexGrow: 1 }} />
          
          {/* User Menu */}
          <Tooltip title="Account settings">
            <IconButton
              onClick={handleMenuClick}
              sx={{
                background: 'rgba(102, 126, 234, 0.1)',
                '&:hover': {
                  background: 'rgba(102, 126, 234, 0.2)',
                },
              }}
            >
              <AccountCircle />
            </IconButton>
          </Tooltip>
          
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
            PaperProps={{
              sx: {
                background: 'rgba(255, 255, 255, 0.95)',
                backdropFilter: 'blur(20px)',
                border: '1px solid rgba(0, 0, 0, 0.08)',
                borderRadius: 2,
                mt: 1,
              },
            }}
          >
            <MenuItem onClick={handleMenuClose}>
              <ListItemIcon>
                <AccountCircle fontSize="small" />
              </ListItemIcon>
              Profile
            </MenuItem>
            <MenuItem onClick={() => { handleMenuClose(); navigate('/settings'); }}>
              <ListItemIcon>
                <Settings fontSize="small" />
              </ListItemIcon>
              Settings
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <Logout fontSize="small" />
              </ListItemIcon>
              Logout
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      {/* Sidebar */}
      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      >
        {/* Mobile drawer */}
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
              background: 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              borderRight: '1px solid rgba(0, 0, 0, 0.08)',
            },
          }}
        >
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 1 }}>
            <IconButton onClick={handleDrawerToggle}>
              <Close />
            </IconButton>
          </Box>
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
              background: 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              borderRight: '1px solid rgba(0, 0, 0, 0.08)',
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      {/* Main content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          mt: 8,
          overflow: 'auto',
          background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
          minHeight: 'calc(100vh - 64px)',
        }}
      >
        <AnimatePresence mode="wait">
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.3 }}
          >
            <Outlet />
          </motion.div>
        </AnimatePresence>
      </Box>
    </Box>
  );
};

export default DashboardLayout;
