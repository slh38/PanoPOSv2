import { Link } from 'react-router';
import { ArrowUpRight } from 'lucide-react';
import { navigationItems } from '../layouts/navigation';
import { LocalClock } from '../components/LocalClock';

export function DashboardPage({ name }: { name: string }) {
  return <section className="dashboard" aria-label="Ana Sayfa">
    <div className="dashboard-heading">
      <div><p className="page-eyebrow">ANA SAYFA</p><h1 tabIndex={-1}>Merhaba, {name}</h1>
        <p>{'\u0130\u015flemlerinize ba\u015flamak i\u00e7in bir mod\u00fcl se\u00e7in.'}</p></div>
      <LocalClock />
    </div>
    <div className="module-grid" aria-label={'Mod\u00fcller'}>
      {navigationItems.filter(item => item.id !== 'home').map(({ id, label, icon: Icon, to, tone }) => {
        const contents = <><span className={`module-icon module-icon--${tone}`}><Icon aria-hidden="true" /></span>
          <strong>{label}</strong><span className="module-status">{to ? <>{'A\u00e7'} <ArrowUpRight aria-hidden="true" /></> : 'Yak\u0131nda'}</span></>;
        return to ? <Link key={id} to={to} aria-label={label} className="module-card module-card--active">{contents}</Link>
          : <button key={id} disabled className="module-card" aria-label={`${label} - Yak\u0131nda`}>{contents}</button>;
      })}
    </div>
  </section>;
}
