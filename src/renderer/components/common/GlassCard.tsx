import React from 'react';
import { Card, CardProps, styled } from '@mui/material';
import { motion, MotionProps } from 'framer-motion';

const MotionCard = motion(Card);

interface GlassCardProps extends Omit<CardProps, 'component'>, MotionProps {
  children: React.ReactNode;
  blur?: number;
  opacity?: number;
  borderOpacity?: number;
}

const StyledGlassCard = styled(MotionCard)<{
  blur?: number;
  opacity?: number;
  borderOpacity?: number;
}>(({ theme, blur = 20, opacity = 0.9, borderOpacity = 0.2 }) => ({
  background: `rgba(255, 255, 255, ${opacity})`,
  backdropFilter: `blur(${blur}px)`,
  WebkitBackdropFilter: `blur(${blur}px)`,
  border: `1px solid rgba(255, 255, 255, ${borderOpacity})`,
  borderRadius: 16,
  overflow: 'hidden',
  position: 'relative',
  transition: 'all 0.3s ease',
  
  '&::before': {
    content: '""',
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: '1px',
    background: 'linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.6), transparent)',
    opacity: 0,
    transition: 'opacity 0.3s ease',
  },
  
  '&:hover': {
    transform: 'translateY(-4px)',
    boxShadow: '0 20px 40px rgba(0, 0, 0, 0.1)',
    border: `1px solid rgba(103, 126, 234, ${borderOpacity + 0.2})`,
    
    '&::before': {
      opacity: 1,
    },
  },

  // Dark mode support
  [theme.breakpoints.down('sm')]: {
    borderRadius: 12,
  },

  // Add subtle inner glow
  '&::after': {
    content: '""',
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    background: 'radial-gradient(circle at 50% 0%, rgba(103, 126, 234, 0.1), transparent 50%)',
    opacity: 0,
    transition: 'opacity 0.3s ease',
    pointerEvents: 'none',
  },

  '&:hover::after': {
    opacity: 1,
  },
}));

const GlassCard: React.FC<GlassCardProps> = ({
  children,
  blur = 20,
  opacity = 0.9,
  borderOpacity = 0.2,
  ...props
}) => {
  const cardVariants = {
    hidden: {
      opacity: 0,
      y: 20,
      scale: 0.95,
    },
    visible: {
      opacity: 1,
      y: 0,
      scale: 1,
      transition: {
        duration: 0.5,
        ease: [0.25, 0.46, 0.45, 0.94],
      },
    },
  };

  return (
    <StyledGlassCard
      variants={cardVariants}
      initial="hidden"
      animate="visible"
      blur={blur}
      opacity={opacity}
      borderOpacity={borderOpacity}
      {...props}
    >
      {children}
    </StyledGlassCard>
  );
};

export default GlassCard;
