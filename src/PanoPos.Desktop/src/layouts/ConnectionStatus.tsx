import { RefreshCw } from 'lucide-react';
import { useApiHealth } from '../features/connection/ApiHealth';

export function ConnectionStatus() {
  const { status, error, refresh } = useApiHealth();
  return <div className="shell-connection">
    <span role="status" className={`connection connection--${status}`} title={error || 'Local API ba\u011flant\u0131s\u0131'}>
      <span className="connection-dot" aria-hidden="true" />
      {status === 'connected' ? 'Ba\u011fl\u0131' : status === 'offline' ? 'Ba\u011flant\u0131 Yok' : 'Kontrol ediliyor'}
    </span>
    <button className="connection-retry" disabled={status === 'checking'} onClick={() => void refresh()}
      aria-label={'Ba\u011flant\u0131y\u0131 kontrol et'} title={'Ba\u011flant\u0131y\u0131 kontrol et'}><RefreshCw aria-hidden="true" /></button>
  </div>;
}
