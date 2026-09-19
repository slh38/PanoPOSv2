import type { DesktopConfig } from './config';
import { ApiError, fallbackMessage, networkMessage, problemError } from './errors';
import type { SessionStore } from '../stores/session';

export type HttpTransport = (url: string, init: RequestInit) => Promise<Response>;
interface RequestOptions { anonymous?: boolean; timeoutMs?: number }

export function createApiClient(config: DesktopConfig, session: SessionStore, transport: HttpTransport) {
  async function request<T>(method: 'GET' | 'POST', path: string, body: unknown, options: RequestOptions = {}): Promise<T> {
    if (!/^\/api\/v1\/[a-z0-9/-]+$/i.test(path)) throw new ApiError(fallbackMessage);
    const token = options.anonymous ? null : session.getToken();
    if (!options.anonymous && !token) throw new ApiError('Oturum gerekli. Yeniden giri\u015f yap\u0131n.', 401);
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), options.timeoutMs ?? config.timeoutMs);
    const headers: Record<string, string> = { Accept: 'application/json' };
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    if (token) headers.Authorization = `Bearer ${token}`;
    try {
      const response = await transport(config.apiBaseUrl + path, {
        method, headers, body: body === undefined ? undefined : JSON.stringify(body),
        signal: controller.signal, credentials: 'omit', redirect: 'error', cache: 'no-store',
      });
      if (response.status === 401 && token) session.expire(token);
      if (!response.ok) {
        const problem: unknown = await response.json().catch(() => null);
        throw problemError(response.status, problem);
      }
      if (response.status === 204) return undefined as T;
      try { return await response.json() as T; }
      catch { throw new ApiError(fallbackMessage); }
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw new ApiError(controller.signal.aborted
        ? 'Sunucu yan\u0131t\u0131 zaman a\u015f\u0131m\u0131na u\u011frad\u0131. Tekrar deneyin.' : networkMessage);
    } finally { clearTimeout(timer); }
  }
  return {
    get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, undefined, options),
    post: <T>(path: string, body?: unknown, options?: RequestOptions) => request<T>('POST', path, body, options),
  };
}
