import { useEffect, useRef, useState } from 'react';
import { Building2, Monitor } from 'lucide-react';
import { Outlet, useLocation } from 'react-router';
import type { SessionInfo } from '../features/auth/contracts';
import { Sidebar } from './Sidebar';
import { UserMenu } from './UserMenu';
import { ConnectionStatus } from './ConnectionStatus';
import './shell.css';

export function ApplicationShell({ session }: { session: SessionInfo }) {
  const [collapsed, setCollapsed] = useState(false);
  const branch = session.subeler.find(item => item.subeId === session.subeId);
  const location = useLocation();
  const content = useRef<HTMLElement>(null);
  useEffect(() => {
    content.current?.querySelector<HTMLElement>('h1')?.focus();
    content.current?.scrollTo?.(0, 0);
  }, [location.pathname]);

  return <div className={`application-shell${collapsed ? ' application-shell--collapsed' : ''}`}>
    <a className="skip-link" href="#main-content" onClick={event => {
      event.preventDefault(); content.current?.focus();
    }}>{'\u0130\u00e7eri\u011fe ge\u00e7'}</a>
    <Sidebar collapsed={collapsed} onToggle={() => setCollapsed(!collapsed)} />
    <div className="shell-workspace">
      <header className="shell-topbar">
        <div className="session-context">
          <span title={branch?.ad ?? `\u015eube ${session.subeId}`}><Building2 aria-hidden="true" />
            <span>{branch?.ad ?? `\u015eube ${session.subeId}`}</span></span>
          <span><Monitor aria-hidden="true" /><span>Cihaz {session.cihazId}</span></span>
        </div>
        <ConnectionStatus />
        <UserMenu name={session.adSoyad} />
      </header>
      <main id="main-content" className="shell-content" ref={content} tabIndex={-1}><Outlet /></main>
    </div>
  </div>;
}
