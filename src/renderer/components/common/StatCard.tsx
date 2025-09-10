import React from 'react';
import { Card, CardContent, Typography, Box, styled } from '@mui/material';
import { motion } from 'framer-motion';
import { SvgIconComponent } from '@mui/icons-material';

const MotionCard = motion(Card);

const StyledStatCard = styled(MotionCard)(({ theme }) => ({
  background: 'rgba(255, 255, 255, 0.9)',
  backdropFilter: 'blur(20px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
  borderRadius: 16,
  overflow: 'hidden',
  position: 'relative',
  cursor: 'pointer',
  transition: 'all 0.3s ease',
  
  '&::before': {
    content: '""',
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: '3px',
    background: 'linear-gradient(90deg, #667eea, #764ba2)',
    transform: 'scaleX(0)',
    transformOrigin: 'left',
    transition: 'transform 0.3s ease',
  },
  
  '&:hover': {
    transform: 'translateY(-8px)',
    boxShadow: '0 20px 40px rgba(0, 0, 0, 0.15)',
    
    '&::before': {
      transform: 'scaleX(1)',
    },
  },
}));

const IconContainer = styled(Box)(({ theme }) => ({
  width: 60,
  height: 60,
  borderRadius: 16,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
  color: '#ffffff',
  marginBottom: theme.spacing(2),
  position: 'relative',
  overflow: 'hidden',
  
  '&::before': {
    content: '""',
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    background: 'linear-gradient(45deg, rgba(255, 255, 255, 0.1), transparent)',
    opacity: 0,
    transition: 'opacity 0.3s ease',
  },
  
  '&:hover::before': {
    opacity: 1,
  },
}));

const ValueText = styled(Typography)(({ theme }) => ({
  fontSize: '2rem',
  fontWeight: 700,
  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
  WebkitBackgroundClip: 'text',
  WebkitTextFillColor: 'transparent',
  backgroundClip: 'text',
  marginBottom: theme.spacing(0.5),
}));

const LabelText = styled(Typography)(({ theme }) => ({
  fontSize: '0.875rem',
  fontWeight: 500,
  color: theme.palette.text.secondary,
  textTransform: 'uppercase',
  letterSpacing: '0.5px',
}));

const ChangeIndicator = styled(Box)<{ positive?: boolean }>(({ theme, positive }) => ({
  display: 'flex',
  alignItems: 'center',
  gap: theme.spacing(0.5),
  marginTop: theme.spacing(1),
  padding: theme.spacing(0.5, 1),
  borderRadius: 8,
  fontSize: '0.75rem',
  fontWeight: 600,
  background: positive 
    ? 'rgba(76, 175, 80, 0.1)' 
    : 'rgba(244, 67, 54, 0.1)',
  color: positive 
    ? theme.palette.success.main 
    : theme.palette.error.main,
}));

interface StatCardProps {
  title: string;
  value: string | number;
  icon: SvgIconComponent;
  change?: {
    value: string;
    positive: boolean;
  };
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'error';
  onClick?: () => void;
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon: Icon,
  change,
  color = 'primary',
  onClick,
}) => {
  const getGradient = () => {
    switch (color) {
      case 'primary':
        return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
      case 'secondary':
        return 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)';
      case 'success':
        return 'linear-gradient(135deg, #56ab2f 0%, #a8e6cf 100%)';
      case 'warning':
        return 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)';
      case 'error':
        return 'linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%)';
      default:
        return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
    }
  };

  const cardVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { 
      opacity: 1, 
      y: 0,
      transition: {
        duration: 0.5,
        ease: [0.25, 0.46, 0.45, 0.94],
      },
    },
  };

  return (
    <StyledStatCard
      variants={cardVariants}
      initial="hidden"
      animate="visible"
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      onClick={onClick}
    >
      <CardContent>
        <IconContainer sx={{ background: getGradient() }}>
          <Icon sx={{ fontSize: 28 }} />
        </IconContainer>
        
        <ValueText>{value}</ValueText>
        <LabelText>{title}</LabelText>
        
        {change && (
          <ChangeIndicator positive={change.positive}>
            {change.positive ? '↗' : '↘'} {change.value}
          </ChangeIndicator>
        )}
      </CardContent>
    </StyledStatCard>
  );
};

export default StatCard;
