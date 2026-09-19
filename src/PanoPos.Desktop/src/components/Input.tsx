import { useId } from 'react';
import type { InputHTMLAttributes, Ref } from 'react';
import './components.css';

type InputProps = InputHTMLAttributes<HTMLInputElement> & { label: string; ref?: Ref<HTMLInputElement> };

export function Input({ label, id, className = '', ...props }: InputProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  return (
    <div className="field">
      <label htmlFor={inputId}>{label}</label>
      <input id={inputId} className={`input ${className}`} {...props} />
    </div>
  );
}
