import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { App } from '../src/App';
import { createAuthRuntime } from '../src/features/auth/runtime';
import { config, json, login, invalidPin } from './fixtures';
import type { HttpTransport } from '../src/api/client';

function mount(loginTransport: HttpTransport = async () => json(login)) {
  const send = vi.fn<HttpTransport>(async (url, init) => {
    if (url.endsWith('/health')) return json({ status: 'Healthy' });
    if (url.endsWith('/logout')) return new Response(null, { status: 204 });
    return loginTransport(url, init);
  });
  const auth = createAuthRuntime(send, config);
  render(<App runtime={auth} />);
  return { auth, send, user: userEvent.setup(), input: screen.getByLabelText('PIN') as HTMLInputElement };
}

describe('PIN screen', () => {
  it('focuses and masks PIN; keypad appends digits', async () => {
    const { user, input } = mount();
    expect(document.activeElement).toBe(input);
    expect(input.type).toBe('password');
    await user.click(screen.getByRole('button', { name: '1' }));
    await user.click(screen.getByRole('button', { name: '2' }));
    expect(input.value).toBe('12');
    expect(document.activeElement).toBe(input);
  });
  it('keyboard Backspace and keypad delete remove one digit', async () => {
    const { user, input } = mount();
    await user.type(input, '123');
    await user.keyboard('{Backspace}');
    expect(input.value).toBe('12');
    await user.click(screen.getByRole('button', { name: 'Son rakami sil' }));
    expect(input.value).toBe('1');
  });
  it('Escape clears PIN and non-digits are removed', async () => {
    const { user, input } = mount();
    await user.type(input, 'a1b2');
    expect(input.value).toBe('12');
    await user.keyboard('{Escape}');
    expect(input.value).toBe('');
  });
  it('Enter logs in; shows backend branch and no token', async () => {
    const { user, input, auth } = mount();
    await user.type(input, '1234{Enter}');
    await screen.findByText('Test Sube');
    expect(auth.session.getToken()).toBe(login.oturumToken);
    expect(document.body.textContent).not.toContain(login.oturumToken);
    expect(screen.queryByText('Hizli Satis')).toBeNull();
  });
  it('wrong PIN shows meaningful error, clears and refocuses', async () => {
    const { user, input } = mount(async () => invalidPin());
    await user.type(input, '9999{Enter}');
    await screen.findByText('PIN hatal\u0131.');
    expect(input.value).toBe('');
    expect(document.activeElement).toBe(input);
    expect(document.body.textContent).not.toContain('DO NOT DISPLAY');
  });
  it('empty PIN warns without posting login', async () => {
    const { send, user } = mount();
    await user.click(screen.getByRole('button', { name: 'Giris yap' }));
    await screen.findByText('PIN girin.');
    expect(send.mock.calls.filter(([url]) => url.endsWith('/login'))).toHaveLength(0);
  });
  it('duplicate form submissions send one login request', async () => {
    let finish!: (value: Response) => void;
    const { input, send } = mount(() => new Promise(resolve => { finish = resolve; }));
    fireEvent.change(input, { target: { value: '1234' } });
    const form = screen.getByRole('form', { name: 'PIN ile oturum' });
    fireEvent.submit(form); fireEvent.submit(form);
    expect(send.mock.calls.filter(([url]) => url.endsWith('/login'))).toHaveLength(1);
    expect((screen.getByRole('button', { name: 'Giris yap' }) as HTMLButtonElement).disabled).toBe(true);
    await act(async () => { finish(json(login)); });
    await screen.findByText('Test Sube');
  });
  it('logout clears session and returns focus to PIN', async () => {
    const { user, input, auth } = mount();
    await user.type(input, '1234{Enter}');
    await user.click(await screen.findByRole('button', { name: '\u00c7\u0131k\u0131\u015f Yap' }));
    const pin = await screen.findByLabelText('PIN');
    expect(document.activeElement).toBe(pin);
    expect(auth.session.getToken()).toBeNull();
    expect(localStorage.length).toBe(0);
    expect(sessionStorage.length).toBe(0);
  });
  it('central 401 returns authenticated UI to login', async () => {
    const { user, input, auth } = mount(async url => url.endsWith('/login') ? json(login) : json({}, 401));
    await user.type(input, '1234{Enter}');
    await screen.findByText('Test Sube');
    await act(async () => { await auth.api.get('/api/v1/fatura').catch(() => undefined); });
    await screen.findByLabelText('PIN');
    expect(auth.session.getToken()).toBeNull();
    expect(screen.getByRole('alert').textContent).toContain('Oturum sona erdi');
  });
  it('health failure does not block typing or pretend to be connected', async () => {
    const auth = createAuthRuntime(async () => { throw Error('raw network error'); }, config);
    render(<App runtime={auth} />);
    await screen.findByText('Sunucuya ula\u015f\u0131lam\u0131yor');
    const input = screen.getByLabelText('PIN') as HTMLInputElement;
    await userEvent.type(input, '1234');
    expect(input.value).toBe('1234');
    expect(document.body.textContent).not.toContain('raw network error');
    expect(screen.getByRole('button', { name: 'Tekrar dene' })).toBeTruthy();
  });
  it('health is checked on mount, not on every PIN change', async () => {
    const { input, user, send } = mount();
    await waitFor(() => expect(send).toHaveBeenCalledTimes(1));
    await user.type(input, '123456');
    expect(send.mock.calls.filter(([url]) => url.endsWith('/health'))).toHaveLength(1);
  });
});
