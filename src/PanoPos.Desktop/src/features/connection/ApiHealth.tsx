import { createContext, useContext, useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { useAuth } from '../auth/AuthContext';
import { errorMessage } from '../../api/errors';

const healthInterval = 60_000;
type HealthState = 'checking' | 'connected' | 'offline';
type HealthContextValue = { status: HealthState; error: string; refresh: () => Promise<void> };
const HealthContext = createContext<HealthContextValue | null>(null);

export function ApiHealthProvider({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const [status, setStatus] = useState<HealthState>('checking');
  const [error, setError] = useState('');
  const pending = useRef<Promise<void> | null>(null);
  const active = useRef(false);
  const generation = useRef(0);
  const checkedAt = useRef(0);

  function refresh(): Promise<void> {
    if (pending.current) return pending.current;
    const current = generation.current;
    checkedAt.current = Date.now();
    setStatus('checking');
    setError('');
    pending.current = auth.checkHealth().then(() => {
      if (active.current && generation.current === current) setStatus('connected');
    }).catch(e => {
      if (active.current && generation.current === current) {
        setStatus('offline'); setError(errorMessage(e));
      }
    }).finally(() => { pending.current = null; });
    return pending.current;
  }

  useEffect(() => {
    active.current = true;
    void refresh();
    const checkVisible = () => {
      if (!document.hidden && Date.now() - checkedAt.current >= healthInterval) void refresh();
    };
    const online = () => { void refresh(); };
    const offline = () => {
      generation.current += 1;
      setStatus('offline'); setError('A\u011f ba\u011flant\u0131s\u0131 kesildi.');
    };
    const timer = window.setInterval(checkVisible, healthInterval);
    window.addEventListener('focus', checkVisible);
    window.addEventListener('online', online);
    window.addEventListener('offline', offline);
    document.addEventListener('visibilitychange', checkVisible);
    return () => {
      active.current = false;
      window.clearInterval(timer);
      window.removeEventListener('focus', checkVisible);
      window.removeEventListener('online', online);
      window.removeEventListener('offline', offline);
      document.removeEventListener('visibilitychange', checkVisible);
    };
  }, [auth]);

  return <HealthContext value={{ status, error, refresh }}>{children}</HealthContext>;
}

export function useApiHealth() {
  const health = useContext(HealthContext);
  if (!health) throw new Error('API health provider is missing.');
  return health;
}
