import React, { useState } from 'react';
import {
  Box,
  Grid,
  Typography,
  Stack,
  Button,
  TextField,
  Switch,
  FormControlLabel,
  Chip,
  Alert,
  Paper
} from '@mui/material';
import {
  Dashboard,
  Security,
  ChildCare,
  Warning,
  TrendingUp,
  Computer,
  Schedule,
  PlayArrow,
  Stop,
  MonitorHeart,
  Shield,
  Assessment,
  Notifications
} from '@mui/icons-material';
import { motion } from 'framer-motion';
import StatCard from '../components/common/StatCard';
import GlassCard from '../components/common/GlassCard';
import GradientButton from '../components/common/GradientButton';
import FadeInUp from '../components/animations/FadeInUp';
import AnimatedCard from '../components/animations/AnimatedCard';
import LoadingSpinner from '../components/animations/LoadingSpinner';

const UITestPage: React.FC = () => {
  const [darkMode, setDarkMode] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleLoadingTest = () => {
    setLoading(true);
    setTimeout(() => setLoading(false), 3000);
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
              UI Components Test
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Testing modern UI components and animations
            </Typography>
          </Box>
        </FadeInUp>

        {/* Controls */}
        <FadeInUp delay={0.2}>
          <Paper sx={{ p: 2, mb: 4, background: 'rgba(255, 255, 255, 0.8)', backdropFilter: 'blur(10px)' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <FormControlLabel
                control={<Switch checked={darkMode} onChange={(e) => setDarkMode(e.target.checked)} />}
                label="Dark Mode"
              />
              <GradientButton onClick={handleLoadingTest} disabled={loading}>
                {loading ? 'Loading...' : 'Test Loading'}
              </GradientButton>
              {loading && <LoadingSpinner size={24} />}
            </Stack>
          </Paper>
        </FadeInUp>

        {/* Stat Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.3}>
              <StatCard
                title="Total Users"
                value="1,247"
                icon={Computer}
                color="primary"
                change={{ value: '+12%', positive: true }}
              />
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.4}>
              <StatCard
                title="Active Alerts"
                value="23"
                icon={Warning}
                color="warning"
                change={{ value: '-5%', positive: true }}
              />
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <FadeInUp delay={0.5}>
              <StatCard
                title="Monitoring Time"
                value="4h 32m"
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
                value="High"
                icon={Shield}
                color="success"
              />
            </FadeInUp>
          </Grid>
        </Grid>

        {/* Glass Cards and Buttons */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} md={6}>
            <FadeInUp delay={0.7}>
              <GlassCard>
                <Box sx={{ p: 3 }}>
                  <Typography variant="h6" sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <MonitorHeart color="primary" />
                    Gradient Buttons
                  </Typography>
                  
                  <Stack spacing={2}>
                    <GradientButton fullWidth size="large" startIcon={<PlayArrow />}>
                      Primary Button
                    </GradientButton>
                    
                    <GradientButton fullWidth variant="outlined" color="secondary" startIcon={<Stop />}>
                      Secondary Outlined
                    </GradientButton>
                    
                    <Stack direction="row" spacing={2}>
                      <GradientButton color="error" startIcon={<Warning />} sx={{ flex: 1 }}>
                        Error
                      </GradientButton>
                      <GradientButton color="success" startIcon={<Shield />} sx={{ flex: 1 }}>
                        Success
                      </GradientButton>
                    </Stack>
                  </Stack>
                </Box>
              </GlassCard>
            </FadeInUp>
          </Grid>

          <Grid item xs={12} md={6}>
            <FadeInUp delay={0.8}>
              <AnimatedCard>
                <Box sx={{ p: 3 }}>
                  <Typography variant="h6" sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Assessment color="primary" />
                    Form Elements
                  </Typography>
                  
                  <Stack spacing={2}>
                    <TextField
                      fullWidth
                      label="Username"
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          background: 'rgba(255, 255, 255, 0.8)',
                          backdropFilter: 'blur(10px)',
                        },
                      }}
                    />
                    
                    <TextField
                      fullWidth
                      label="Password"
                      type="password"
                      variant="outlined"
                      sx={{
                        '& .MuiOutlinedInput-root': {
                          background: 'rgba(255, 255, 255, 0.8)',
                          backdropFilter: 'blur(10px)',
                        },
                      }}
                    />
                    
                    <Stack direction="row" spacing={1} flexWrap="wrap">
                      <Chip label="Active" color="success" />
                      <Chip label="Warning" color="warning" />
                      <Chip label="Error" color="error" />
                      <Chip label="Info" color="info" />
                    </Stack>
                  </Stack>
                </Box>
              </AnimatedCard>
            </FadeInUp>
          </Grid>
        </Grid>

        {/* Alerts */}
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <FadeInUp delay={0.9}>
              <Alert severity="success" sx={{ borderRadius: 2, mb: 2 }}>
                This is a success alert with modern styling!
              </Alert>
            </FadeInUp>
          </Grid>
          
          <Grid item xs={12} md={6}>
            <FadeInUp delay={1.0}>
              <Alert severity="warning" sx={{ borderRadius: 2, mb: 2 }}>
                This is a warning alert with glassmorphism effects!
              </Alert>
            </FadeInUp>
          </Grid>
        </Grid>

        {/* Animation Demo */}
        <FadeInUp delay={1.1}>
          <GlassCard sx={{ mt: 4 }}>
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Animation Demo
              </Typography>
              <motion.div
                animate={{
                  scale: [1, 1.1, 1],
                  rotate: [0, 5, -5, 0],
                }}
                transition={{
                  duration: 2,
                  repeat: Infinity,
                  repeatType: "reverse"
                }}
              >
                <Shield sx={{ fontSize: 60, color: 'primary.main' }} />
              </motion.div>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
                Continuous animation with framer-motion
              </Typography>
            </Box>
          </GlassCard>
        </FadeInUp>
      </Box>
    </motion.div>
  );
};

export default UITestPage;
