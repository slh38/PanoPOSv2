import { describe, expect, it, vi } from 'vitest';
import { createApiClient } from '../src/api/client';
import { validateConfig } from '../src/api/config';
import { createSessionStore } from '../src/stores/session';
import { createAuthRuntime } from '../src/features/auth/runtime';
import { config, json, login, invalidPin } from './fixtures';

describe('API and session', () => {
  it('uses configured device and real login fields; never sends Bearer on login', async () => {
    const send = vi.fn(async () => json(login));
    const auth = createAuthRuntime(send, config);
    await auth.login('1234');
    expect(send.mock.calls[0]).toEqual([
      config.apiBaseUrl + '/api/v1/auth/login',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ pin: '1234', cihazId: 7 }),
        headers: { Accept: 'application/json', 'Content-Type': 'application/json' } }),
    ]);
    expect(auth.session.getSnapshot().session?.subeId).toBe(3);
    expect(auth.session.getSnapshot().session).not.toHaveProperty('oturumToken');
    expect(auth.session.getToken()).toBe(login.oturumToken);
  });
  it('sends central Bearer; logout is bodyless and clears memory', async () => {
    const send = vi.fn(async () => new Response(null, { status: 204 }));
    const auth = createAuthRuntime(send, config);
    auth.session.set(login);
    await auth.logout();
    expect(send.mock.calls[0]).toEqual([config.apiBaseUrl + '/api/v1/auth/logout',
      expect.objectContaining({ method: 'POST', body: undefined,
        headers: { Accept: 'application/json', Authorization: 'Bearer test-only-token' } })]);
    expect(auth.session.getToken()).toBeNull();
    expect(auth.session.getSnapshot().session).toBeNull();
  });
  it('clears local token when logout cannot reach backend', async () => {
    const auth = createAuthRuntime(async () => { throw Error('sensitive network detail'); }, config);
    auth.session.set(login);
    await auth.logout();
    expect(auth.session.getToken()).toBeNull();
    expect(auth.session.getSnapshot().notice).toContain('Sunucuda');
    expect(auth.session.getSnapshot().notice).not.toContain('sensitive');
  });
  it('protected 401 expires the session centrally', async () => {
    const store = createSessionStore(); store.set(login);
    const api = createApiClient(config, store, async () => json({}, 401));
    await expect(api.get('/api/v1/fatura')).rejects.toMatchObject({ status: 401 });
    expect(store.getToken()).toBeNull();
    expect(store.getSnapshot().notice).toContain('Oturum sona erdi');
  });
  it('anonymous 401 does not expire an existing session', async () => {
    const store = createSessionStore(); store.set(login);
    const api = createApiClient(config, store, async () => invalidPin());
    await expect(api.post('/api/v1/auth/login', {}, { anonymous: true })).rejects.toThrow('PIN hatal');
    expect(store.getToken()).toBe(login.oturumToken);
  });
  it('old request 401 cannot clear a newer session', async () => {
    const store = createSessionStore(); store.set(login);
    const api = createApiClient(config, store, async () => {
      store.set({ ...login, oturumToken: 'new-test-token' }); return json({}, 401);
    });
    await expect(api.get('/api/v1/fatura')).rejects.toThrow();
    expect(store.getToken()).toBe('new-test-token');
  });
  it('maps known domain errors, never raw ProblemDetails', async () => {
    const auth = createAuthRuntime(async () => invalidPin(), config);
    await expect(auth.login('9999')).rejects.toThrow('PIN hatal');
    expect(auth.session.getSnapshot().session).toBeNull();
  });
  it('unknown errors use safe fallback', async () => {
    const auth = createAuthRuntime(async () => json({ detail: 'PIN=secret token=secret', title: 'secret' }, 500), config);
    await expect(auth.login('1234')).rejects.toThrow('\u0130\u015flem tamamlanamad\u0131. Tekrar deneyin.');
  });
  it('network errors are sanitized', async () => {
    const auth = createAuthRuntime(async () => { throw Error('secret'); }, config);
    await expect(auth.login('1234')).rejects.toThrow('Ana makineye');
  });
  it('timeout aborts transport and gives friendly error', async () => {
    const auth = createAuthRuntime((_url, init) => new Promise((_resolve, reject) => {
      init.signal?.addEventListener('abort', () => reject(new Error('aborted')));
    }), { ...config, timeoutMs: 5 });
    await expect(auth.login('1234')).rejects.toThrow('zaman a\u015f\u0131m\u0131na');
  });
  it('invalid JSON response does not create a session', async () => {
    const auth = createAuthRuntime(async () => new Response('not json'), config);
    await expect(auth.login('1234')).rejects.toThrow();
    expect(auth.session.getToken()).toBeNull();
  });
  it('missing token/contract fields cannot authenticate', async () => {
    const auth = createAuthRuntime(async () => json({ adSoyad: 'Incomplete' }), config);
    await expect(auth.login('1234')).rejects.toThrow();
    expect(auth.session.getSnapshot().session).toBeNull();
  });
  it('empty PIN does not send a request', async () => {
    const send = vi.fn(async () => json(login));
    await expect(createAuthRuntime(send, config).login('')).rejects.toThrow('PIN girin');
    expect(send).not.toHaveBeenCalled();
  });
  it('blocks duplicate in-flight logins in the service', async () => {
    let finish!: (response: Response) => void;
    const send = vi.fn(() => new Promise<Response>(resolve => { finish = resolve; }));
    const auth = createAuthRuntime(send, config);
    const first = auth.login('1234');
    await auth.login('1234');
    expect(send).toHaveBeenCalledTimes(1);
    finish(json(login)); await first;
  });
  it('health uses existing public endpoint without token', async () => {
    const send = vi.fn(async () => json({ status: 'Healthy' }));
    const auth = createAuthRuntime(send, config); auth.session.set(login);
    await auth.checkHealth();
    expect(send.mock.calls[0]).toEqual([config.apiBaseUrl + '/api/v1/system/health',
      expect.objectContaining({ method: 'GET', headers: { Accept: 'application/json' } })]);
  });
  it('requires authentication before protected network calls', async () => {
    const send = vi.fn();
    const api = createApiClient(config, createSessionStore(), send);
    await expect(api.get('/api/v1/fatura')).rejects.toMatchObject({ status: 401 });
    expect(send).not.toHaveBeenCalled();
  });
  it('does not allow arbitrary URLs to receive the Bearer token', async () => {
    const store = createSessionStore(); store.set(login);
    const send = vi.fn();
    const api = createApiClient(config, store, send);
    await expect(api.get('https://other.invalid/api/v1/fatura')).rejects.toThrow();
    expect(send).not.toHaveBeenCalled();
  });
  it('config validates origin and safe positive device IDs', () => {
    expect(validateConfig({ ...config, apiBaseUrl: 'http://localhost:5296/' }).apiBaseUrl).toBe(config.apiBaseUrl);
    for (const apiBaseUrl of ['ftp://host', 'http://user:pass@host', 'http://host/path']) {
      expect(() => validateConfig({ ...config, apiBaseUrl })).toThrow();
    }
    expect(() => validateConfig({ ...config, cihazId: 0 })).toThrow();
  });
});
