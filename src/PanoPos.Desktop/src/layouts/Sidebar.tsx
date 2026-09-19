import { PanelLeftClose, PanelLeftOpen } from 'lucide-react';
import { NavLink } from 'react-router';
import { navigationItems } from './navigation';
import { version } from '../../package.json';

export function Sidebar({ collapsed, onToggle }: { collapsed: boolean; onToggle: () => void }) {
  return <aside className="sidebar" aria-label="Ana navigasyon">
    <div className="sidebar-brand">
      <button className="sidebar-toggle" onClick={onToggle} aria-expanded={!collapsed} aria-controls="main-navigation"
        aria-label={collapsed ? 'Men\u00fcy\u00fc geni\u015flet' : 'Men\u00fcy\u00fc daralt'}
        title={collapsed ? 'Men\u00fcy\u00fc geni\u015flet' : 'Men\u00fcy\u00fc daralt'}>
        {collapsed ? <PanelLeftOpen aria-hidden="true" /> : <PanelLeftClose aria-hidden="true" />}
      </button>
      {!collapsed && <span className="sidebar-wordmark">Pano<span>POS</span></span>}
    </div>
    <nav id="main-navigation" aria-label={'Ana men\u00fc'}>
      {navigationItems.map(({ id, label, icon: Icon, to }) => {
        const contents = <><Icon aria-hidden="true" /><span className="nav-label">{label}</span>
          {!to && <span className="nav-soon">{'Yak\u0131nda'}</span>}</>;
        return to ? <NavLink key={id} end to={to} className="nav-item" aria-label={label} title={label}>{contents}</NavLink>
          : <button key={id} className="nav-item" disabled aria-label={`${label} - Yak\u0131nda`} title={`${label} - Yak\u0131nda`}>{contents}</button>;
      })}
    </nav>
    <footer className="sidebar-footer" title={`PanoPOS v${version}`}>
      {collapsed ? 'POS' : <>PanoPOS <span>v{version}</span></>}
    </footer>
  </aside>;
}
