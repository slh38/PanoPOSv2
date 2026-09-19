import type { ColDef, ICellRendererParams } from 'ag-grid-community';
import { PanoDataGrid } from '../../../components/grid/PanoDataGrid';
import { formatMoney, formatQuantity } from '../../../components/grid/formatters';
import type { DemoCartRow } from '../demoQuickSaleData';

function QuantityCell({ value }: ICellRendererParams<DemoCartRow, number>) {
  return <span className="qs-quantity">{formatQuantity(value)}</span>;
}
function UnitCell({ value }: ICellRendererParams<DemoCartRow, string>) {
  return <span className="qs-unit">{value}</span>;
}

const columns: ColDef<DemoCartRow>[] = [
  { field: 'name', headerName: 'Ürün Adı', minWidth: 150, flex: 1, tooltipField: 'name' },
  { field: 'quantity', headerName: 'Miktar', width: 60, type: 'numericColumn', cellRenderer: QuantityCell },
  { field: 'unit', headerName: 'Birim', width: 52, cellRenderer: UnitCell },
  { field: 'price', headerName: 'Birim Fiyat', width: 80, wrapHeaderText: true, type: 'numericColumn', valueFormatter: p => formatMoney(p.value) },
  { field: 'discount', headerName: 'İndirim', width: 64, type: 'numericColumn', valueFormatter: p => formatMoney(p.value) },
  { field: 'total', headerName: 'Tutar', width: 80, type: 'numericColumn', valueFormatter: p => formatMoney(p.value), cellClass: ['ag-right-aligned-cell', 'qs-line-total'] },
];
const selection = { mode: 'singleRow' as const, checkboxes: false, enableClickSelection: true };

export function QuickSaleGrid({ rows, onInspect }: { rows: DemoCartRow[]; onInspect: (message: string) => void }) {
  return <PanoDataGrid<DemoCartRow> className="qs-cart-grid" rowData={rows} columnDefs={columns}
    containerStyle={{ height: '100%' }} rowHeight={42} headerHeight={38}
    rowSelection={selection} getRowId={p => p.data.id}
    onGridReady={e => e.api.setGridAriaProperty('label', 'Demo sepet')}
    onCellClicked={e => {
      if (e.colDef.field === 'quantity' || e.colDef.field === 'unit')
        onInspect('Miktar ve birim düzenleme sonraki adımda bağlanacak.');
    }}
    onCellKeyDown={e => {
      if ((e.event as KeyboardEvent | undefined)?.key === 'Enter')
        onInspect('Demo satır seçildi. Düzenleme henüz bağlı değil.');
    }} />;
}
