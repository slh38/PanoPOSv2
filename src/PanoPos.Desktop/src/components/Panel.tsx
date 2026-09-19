import type { HTMLAttributes } from 'react';
import './components.css';

export function Panel({ className = '', ...props }: HTMLAttributes<HTMLElement>) {
  return <section className={`panel ${className}`} {...props} />;
}
