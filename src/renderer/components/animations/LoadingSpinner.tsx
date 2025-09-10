import React from 'react';
import { Box, styled } from '@mui/material';
import { motion } from 'framer-motion';

const SpinnerContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: '100%',
  height: '100%',
  minHeight: 60,
}));

const SpinnerSvg = styled(motion.svg)(({ theme }) => ({
  width: 40,
  height: 40,
}));

const SpinnerCircle = styled(motion.circle)(({ theme }) => ({
  fill: 'none',
  stroke: theme.palette.primary.main,
  strokeWidth: 3,
  strokeLinecap: 'round',
}));

interface LoadingSpinnerProps {
  size?: number;
  color?: string;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 40,
  color,
}) => {
  const spinTransition = {
    duration: 1.5,
    ease: "linear",
    repeat: Infinity,
  };

  const drawTransition = {
    duration: 2,
    ease: "easeInOut",
    repeat: Infinity,
    repeatType: "reverse" as const,
  };

  return (
    <SpinnerContainer>
      <SpinnerSvg
        width={size}
        height={size}
        viewBox="0 0 50 50"
        animate={{ rotate: 360 }}
        transition={spinTransition}
      >
        <SpinnerCircle
          cx="25"
          cy="25"
          r="20"
          initial={{ pathLength: 0 }}
          animate={{ pathLength: 1 }}
          transition={drawTransition}
          style={{ stroke: color }}
        />
      </SpinnerSvg>
    </SpinnerContainer>
  );
};

export default LoadingSpinner;
