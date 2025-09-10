import React from 'react';
import { Card, CardProps, styled } from '@mui/material';
import { motion, MotionProps } from 'framer-motion';

const MotionCard = motion(Card);

const StyledAnimatedCard = styled(MotionCard)(({ theme }) => ({
  background: 'rgba(255, 255, 255, 0.9)',
  backdropFilter: 'blur(20px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
  borderRadius: 16,
  overflow: 'hidden',
  position: 'relative',
  
  '&::before': {
    content: '""',
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: '1px',
    background: 'linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent)',
  },
  
  '&:hover': {
    '&::before': {
      background: 'linear-gradient(90deg, transparent, rgba(103, 126, 234, 0.6), transparent)',
    },
  },
}));

interface AnimatedCardProps extends Omit<CardProps, 'component'>, MotionProps {
  children: React.ReactNode;
  delay?: number;
  duration?: number;
  hover?: boolean;
}

const AnimatedCard: React.FC<AnimatedCardProps> = ({
  children,
  delay = 0,
  duration = 0.5,
  hover = true,
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
        duration,
        delay,
        ease: [0.25, 0.46, 0.45, 0.94],
      },
    },
  };

  const hoverVariants = hover ? {
    hover: {
      y: -8,
      scale: 1.02,
      transition: {
        duration: 0.3,
        ease: [0.25, 0.46, 0.45, 0.94],
      },
    },
  } : {};

  return (
    <StyledAnimatedCard
      variants={cardVariants}
      initial="hidden"
      animate="visible"
      whileHover={hover ? "hover" : undefined}
      {...hoverVariants}
      {...props}
    >
      {children}
    </StyledAnimatedCard>
  );
};

export default AnimatedCard;
