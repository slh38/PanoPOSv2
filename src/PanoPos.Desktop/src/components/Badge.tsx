import type { HTMLAttributes } from 'react';
import './components.css';

type BadgeProps = HTMLAttributes<HTMLSpanElement> & { variant?: 'neutral' | 'success' };

export function Badge({ variant = 'neutral', className = '', ...props }: BadgeProps) {
  return <span className={`badge badge--${variant} ${className}`} {...props} />;
}
