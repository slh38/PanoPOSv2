import { Banknote, CreditCard, Split, Barcode, Percent } from 'lucide-react';
import type { Ref } from 'react';
import { formatMoney } from '../../../components/grid/formatters';
import { demoTotals } from '../demoQuickSaleData';

export function QuickSaleCheckout({ barcodeRef, barcode, onBarcode, onAction }: {
  barcodeRef: Ref<HTMLInputElement>; barcode: string; onBarcode: (value: string) => void; onAction: (label: string) => void;
}) {
  return <>
    <form className="qs-barcode" onSubmit={e => { e.preventDefault(); onAction('Barkod arama'); }}>
      <Barcode aria-hidden="true" /><input ref={barcodeRef} aria-label="Barkod veya ürün adı" autoComplete="off" spellCheck={false}
        placeholder="Barkod okutun veya ürün adı yazın..." value={barcode} onChange={e => onBarcode(e.target.value)} />
    </form>
    <section className="qs-totals" aria-label="Demo toplamlar">
      <div><span>Ara Toplam</span><span>{formatMoney(demoTotals.subtotal)}</span></div>
      <div><span>İskonto</span><button type="button" className="qs-discount" aria-label="Genel iskonto" onClick={() => onAction('Genel iskonto')}><Percent aria-hidden="true" /></button><span>{formatMoney(demoTotals.discount)}</span></div>
      <div className="qs-grand-total"><strong>Genel Toplam</strong><strong>{formatMoney(demoTotals.total)} <small>TL</small></strong></div>
    </section>
    <div className="qs-payments" role="group" aria-label="Ödeme seçenekleri">
      <button type="button" className="qs-payment qs-payment--cash" onClick={() => onAction('Nakit')}><Banknote aria-hidden="true" /><strong>Nakit</strong><small>F10</small></button>
      <button type="button" className="qs-payment qs-payment--card" onClick={() => onAction('Kredi Kartı')}><CreditCard aria-hidden="true" /><strong>Kredi Kartı</strong><small>F11</small></button>
      <button type="button" className="qs-payment qs-payment--split" onClick={() => onAction('Parçalı')}><Split aria-hidden="true" /><strong>Parçalı</strong><small>F12</small></button>
    </div>
  </>;
}
