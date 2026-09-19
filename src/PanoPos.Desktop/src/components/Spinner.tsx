import './components.css';

export function Spinner({ label = 'Yukleniyor' }: { label?: string }) {
  return <span className="loading" role="status"><span className="spinner" aria-hidden="true" />{label}</span>;
}
