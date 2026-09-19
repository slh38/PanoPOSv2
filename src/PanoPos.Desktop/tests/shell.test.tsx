import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter, useLocation, useNavigate } from 'react-router';
import { AppRoutes } from '../src/App';
import { createAuthRuntime } from '../src/features/auth/runtime';
import type { HttpTransport } from '../src/api/client';
import { config, json, login } from './fixtures';

function RouteProbe() {
  const location = useLocation();
  const navigate = useNavigate();
  return <><output data-testid="location">{location.pathname}</output>
    <button onClick={() => navigate(-1)}>Test back</button></>;
}

function mount({ authenticated = true, path = '/', healthFails = false, logoutFails = false } = {}) {
  const send = vi.fn<HttpTransport>(async url => {
    if (url.endsWith('/health')) {
      if (healthFails) throw Error('private connection detail');
      return json({ status: 'Healthy' });
    }
    if (url.endsWith('/logout')) {
      if (logoutFails) throw Error('private logout detail');
      return new Response(null, { status: 204 });
    }
    return json(login);
  });
  const auth = createAuthRuntime(send, config);
  if (authenticated) auth.session.set(login);
  render(<MemoryRouter initialEntries={[path]}><AppRoutes runtime={auth} /><RouteProbe /></MemoryRouter>);
  return { auth, send, user: userEvent.setup() };
}

const quickSale = 'H\u0131zl\u0131 Sat\u0131\u015f';
const userMenu = `Kullan\u0131c\u0131 men\u00fcs\u00fc: ${login.adSoyad}`;
const nav = () => within(screen.getByRole('navigation', { name: 'Ana men\u00fc' }));
afterEach(() => { vi.useRealTimers(); });

describe('Application shell', () => {
  it('redirects missing session to login without exposing shell', async () => {
    mount({ authenticated: false });
    await screen.findByLabelText('PIN');
    expect(screen.getByTestId('location').textContent).toBe('/login');
    expect(screen.queryByRole('navigation')).toBeNull();
  });
  it('opens dashboard with actual session name, branch and device', async () => {
    mount();
    expect(screen.getByRole('heading', { name: `Merhaba, ${login.adSoyad}` })).toBeTruthy();
    expect(screen.getByText('Test Sube')).toBeTruthy();
    expect(screen.getByText(`Cihaz ${login.cihazId}`)).toBeTruthy();
    expect(document.body.textContent).not.toContain(login.oturumToken);
    await screen.findByText('Ba\u011fl\u0131');
  });
  it('redirects authenticated login route to dashboard', async () => {
    mount({ path: '/login' });
    await screen.findByRole('heading', { name: `Merhaba, ${login.adSoyad}` });
    expect(screen.getByTestId('location').textContent).toBe('/');
  });
  it('marks home active and quick sale inactive on dashboard', () => {
    mount();
    expect(nav().getByRole('link', { name: 'Ana Sayfa' }).getAttribute('aria-current')).toBe('page');
    expect(nav().getByRole('link', { name: quickSale }).getAttribute('aria-current')).toBeNull();
  });
  it('collapses and expands navigation with accessible icon labels', async () => {
    const { user } = mount();
    await user.click(screen.getByRole('button', { name: 'Men\u00fcy\u00fc daralt' }));
    expect(screen.getByRole('button', { name: 'Men\u00fcy\u00fc geni\u015flet' }).getAttribute('aria-expanded')).toBe('false');
    expect(document.querySelector('.application-shell--collapsed')).toBeTruthy();
    expect(nav().getByRole('link', { name: quickSale }).getAttribute('title')).toBe(quickSale);
    await user.click(screen.getByRole('button', { name: 'Men\u00fcy\u00fc geni\u015flet' }));
    expect(document.querySelector('.application-shell--collapsed')).toBeNull();
  });
  it('sidebar opens the quick sale placeholder and keeps shell', async () => {
    const { user } = mount();
    await user.click(nav().getByRole('link', { name: quickSale }));
    expect(screen.getByTestId('location').textContent).toBe('/hizli-satis');
    expect(screen.getByRole('heading', { name: quickSale })).toBeTruthy();
    expect(screen.getByText('Bu ekran bir sonraki g\u00f6revde olu\u015fturulacak.')).toBeTruthy();
    expect(nav().getByRole('link', { name: quickSale }).getAttribute('aria-current')).toBe('page');
    expect(screen.queryByRole('grid')).toBeNull();
  });
  it('dashboard quick sale card uses the same route as navigation', async () => {
    const { user } = mount();
    const links = screen.getAllByRole('link', { name: quickSale });
    expect(links).toHaveLength(2);
    const [navigationLink, cardLink] = links;
    if (!navigationLink || !cardLink) throw new Error('Quick sale links are missing');
    expect(navigationLink.getAttribute('href')).toBe(cardLink.getAttribute('href'));
    await user.click(cardLink);
    expect(screen.getByTestId('location').textContent).toBe('/hizli-satis');
  });
  it('undeveloped modules are disabled in both navigation and dashboard', async () => {
    const { user } = mount();
    const buttons = screen.getAllByRole('button', { name: /Yak\u0131nda$/ });
    expect(buttons).toHaveLength(14);
    for (const button of buttons) {
      expect((button as HTMLButtonElement).disabled).toBe(true);
      await user.click(button);
    }
    expect(screen.getByTestId('location').textContent).toBe('/');
  });
  it('protects direct quick sale navigation without session', async () => {
    mount({ authenticated: false, path: '/hizli-satis' });
    await screen.findByLabelText('PIN');
    expect(screen.getByTestId('location').textContent).toBe('/login');
    expect(screen.queryByRole('heading', { name: quickSale })).toBeNull();
  });
  it('returns home and preserves collapsed state across routes', async () => {
    const { user } = mount();
    await user.click(screen.getByRole('button', { name: 'Men\u00fcy\u00fc daralt' }));
    await user.click(nav().getByRole('link', { name: quickSale }));
    await user.click(nav().getByRole('link', { name: 'Ana Sayfa' }));
    expect(screen.getByTestId('location').textContent).toBe('/');
    expect(document.querySelector('.application-shell--collapsed')).toBeTruthy();
  });
  it('logout returns to PIN and history cannot reopen protected content', async () => {
    const { user, auth, send } = mount();
    await user.click(nav().getByRole('link', { name: quickSale }));
    await user.click(screen.getByRole('button', { name: userMenu }));
    await user.click(screen.getByRole('button', { name: '\u00c7\u0131k\u0131\u015f Yap' }));
    await screen.findByLabelText('PIN');
    expect(auth.session.getToken()).toBeNull();
    expect(send.mock.calls.filter(([url]) => url.endsWith('/logout'))).toHaveLength(1);
    await user.click(screen.getByRole('button', { name: 'Test back' }));
    await waitFor(() => expect(screen.getByTestId('location').textContent).toBe('/login'));
    expect(screen.queryByRole('navigation')).toBeNull();
  });
  it('logout still clears local session if API is unreachable', async () => {
    const { user, auth } = mount({ logoutFails: true });
    await user.click(screen.getByRole('button', { name: userMenu }));
    await user.click(screen.getByRole('button', { name: '\u00c7\u0131k\u0131\u015f Yap' }));
    await screen.findByLabelText('PIN');
    expect(auth.session.getToken()).toBeNull();
    expect(screen.getByRole('alert').textContent).toContain('Sunucuda');
  });
  it('health failure keeps shell and navigation usable without raw errors', async () => {
    const { user, auth } = mount({ healthFails: true });
    await screen.findByText('Ba\u011flant\u0131 Yok');
    expect(auth.session.getToken()).toBe(login.oturumToken);
    expect(document.body.textContent).not.toContain('private connection detail');
    await user.click(nav().getByRole('link', { name: quickSale }));
    expect(screen.getByRole('heading', { name: quickSale })).toBeTruthy();
  });
  it('shares health across routes and rechecks explicitly', async () => {
    const { user, send } = mount();
    await screen.findByText('Ba\u011fl\u0131');
    await user.click(nav().getByRole('link', { name: quickSale }));
    await user.click(nav().getByRole('link', { name: 'Ana Sayfa' }));
    expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(1);
    await user.click(screen.getByRole('button', { name: 'Ba\u011flant\u0131y\u0131 kontrol et' }));
    await waitFor(() => expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(2));
  });
  it('central 401 on quick sale returns to login', async () => {
    const { auth } = mount({ path: '/hizli-satis' });
    await screen.findByText('Ba\u011fl\u0131');
    await act(async () => { auth.session.expire(login.oturumToken); });
    await screen.findByLabelText('PIN');
    expect(screen.getByTestId('location').textContent).toBe('/login');
  });
  it('user disclosure supports keyboard activation and Escape focus return', async () => {
    const { user } = mount();
    const trigger = screen.getByRole('button', { name: userMenu });
    trigger.focus();
    await user.keyboard('{Enter}');
    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    await user.tab();
    expect(document.activeElement).toBe(screen.getByRole('button', { name: '\u00c7\u0131k\u0131\u015f Yap' }));
    await user.keyboard('{Escape}');
    expect(trigger.getAttribute('aria-expanded')).toBe('false');
    expect(document.activeElement).toBe(trigger);
  });
  it('network offline event leaves shell open and online rechecks API', async () => {
    const { send } = mount();
    await screen.findByText('Ba\u011fl\u0131');
    fireEvent(window, new Event('offline'));
    expect(screen.getByText('Ba\u011flant\u0131 Yok')).toBeTruthy();
    expect(screen.getByRole('navigation')).toBeTruthy();
    fireEvent(window, new Event('online'));
    await screen.findByText('Ba\u011fl\u0131');
    expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(2);
  });
  it('polls health only once per minute while visible', async () => {
    vi.useFakeTimers();
    const { send } = mount();
    await act(async () => { await vi.advanceTimersByTimeAsync(59_000); });
    expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(1);
    await act(async () => { await vi.advanceTimersByTimeAsync(1_000); });
    expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(2);
  });
});
