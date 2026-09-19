import { useSyncExternalStore } from 'react';
import { AuthContext } from './features/auth/AuthContext';
import { createAuthRuntime, type AuthRuntime } from './features/auth/runtime';
import { desktopTransport } from './tauri/transport';
import { LoginPage } from './pages/LoginPage';
import { AuthenticatedPage } from './pages/AuthenticatedPage';

const defaultRuntime = createAuthRuntime(desktopTransport);

export function App({ runtime = defaultRuntime }: { runtime?: AuthRuntime }) {
  const { session } = useSyncExternalStore(runtime.session.subscribe, runtime.session.getSnapshot);
  return (
    <AuthContext value={runtime}>
      {session ? <AuthenticatedPage /> : <LoginPage />}
    </AuthContext>
  );
}
