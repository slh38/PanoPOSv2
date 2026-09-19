import { useState, useSyncExternalStore } from 'react';
import { Button } from '../components/Button';
import { Panel } from '../components/Panel';
import { useAuth } from '../features/auth/AuthContext';
import './auth.css';

export function AuthenticatedPage() {
  const auth = useAuth();
  const { session } = useSyncExternalStore(auth.session.subscribe, auth.session.getSnapshot);
  const [pending, setPending] = useState(false);
  if (!session) return null;
  const branch = session.subeler.find(item => item.subeId === session.subeId);
  async function logout() {
    if (pending) return;
    setPending(true);
    try { await auth.logout(); } finally { setPending(false); }
  }
  return (
    <main className="authenticated-page">
      <Panel className="authenticated-card">
        <div className="brand-wordmark">Pano<span>POS</span></div>
        <h1>{'Ho\u015f geldiniz, '}{session.adSoyad}</h1>
        <dl><div><dt>{'\u015eube'}</dt><dd>{branch?.ad ?? session.subeId}</dd></div>
          <div><dt>Cihaz</dt><dd>{session.cihazId}</dd></div></dl>
        <p>{'Ana Men\u00fc bir sonraki g\u00f6revde olu\u015fturulacak.'}</p>
        <Button variant="secondary" onClick={logout} disabled={pending}>
          {pending ? '\u00c7\u0131k\u0131\u015f yap\u0131l\u0131yor...' : '\u00c7\u0131k\u0131\u015f Yap'}
        </Button>
      </Panel>
    </main>
  );
}
