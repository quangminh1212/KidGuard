import React from 'react';
import { motion, MotionProps } from 'framer-motion';
import { Box, BoxProps } from '@mui/material';

const MotionBox = motion(Box);

interface FadeInUpProps extends Omit<BoxProps, 'component'>, MotionProps {
  children: React.ReactNode;
  delay?: number;
  duration?: number;
  distance?: number;
}

const FadeInUp: React.FC<FadeInUpProps> = ({
  children,
  delay = 0,
  duration = 0.6,
  distance = 30,
  ...props
}) => {
  const variants = {
    hidden: {
      opacity: 0,
      y: distance,
    },
    visible: {
      opacity: 1,
      y: 0,
      transition: {
        duration,
        delay,
        ease: [0.25, 0.46, 0.45, 0.94],
      },
    },
  };

  return (
    <MotionBox
      variants={variants}
      initial="hidden"
      animate="visible"
      {...props}
    >
      {children}
    </MotionBox>
  );
};

export default FadeInUp;
