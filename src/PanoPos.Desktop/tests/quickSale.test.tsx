import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router';
import { QuickSalePage } from '../src/pages/QuickSalePage';
import { QuickSaleGrid } from '../src/features/quick-sale/components/QuickSaleGrid';
import { nextDemoMultiplier } from '../src/features/quick-sale/components/QuickSaleNumpad';
import { demoCartRows } from '../src/features/quick-sale/demoQuickSaleData';
import { AppRoutes } from '../src/App';
import { createAuthRuntime } from '../src/features/auth/runtime';
import type { HttpTransport } from '../src/api/client';
import { config, json, login } from './fixtures';

const mount = () => render(<QuickSalePage />);
const pad = () => within(screen.getByRole('region', { name: 'Sayısal tuş takımı' }));
const catalog = () => within(screen.getByRole('region', { name: 'Demo ürünler' }));

describe('Quick sale visual preview', () => {
  it('renders the screen with an explicit no-sale demo notice', () => {
    mount();
    expect(screen.getByRole('heading', { name: 'Hızlı Satış' })).toBeTruthy();
    expect(screen.getByText('DEMO · API satışı kapalı')).toBeTruthy();
  });
  it('keeps all eight operation buttons in a separate mandatory column', () => {
    mount();
    const actions = within(screen.getByRole('complementary', { name: 'Satış işlemleri' }));
    for (const name of ['Borç', 'Beklet', 'Bekleyen Satışlar', 'Müşteri Seç', 'Fiyat Gör', 'İskonto', 'Satış İptal', 'Diğer İşlemler'])
      expect(actions.getByRole('button', { name })).toBeTruthy();
    expect(actions.getAllByRole('button')).toHaveLength(8);
  });
  it('uses the real shared grid and shows all six columns', async () => {
    render(<QuickSaleGrid rows={demoCartRows} onInspect={vi.fn()} />);
    expect(await screen.findByRole('grid', { name: 'Demo sepet' })).toBeTruthy();
    for (const name of ['Ürün Adı', 'Miktar', 'Birim', 'Birim Fiyat', 'İndirim', 'Tutar'])
      expect(screen.getByRole('columnheader', { name })).toBeTruthy();
  });
  it('shows local cart rows with formatted quantity and money', async () => {
    render(<QuickSaleGrid rows={demoCartRows} onInspect={vi.fn()} />);
    expect(await screen.findByText('Coca Cola 1.5 L')).toBeTruthy();
    expect(screen.getByText('Lays Klasik 100 g')).toBeTruthy();
    expect(screen.getByText('Eti Çikolata 60 g')).toBeTruthy();
    const cola = screen.getByText('Coca Cola 1.5 L').closest('[role="row"]');
    expect(cola?.textContent).toContain('60,00');
    expect(cola?.textContent).toContain('120,00');
    expect(cola?.textContent).toContain('Adet');
    expect(cola?.querySelector('.qs-quantity')?.textContent).toBe('2');
  });
  it('preserves numeric alignment when the total cell has custom emphasis', async () => {
    render(<QuickSaleGrid rows={demoCartRows} onInspect={vi.fn()} />);
    const row = (await screen.findByText('Coca Cola 1.5 L')).closest('[role="row"]');
    for (const field of ['quantity', 'price', 'discount', 'total'])
      expect(row?.querySelector(`[col-id="${field}"]`)?.classList.contains('ag-right-aligned-cell')).toBe(true);
  });
  it('keeps quantity and unit read-only but supports feature interaction', async () => {
    const inspect = vi.fn();
    render(<QuickSaleGrid rows={demoCartRows} onInspect={inspect} />);
    fireEvent.click((await screen.findAllByText('Adet'))[0]!);
    await waitFor(() => expect(inspect).toHaveBeenCalledWith('Miktar ve birim düzenleme sonraki adımda bağlanacak.'));
    expect(screen.queryByRole('textbox')).toBeNull();
  });
  it('provides barcode input and Enter feedback without a lookup', () => {
    mount();
    const input = screen.getByRole('textbox', { name: 'Barkod veya ürün adı' });
    expect(input.getAttribute('placeholder')).toBe('Barkod okutun veya ürün adı yazın...');
    fireEvent.change(input, { target: { value: '869123' } });
    fireEvent.submit(input.closest('form')!);
    expect(screen.getByRole('status', { name: 'Demo işlem durumu' }).textContent).toContain('Barkod arama: demo');
  });
  it('shows the fixed demo totals without computing business values', () => {
    mount();
    const totals = within(screen.getByRole('region', { name: 'Demo toplamlar' }));
    for (const label of ['Ara Toplam', 'İskonto', 'Genel Toplam']) expect(totals.getByText(label)).toBeTruthy();
    expect(totals.getAllByText(/285,00/)).toHaveLength(2);
  });
  it('shows cash, card and split buttons with shortcut hints', () => {
    mount();
    const payments = within(screen.getByRole('group', { name: 'Ödeme seçenekleri' }));
    for (const name of ['Nakit F10', 'Kredi Kartı F11', 'Parçalı F12']) expect(payments.getByRole('button', { name })).toBeTruthy();
  });
  it('payment and cancel feedback never clear the demo cart', async () => {
    mount();
    fireEvent.click(screen.getByRole('button', { name: 'Nakit F10' }));
    expect(screen.getByRole('status', { name: 'Demo işlem durumu' }).textContent).toContain('işlem henüz bağlı değil');
    fireEvent.click(screen.getByRole('button', { name: 'Satış İptal' }));
    expect(await screen.findByText('Coca Cola 1.5 L')).toBeTruthy();
  });
  it('renders numeric buttons in the specified vertical order', () => {
    mount();
    expect(pad().getAllByRole('button').map(b => b.textContent)).toEqual(['1','2','3','4','5','6','7','8','9','0',',','Sil']);
  });
  it('updates numeric demo state and supports decimal and backspace', () => {
    mount();
    for (const name of ['2', ',', '5']) fireEvent.click(pad().getByRole('button', { name }));
    expect(screen.getByLabelText('Demo çarpan').textContent).toBe('2,5 ×');
    fireEvent.click(pad().getByRole('button', { name: 'Sil' }));
    expect(screen.getByLabelText('Demo çarpan').textContent).toBe('2, ×');
  });
  it('limits numeric input and prevents duplicate separators', () => {
    expect(nextDemoMultiplier('', ',')).toBe('0,');
    expect(nextDemoMultiplier('1,2', ',')).toBe('1,2');
    expect(nextDemoMultiplier('12345678', '9')).toBe('12345678');
  });
  it('does not retain numeric state after leaving the feature', () => {
    const view = mount();
    fireEvent.click(pad().getByRole('button', { name: '7' }));
    view.unmount(); mount();
    expect(screen.getByLabelText('Demo çarpan').textContent).toBe('1 ×');
  });
  it('shows all category controls and filters locally', () => {
    mount();
    const categories = within(screen.getByRole('group', { name: 'Kategoriler' }));
    expect(categories.getAllByRole('button')).toHaveLength(7);
    fireEvent.click(categories.getByRole('button', { name: 'Fırın' }));
    expect(catalog().getByRole('button', { name: /Simit/ })).toBeTruthy();
    expect(catalog().queryByRole('button', { name: /Coca Cola/ })).toBeNull();
  });
  it('searches Turkish demo names and handles empty results', () => {
    mount();
    fireEvent.change(screen.getByRole('textbox', { name: 'Ürün ara' }), { target: { value: 'SIVI' } });
    expect(catalog().getByRole('button', { name: /Sıvı Sabun/ })).toBeTruthy();
    fireEvent.change(screen.getByRole('textbox', { name: 'Ürün ara' }), { target: { value: 'unknown' } });
    expect(screen.getByText('Demo ürün bulunamadı.')).toBeTruthy();
  });
  it('shows compact product artwork, packaging, price and a missing-image fallback', () => {
    mount();
    const card = catalog().getByRole('button', { name: /Coca Cola/ });
    expect(card.textContent).toContain('1.5 L');
    expect(card.textContent).toContain('60,00 TL');
    expect(card.querySelector('svg')).toBeTruthy();
    expect(catalog().getByRole('button', { name: /Kağıt Havlu/ }).querySelector('svg')).toBeTruthy();
  });
  it('product click reports the multiplier but does not change cart rows', async () => {
    mount();
    fireEvent.click(pad().getByRole('button', { name: '4' }));
    fireEvent.click(catalog().getByRole('button', { name: /Fanta/ }));
    expect(screen.getByRole('status', { name: 'Demo işlem durumu' }).textContent).toContain('Fanta · 4 × demo seçim. Sepete eklenmedi.');
    const grid = await screen.findByRole('grid', { name: 'Demo sepet' });
    expect(within(grid).queryByText('Fanta')).toBeNull();
  });
  it('keeps product scrolling independent from categories and search', () => {
    mount();
    const region = screen.getByRole('region', { name: 'Ürün kartları' });
    expect(region.classList.contains('qs-product-scroll')).toBe(true);
    expect(region.contains(screen.getByRole('group', { name: 'Kategoriler' }))).toBe(false);
    expect(region.contains(screen.getByRole('textbox', { name: 'Ürün ara' }))).toBe(false);
    expect(region.tabIndex).toBe(0);
  });
  it('removes the obsolete grid demo toggle', () => {
    mount();
    expect(screen.queryByRole('button', { name: /Grid demosunu/ })).toBeNull();
    expect(screen.queryByRole('region', { name: 'Grid demosu' })).toBeNull();
  });
  it('preserves navigation collapse and makes only existing health calls', async () => {
    const send = vi.fn<HttpTransport>(async () => json({ status: 'Healthy' }));
    const auth = createAuthRuntime(send, config); auth.session.set(login);
    render(<MemoryRouter initialEntries={['/hizli-satis']}><AppRoutes runtime={auth} /></MemoryRouter>);
    await screen.findByText('Bağlı');
    fireEvent.click(screen.getByRole('button', { name: 'Menüyü daralt' }));
    expect(document.querySelector('.application-shell--collapsed')).toBeTruthy();
    expect(screen.getByRole('complementary', { name: 'Satış işlemleri' })).toBeTruthy();
    fireEvent.click(screen.getByRole('button', { name: 'Beklet' }));
    fireEvent.click(screen.getByRole('button', { name: 'Parçalı F12' }));
    fireEvent.click(catalog().getByRole('button', { name: /Fanta/ }));
    expect(send.mock.calls.every(([url]) => url.endsWith('/health'))).toBe(true);
  });
});
