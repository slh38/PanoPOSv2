import { createRef } from 'react';
import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';
import type { ColDef, GridApi, ICellRendererParams } from 'ag-grid-community';
import type { AgGridReact } from 'ag-grid-react';
import { PanoDataGrid } from '../src/components/grid/PanoDataGrid';
import { panoGridTheme } from '../src/components/grid/panoGridTheme';
import { formatDecimal, formatMoney, formatQuantity } from '../src/components/grid/formatters';
import manifest from '../package.json';
import lock from '../package-lock.json';
import tauri from '../src-tauri/tauri.conf.json';

// JSDOM has no layout or innerText implementation. Only this test file adapts
// those browser boundaries; the real grid and its API are not mocked.
const innerTextDescriptor = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'innerText');
beforeAll(() => {
  Object.defineProperty(HTMLElement.prototype, 'innerText', {
    configurable: true,
    get() { return this.textContent ?? ''; },
    set(value: string) { this.textContent = value; },
  });
});
afterAll(() => {
  if (innerTextDescriptor) Object.defineProperty(HTMLElement.prototype, 'innerText', innerTextDescriptor);
  else Reflect.deleteProperty(HTMLElement.prototype, 'innerText');
});

interface Row { id: string; name: string; quantity: number }
const rows: Row[] = [{ id: 'one', name: 'Test Kahve', quantity: 2 }, { id: 'two', name: 'Test Su', quantity: 1.25 }];
const columns: ColDef<Row>[] = [{ field: 'name', headerName: '\u00dcr\u00fcn Ad\u0131' }, { field: 'quantity', headerName: 'Miktar' }];
const base = { columnDefs: columns, rowData: rows, suppressColumnVirtualisation: true };

describe('PanoDataGrid integration', () => {
  it('renders the real Community grid with supplied column definitions', async () => {
    render(<PanoDataGrid<Row> {...base} />);
    await screen.findByRole('grid');
    expect(await screen.findByRole('columnheader', { name: '\u00dcr\u00fcn Ad\u0131' })).toBeTruthy();
    expect(await screen.findByRole('columnheader', { name: 'Miktar' })).toBeTruthy();
  });
  it('shows supplied row data', async () => {
    render(<PanoDataGrid<Row> {...base} />);
    expect(await screen.findByText('Test Kahve')).toBeTruthy();
    expect(screen.getByText('Test Su')).toBeTruthy();
  });
  it('shows the shared Turkish empty state', async () => {
    render(<PanoDataGrid<Row> {...base} rowData={[]} />);
    expect(await screen.findByText('Kay\u0131t bulunamad\u0131.')).toBeTruthy();
  });
  it('shows loading then supplied rows when loading ends', async () => {
    const view = render(<PanoDataGrid<Row> {...base} loading />);
    expect(await screen.findByText('Y\u00fckleniyor...')).toBeTruthy();
    view.rerender(<PanoDataGrid<Row> {...base} loading={false} />);
    await waitFor(() => expect(screen.queryByText('Y\u00fckleniyor...')).toBeNull());
    expect(screen.getByText('Test Kahve')).toBeTruthy();
  });
  it('allows feature empty and loading message overrides', async () => {
    const locale = { noRowsToShow: 'Liste bos', loadingOoo: 'Liste hazirlaniyor' };
    const view = render(<PanoDataGrid<Row> {...base} rowData={[]} localeText={locale} />);
    expect(await screen.findByText('Liste bos')).toBeTruthy();
    view.rerender(<PanoDataGrid<Row> {...base} rowData={[]} localeText={locale} loading />);
    expect(await screen.findByText('Liste hazirlaniyor')).toBeTruthy();
  });
  it('allows native custom overlay components', async () => {
    render(<PanoDataGrid<Row> {...base} loading overlayComponent={() => <span>Feature overlay</span>} />);
    expect(await screen.findByText('Feature overlay')).toBeTruthy();
  });
  it('renders a feature-provided cellRenderer', async () => {
    const custom: ColDef<Row>[] = [{ field: 'name', cellRenderer: (p: ICellRendererParams<Row, string>) => <strong>Ozel: {p.value}</strong> }];
    render(<PanoDataGrid<Row> {...base} columnDefs={custom} />);
    expect(await screen.findByText('Ozel: Test Kahve')).toBeTruthy();
  });
  it('forwards feature cell events unchanged', async () => {
    const clicked = vi.fn();
    render(<PanoDataGrid<Row> {...base} onCellClicked={clicked} />);
    fireEvent.click(await screen.findByText('Test Kahve'));
    await waitFor(() => expect(clicked).toHaveBeenCalledTimes(1));
    expect(clicked.mock.calls[0]?.[0].data.id).toBe('one');
  });
  it('exposes the original grid API and serializable column state through ref', async () => {
    const ref = createRef<AgGridReact<Row>>();
    const ready = vi.fn();
    render(<PanoDataGrid<Row> {...base} ref={ref} onGridReady={ready} />);
    await waitFor(() => expect(ready).toHaveBeenCalledTimes(1));
    expect(ref.current?.api).toBe(ready.mock.calls[0]?.[0].api);
    expect(ref.current?.api.getDisplayedRowCount()).toBe(2);
    const saved = JSON.stringify(ref.current?.api.getColumnState());
    expect(JSON.parse(saved)).toHaveLength(2);
    act(() => { ref.current?.api.applyColumnState({ state: [{ colId: 'name', width: 250 }] }); });
    expect(ref.current?.api.getColumn('name')?.getActualWidth()).toBe(250);
  });
  it('isolates feature defaults, themes, class names and editing between two grids', async () => {
    let first: GridApi<Row> | undefined;
    let second: GridApi<Row> | undefined;
    const customTheme = panoGridTheme.withParams({ accentColor: '#166534' });
    render(<><section data-testid="first"><PanoDataGrid<Row> {...base} className="customer-grid"
      theme={customTheme} rowHeight={48} defaultColDef={{ sortable: true, editable: true }}
      onGridReady={e => { first = e.api; }} /></section>
      <section data-testid="second"><PanoDataGrid<Row> {...base} columnDefs={[{ field: 'name', headerName: 'Fatura' }]}
        rowHeight={32} onGridReady={e => { second = e.api; }} /></section></>);
    await waitFor(() => expect(first && second).toBeTruthy());
    expect(first?.getColumn('name')?.getColDef().sortable).toBe(true);
    expect(first?.getColumn('name')?.getColDef().editable).toBe(true);
    expect(second?.getColumn('name')?.getColDef().sortable).toBe(false);
    expect(second?.getColumn('name')?.getColDef().editable).toBe(false);
    expect(first?.getGridOption('theme')).toBe(customTheme);
    expect(second?.getGridOption('theme')).toBe(panoGridTheme);
    expect(first?.getRowNode('0')?.rowHeight).toBe(48);
    expect(second?.getRowNode('0')?.rowHeight).toBe(32);
    expect(screen.getByTestId('first').querySelector('.customer-grid')).toBeTruthy();
    expect(screen.getByTestId('second').querySelector('.customer-grid')).toBeNull();
    expect(within(screen.getByTestId('second')).getByRole('columnheader', { name: 'Fatura' })).toBeTruthy();
  });
  it('honours gridOptions defaults and top-level overrides without mutating callers', async () => {
    let api: GridApi<Row> | undefined;
    const customTheme = panoGridTheme.withParams({ headerHeight: 48 });
    const options = { defaultColDef: { sortable: true, filter: true }, theme: customTheme, rowHeight: 42 };
    render(<PanoDataGrid<Row> {...base} gridOptions={options} defaultColDef={{ sortable: false }}
      onGridReady={e => { api = e.api; }} />);
    await waitFor(() => expect(api).toBeTruthy());
    expect(api?.getColumn('name')?.getColDef().sortable).toBe(false);
    expect(api?.getColumn('name')?.getColDef().filter).toBe(true);
    expect(api?.getGridOption('theme')).toBe(customTheme);
    expect(api?.getGridOption('rowHeight')).toBe(42);
    expect(options.defaultColDef).toEqual({ sortable: true, filter: true });
  });
  it('preserves feature selection and keyboard event callbacks', async () => {
    const selected = vi.fn();
    const keyboard = vi.fn();
    render(<PanoDataGrid<Row> {...base} rowSelection={{ mode: 'singleRow', checkboxes: false, enableClickSelection: true }}
      onSelectionChanged={selected} onCellKeyDown={keyboard} />);
    const text = await screen.findByText('Test Kahve');
    fireEvent.click(text);
    await waitFor(() => expect(selected).toHaveBeenCalled());
    const cell = text.closest('[role="gridcell"]');
    if (!cell) throw Error('Cell missing');
    fireEvent.keyDown(cell, { key: 'Enter', code: 'Enter' });
    await waitFor(() => expect(keyboard).toHaveBeenCalled());
  });
  it('passes a feature cellEditor and edit callback to AG Grid', async () => {
    let api: GridApi<Row> | undefined;
    const changed = vi.fn();
    const editableColumns: ColDef<Row>[] = [{ field: 'name', editable: true, cellEditor: 'agTextCellEditor' }];
    render(<PanoDataGrid<Row> rowData={[{ id: 'edit', name: 'Before', quantity: 1 }]} columnDefs={editableColumns}
      suppressColumnVirtualisation onGridReady={e => { api = e.api; }} onCellValueChanged={changed} />);
    await screen.findByText('Before');
    act(() => { api?.startEditingCell({ rowIndex: 0, colKey: 'name' }); });
    const editor = await screen.findByRole('textbox');
    fireEvent.input(editor, { target: { value: 'After' } });
    act(() => { api?.stopEditing(); });
    await waitFor(() => expect(changed).toHaveBeenCalledTimes(1));
    expect(await screen.findByText('After')).toBeTruthy();
  });
});

describe('Display-only formatters', () => {
  it('formats money in Turkish with exactly two decimals', () => {
    expect(formatMoney(1234.56)).toBe('1.234,56');
    expect(formatMoney(0)).toBe('0,00');
    expect(formatMoney(-10.5)).toBe('-10,50');
  });
  it('formats quantities up to four decimal places without padding integers', () => {
    expect(formatQuantity(1234.5678)).toBe('1.234,5678');
    expect(formatQuantity(2)).toBe('2');
    expect(formatQuantity(0.75)).toBe('0,75');
  });
  it('formats four-decimal values without modifying the input', () => {
    const value = 1234.5678;
    expect(formatDecimal(value)).toBe('1.234,5678');
    expect(formatDecimal(2)).toBe('2,0000');
    expect(value).toBe(1234.5678);
  });
  it('does not display missing or non-finite values as zero', () => {
    for (const format of [formatMoney, formatQuantity, formatDecimal]) {
      for (const value of [null, undefined, NaN, Infinity]) expect(format(value)).toBe('');
    }
  });
});

describe('Community-only package contract', () => {
  it('pins matching Community/React versions and has no enterprise package in manifest or lock', () => {
    expect(manifest.dependencies['ag-grid-community']).toBe('36.2.0');
    expect(manifest.dependencies['ag-grid-react']).toBe('36.2.0');
    expect(Object.keys({ ...manifest.dependencies, ...manifest.devDependencies }).some(key => /ag-.*enterprise/.test(key))).toBe(false);
    expect(Object.keys(lock.packages).some(key => /ag-.*enterprise/.test(key))).toBe(false);
  });
  it('keeps production style/script CSP strict instead of allowing unsafe-inline or unsafe-eval', () => {
    expect(tauri.app.security.csp).not.toMatch(/unsafe-inline|unsafe-eval/);
  });
});
