import type { LoginResponse } from '../src/features/auth/contracts';
import type { DesktopConfig } from '../src/api/config';

export const config: DesktopConfig = {
  apiBaseUrl: 'http://localhost:5296', cihazId: 7, timeoutMs: 1000, healthTimeoutMs: 1000,
};
export const login: LoginResponse = {
  oturumToken: 'test-only-token', tenantId: '11111111-1111-1111-1111-111111111111',
  subeId: 3, kullaniciId: 9, cihazId: 7, adSoyad: 'Test Kullanici',
  oturumId: 11, varsayilanSubeId: 3, roller: ['Admin'],
  subeler: [{ subeId: 3, ad: 'Test Sube' }],
};
export const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), {
  status, headers: { 'Content-Type': 'application/json' },
});
export const invalidPin = () => json({ type: 'https://panopos/errors/invalid_pin', detail: 'DO NOT DISPLAY' }, 401);
