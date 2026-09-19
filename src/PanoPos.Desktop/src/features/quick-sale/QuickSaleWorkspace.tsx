import { lazy, Suspense, useRef, useState } from 'react';
import { Trash2, UserRoundPlus, UserRoundSearch } from 'lucide-react';
import { demoCartRows } from './demoQuickSaleData';
import { QuickSaleActions } from './components/QuickSaleActions';
import { Spinner } from '../../components/Spinner';
import { QuickSaleNumpad } from './components/QuickSaleNumpad';
import { QuickSaleProducts } from './components/QuickSaleProducts';
import { QuickSaleCheckout } from './components/QuickSaleCheckout';
import './quickSale.css';

const QuickSaleGrid = lazy(() => import('./components/QuickSaleGrid').then(module => ({ default: module.QuickSaleGrid })));

export function QuickSaleWorkspace() {
  const [multiplier, setMultiplier] = useState('');
  const [barcode, setBarcode] = useState('');
  const [feedback, setFeedback] = useState('Yerel örnek veriler. Gerçek satış veya ödeme yapılmaz.');
  const barcodeRef = useRef<HTMLInputElement>(null);
  function previewAction(label: string) { setFeedback(`${label}: demo önizleme, işlem henüz bağlı değil.`); }

  return <section className="quick-sale" aria-label="Hızlı Satış önizlemesi">
    <header className="qs-heading"><h1 tabIndex={-1}>Hızlı Satış</h1><span className="qs-demo-badge">DEMO · API satışı kapalı</span>
      <p role="status" aria-label="Demo işlem durumu" title={feedback}>{feedback}</p></header>
    <div className="qs-layout">
      <QuickSaleActions onAction={previewAction} />
      <section className="qs-cart" aria-label="Sepet">
        <div className="qs-cart-toolbar">
          <select aria-label="Fiyat tipi (demo)" defaultValue="retail"><option value="retail">Perakende Satış</option></select>
          <button type="button" className="qs-icon-button" aria-label="Müşteri seç" title="Müşteri seç" onClick={() => previewAction('Müşteri seç')}><UserRoundSearch aria-hidden="true" /></button>
          <button type="button" className="qs-icon-button" aria-label="Müşteri ekle" title="Müşteri ekle" onClick={() => previewAction('Müşteri ekle')}><UserRoundPlus aria-hidden="true" /></button>
          <button type="button" className="qs-icon-button qs-icon-button--danger" aria-label="Sepeti temizle" title="Sepeti temizle" onClick={() => previewAction('Sepeti temizle')}><Trash2 aria-hidden="true" /></button>
        </div>
        <div className="qs-grid-area"><Suspense fallback={<Spinner label="Sepet hazırlanıyor" />}>
          <QuickSaleGrid rows={demoCartRows} onInspect={setFeedback} />
        </Suspense></div>
        <QuickSaleCheckout barcodeRef={barcodeRef} barcode={barcode} onBarcode={setBarcode} onAction={previewAction} />
      </section>
      <QuickSaleNumpad value={multiplier} onChange={setMultiplier} />
      <QuickSaleProducts onSelect={p => setFeedback(`${p.name} · ${multiplier || '1'} × demo seçim. Sepete eklenmedi.`)} />
    </div>
  </section>;
}
