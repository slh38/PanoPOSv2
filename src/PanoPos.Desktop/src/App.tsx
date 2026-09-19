import { useSyncExternalStore } from 'react';
import { HashRouter, Navigate, Route, Routes } from 'react-router';
import { AuthContext } from './features/auth/AuthContext';
import { createAuthRuntime, type AuthRuntime } from './features/auth/runtime';
import { desktopTransport } from './tauri/transport';
import { LoginPage } from './pages/LoginPage';
import { ApiHealthProvider } from './features/connection/ApiHealth';
import { ApplicationShell } from './layouts/ApplicationShell';
import { DashboardPage } from './pages/DashboardPage';
import { QuickSalePage } from './pages/QuickSalePage';

const defaultRuntime = createAuthRuntime(desktopTransport);

export function App({ runtime = defaultRuntime }: { runtime?: AuthRuntime }) {
  return <HashRouter><AppRoutes runtime={runtime} /></HashRouter>;
}

export function AppRoutes({ runtime }: { runtime: AuthRuntime }) {
  const { session } = useSyncExternalStore(runtime.session.subscribe, runtime.session.getSnapshot);
  return (
    <AuthContext value={runtime}>
      <ApiHealthProvider>
        <Routes>
          <Route path="/login" element={session ? <Navigate to="/" replace /> : <LoginPage />} />
          <Route element={session ? <ApplicationShell key={session.oturumId} session={session} /> : <Navigate to="/login" replace />}>
            <Route index element={session && <DashboardPage name={session.adSoyad} />} />
            <Route path="/hizli-satis" element={<QuickSalePage />} />
          </Route>
          <Route path="*" element={<Navigate to={session ? '/' : '/login'} replace />} />
        </Routes>
      </ApiHealthProvider>
    </AuthContext>
  );
}
