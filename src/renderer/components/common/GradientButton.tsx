import React from 'react';
import { Button, ButtonProps, styled } from '@mui/material';
import { motion } from 'framer-motion';

const MotionButton = motion(Button);

const StyledGradientButton = styled(MotionButton)(({ theme, variant, color }) => {
  const getGradient = () => {
    switch (color) {
      case 'primary':
        return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
      case 'secondary':
        return 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)';
      case 'error':
        return 'linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%)';
      case 'warning':
        return 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)';
      case 'success':
        return 'linear-gradient(135deg, #56ab2f 0%, #a8e6cf 100%)';
      default:
        return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
    }
  };

  const getHoverGradient = () => {
    switch (color) {
      case 'primary':
        return 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)';
      case 'secondary':
        return 'linear-gradient(135deg, #0f8a7e 0%, #32d670 100%)';
      case 'error':
        return 'linear-gradient(135deg, #ff5252 0%, #e53e3e 100%)';
      case 'warning':
        return 'linear-gradient(135deg, #ee82ee 0%, #f04759 100%)';
      case 'success':
        return 'linear-gradient(135deg, #4a9b26 0%, #96dcc0 100%)';
      default:
        return 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)';
    }
  };

  return {
    background: variant === 'contained' ? getGradient() : 'transparent',
    border: variant === 'outlined' ? `2px solid transparent` : 'none',
    backgroundImage: variant === 'outlined' ? getGradient() : undefined,
    backgroundOrigin: 'border-box',
    backgroundClip: variant === 'outlined' ? 'padding-box, border-box' : 'padding-box',
    color: variant === 'contained' ? '#ffffff' : 'transparent',
    backgroundClip: variant === 'outlined' ? 'text, border-box' : 'padding-box',
    WebkitBackgroundClip: variant === 'outlined' ? 'text, border-box' : 'padding-box',
    borderRadius: 12,
    padding: '12px 32px',
    fontSize: '0.875rem',
    fontWeight: 600,
    textTransform: 'none',
    boxShadow: variant === 'contained' ? '0 4px 15px rgba(0, 0, 0, 0.2)' : 'none',
    position: 'relative',
    overflow: 'hidden',
    transition: 'all 0.3s ease',

    '&::before': {
      content: '""',
      position: 'absolute',
      top: 0,
      left: '-100%',
      width: '100%',
      height: '100%',
      background: 'linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent)',
      transition: 'left 0.5s ease',
    },

    '&:hover': {
      background: variant === 'contained' ? getHoverGradient() : 'transparent',
      transform: 'translateY(-2px)',
      boxShadow: variant === 'contained' ? '0 8px 25px rgba(0, 0, 0, 0.3)' : 'none',
      
      '&::before': {
        left: '100%',
      },
    },

    '&:active': {
      transform: 'translateY(0)',
    },

    '&:disabled': {
      background: theme.palette.grey[300],
      color: theme.palette.grey[500],
      transform: 'none',
      boxShadow: 'none',
    },
  };
});

interface GradientButtonProps extends Omit<ButtonProps, 'component'> {
  children: React.ReactNode;
}

const GradientButton: React.FC<GradientButtonProps> = ({
  children,
  ...props
}) => {
  return (
    <StyledGradientButton
      whileTap={{ scale: 0.98 }}
      whileHover={{ scale: 1.02 }}
      {...props}
    >
      {children}
    </StyledGradientButton>
  );
};

export default GradientButton;
