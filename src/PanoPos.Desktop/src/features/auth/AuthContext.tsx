import { createContext, useContext } from 'react';
import type { AuthRuntime } from './runtime';

export const AuthContext = createContext<AuthRuntime | null>(null);
export function useAuth() {
  const runtime = useContext(AuthContext);
  if (!runtime) throw new Error('Auth provider is missing.');
  return runtime;
}
