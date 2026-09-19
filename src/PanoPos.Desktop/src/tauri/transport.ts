import { isTauri } from '@tauri-apps/api/core';
import type { HttpTransport } from '../api/client';

export const desktopTransport: HttpTransport = async (url, init) => {
  if (isTauri()) {
    const { fetch } = await import('@tauri-apps/plugin-http');
    return fetch(url, { ...init, maxRedirections: 0 });
  }
  // Vite's development-only same-origin proxy; native builds use scoped Tauri HTTP.
  const target = new URL(url);
  return globalThis.fetch(target.pathname + target.search, init);
};
