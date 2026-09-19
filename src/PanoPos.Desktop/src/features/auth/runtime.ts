import { createApiClient, type HttpTransport } from '../../api/client';
import { desktopConfig, type DesktopConfig } from '../../api/config';
import { ApiError, fallbackMessage } from '../../api/errors';
import { createSessionStore } from '../../stores/session';
import { isLoginResponse, type LoginRequest } from './contracts';

export function createAuthRuntime(transport: HttpTransport, config: DesktopConfig = desktopConfig) {
  const session = createSessionStore();
  const api = createApiClient(config, session, transport);
  let loginPending = false;
  let logoutPending = false;
  return {
    config, session, api,
    async login(pin: string) {
      if (loginPending || logoutPending) return;
      if (!/^\d+$/.test(pin)) throw new ApiError('PIN girin. Yaln\u0131zca rakam kullan\u0131n.');
      loginPending = true;
      try {
        const request: LoginRequest = { pin, cihazId: config.cihazId };
        const response: unknown = await api.post('/api/v1/auth/login', request, { anonymous: true });
        if (!isLoginResponse(response)) throw new ApiError(fallbackMessage);
        session.set(response);
      } finally { loginPending = false; }
    },
    async logout() {
      if (logoutPending) return;
      const token = session.getToken();
      if (!token) { session.clear(); return; }
      logoutPending = true;
      let notice: string | null = null;
      try { await api.post<void>('/api/v1/auth/logout'); }
      catch (error) {
        if (!(error instanceof ApiError && error.status === 401)) {
          notice = 'Bu cihazdaki oturum kapat\u0131ld\u0131. Sunucuda \u00e7\u0131k\u0131\u015f do\u011frulanamad\u0131.';
        }
      } finally {
        if (session.getToken() === token || session.getToken() === null) session.clear(notice);
        logoutPending = false;
      }
    },
    async checkHealth() {
      const result = await api.get<{ status: string }>('/api/v1/system/health', {
        anonymous: true, timeoutMs: config.healthTimeoutMs,
      });
      if (result.status !== 'Healthy') throw new ApiError('Sunucu haz\u0131r de\u011fil. Tekrar deneyin.');
    },
  };
}
export type AuthRuntime = ReturnType<typeof createAuthRuntime>;
