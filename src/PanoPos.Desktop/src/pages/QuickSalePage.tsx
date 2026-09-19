import { lazy, Suspense, useState } from 'react';
import { Button } from '../components/Button';
import { Spinner } from '../components/Spinner';

const GridDemo = lazy(() => import('../dev/GridDemo'));

export function QuickSalePage() {
  const [showDemo, setShowDemo] = useState(false);
  return <section className="quick-sale-placeholder">
    <h1 tabIndex={-1}>{'H\u0131zl\u0131 Sat\u0131\u015f'}</h1>
    <p>{'Bu ekran bir sonraki g\u00f6revde olu\u015fturulacak.'}</p>
    <div className="grid-demo-toggle"><Button variant="secondary" aria-expanded={showDemo} aria-controls="grid-demo-area"
      onClick={() => setShowDemo(!showDemo)}>{showDemo ? 'Grid demosunu kapat' : 'Grid demosunu a\u00e7'}</Button></div>
    {showDemo && <div id="grid-demo-area"><Suspense fallback={<Spinner label={'Grid y\u00fckleniyor'} />}><GridDemo /></Suspense></div>}
  </section>;
}
