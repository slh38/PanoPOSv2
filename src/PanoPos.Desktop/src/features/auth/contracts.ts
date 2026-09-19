export interface LoginRequest {
  pin: string;
  cihazId: number;
}

export interface SubeBilgisi {
  subeId: number;
  ad: string;
}

export interface LoginResponse {
  oturumToken: string;
  tenantId: string;
  subeId: number;
  kullaniciId: number;
  cihazId: number;
  adSoyad: string;
  oturumId: number;
  varsayilanSubeId: number;
  roller: string[];
  subeler: SubeBilgisi[];
}

export type SessionInfo = Omit<LoginResponse, 'oturumToken'>;

export function isLoginResponse(value: unknown): value is LoginResponse {
  if (typeof value !== 'object' || value === null) return false;
  const data = value as Record<string, unknown>;
  return typeof data.oturumToken === 'string' && data.oturumToken.length > 0 &&
    typeof data.tenantId === 'string' && data.tenantId.length > 0 &&
    typeof data.adSoyad === 'string' &&
    ['subeId', 'kullaniciId', 'cihazId', 'oturumId', 'varsayilanSubeId']
      .every(key => typeof data[key] === 'number' && Number.isSafeInteger(data[key]) && (data[key] as number) > 0) &&
    Array.isArray(data.roller) && data.roller.every(role => typeof role === 'string') &&
    Array.isArray(data.subeler) && data.subeler.every(branch =>
      typeof branch === 'object' && branch !== null && Number.isSafeInteger(branch.subeId) &&
      branch.subeId > 0 && typeof branch.ad === 'string');
}
