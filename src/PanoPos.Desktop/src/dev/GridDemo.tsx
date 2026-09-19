import { useState } from 'react';
import type { ColDef, GridReadyEvent } from 'ag-grid-community';
import { PanoDataGrid } from '../components/grid/PanoDataGrid';
import { formatMoney, formatQuantity } from '../components/grid/formatters';
import { Button } from '../components/Button';
import './gridDemo.css';

interface DemoRow { id: string; ad: string; miktar: number; birim: string; fiyat: number; tutar: number }
const rows: DemoRow[] = [
  { id: 'demo-1', ad: 'Demo - Filtre Kahve', miktar: 2, birim: 'Adet', fiyat: 85, tutar: 170 },
  { id: 'demo-2', ad: 'Demo - Su 500 ml', miktar: 1, birim: 'Koli', fiyat: 240, tutar: 240 },
  { id: 'demo-3', ad: 'Demo - Taze Kahve', miktar: 0.75, birim: 'Kg', fiyat: 1234.56, tutar: 925.92 },
  { id: 'demo-4', ad: 'Demo - Sandvi\u00e7', miktar: 3, birim: 'Adet', fiyat: 125, tutar: 375 },
];
const emptyRows: DemoRow[] = [];
const columns: ColDef<DemoRow>[] = [
  { field: 'ad', headerName: '\u00dcr\u00fcn Ad\u0131', minWidth: 180, flex: 1 },
  { field: 'miktar', headerName: 'Miktar', width: 90, type: 'numericColumn', valueFormatter: p => formatQuantity(p.value) },
  { field: 'birim', headerName: 'Birim', width: 80 },
  { field: 'fiyat', headerName: 'Birim Fiyat', width: 120, type: 'numericColumn', valueFormatter: p => formatMoney(p.value) },
  { field: 'tutar', headerName: 'Tutar', width: 120, type: 'numericColumn', valueFormatter: p => formatMoney(p.value) },
];
const defaults = { sortable: true };
const selection = { mode: 'singleRow' as const, checkboxes: false, enableClickSelection: true };
function labelGrid(event: GridReadyEvent<DemoRow>) { event.api.setGridAriaProperty('label', 'Demo veri listesi'); }

export default function GridDemo() {
  const [mode, setMode] = useState<'data' | 'empty' | 'loading'>('data');
  const [lastClick, setLastClick] = useState('');
  return <section className="grid-demo" aria-label="Grid demosu">
    <header className="grid-demo-heading"><h2>Grid demosu</h2>
      <p>{'Statik test verisidir. Ger\u00e7ek sat\u0131\u015f de\u011fildir; API iste\u011fi veya hesaplama yapmaz.'}</p></header>
    <div className="grid-demo-toolbar" aria-label={'Demo g\u00f6r\u00fcn\u00fcm\u00fc'}>
      <Button variant="secondary" aria-pressed={mode === 'data'} onClick={() => setMode('data')}>{'\u00d6rnek sat\u0131rlar'}</Button>
      <Button variant="secondary" aria-pressed={mode === 'empty'} onClick={() => setMode('empty')}>{'Bo\u015f liste'}</Button>
      <Button variant="secondary" aria-pressed={mode === 'loading'} onClick={() => setMode('loading')}>{'Y\u00fckleniyor g\u00f6r\u00fcn\u00fcm\u00fc'}</Button>
    </div>
    <PanoDataGrid<DemoRow> className="demo-product-grid" rowData={mode === 'data' ? rows : emptyRows}
      columnDefs={columns} defaultColDef={defaults} loading={mode === 'loading'}
      rowSelection={selection} getRowId={p => p.data.id} onGridReady={labelGrid}
      onCellClicked={event => setLastClick(event.data?.ad ?? '')} />
    <p className="grid-demo-status" role="status">{lastClick ? `Son t\u0131klanan: ${lastClick}` : 'Kolonlar\u0131 yeniden boyutland\u0131rabilir, s\u0131ralayabilir ve sat\u0131r se\u00e7ebilirsiniz.'}</p>
  </section>;
}
