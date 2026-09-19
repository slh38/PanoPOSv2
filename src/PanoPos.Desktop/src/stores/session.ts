import type { LoginResponse, SessionInfo } from '../features/auth/contracts';

export interface SessionSnapshot {
  session: SessionInfo | null;
  notice: string | null;
}

export function createSessionStore() {
  let token: string | null = null;
  let snapshot: SessionSnapshot = { session: null, notice: null };
  const listeners = new Set<() => void>();
  const notify = () => listeners.forEach(listener => listener());
  return {
    getSnapshot: () => snapshot,
    getToken: () => token,
    subscribe(listener: () => void) {
      listeners.add(listener);
      return () => { listeners.delete(listener); };
    },
    set(response: LoginResponse) {
      const { oturumToken, ...session } = response;
      token = oturumToken;
      snapshot = { session, notice: null };
      notify();
    },
    clear(notice: string | null = null) {
      token = null;
      snapshot = { session: null, notice };
      notify();
    },
    expire(requestToken: string) {
      // A late response from an old session must not clear a newer login.
      if (token === requestToken) this.clear('Oturum sona erdi. Yeniden giri\u015f yap\u0131n.');
    },
  };
}
export type SessionStore = ReturnType<typeof createSessionStore>;
