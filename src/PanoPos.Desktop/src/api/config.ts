import source from '../../desktop.config.json';

export interface DesktopConfig {
  apiBaseUrl: string;
  cihazId: number;
  timeoutMs: number;
  healthTimeoutMs: number;
}

export function validateConfig(value: DesktopConfig): Readonly<DesktopConfig> {
  const url = new URL(value.apiBaseUrl);
  if (!['http:', 'https:'].includes(url.protocol) || url.username || url.password ||
      url.search || url.hash || url.pathname !== '/') {
    throw new Error('Desktop API configuration is invalid.');
  }
  if (!Number.isSafeInteger(value.cihazId) || value.cihazId <= 0 ||
      !Number.isSafeInteger(value.timeoutMs) || value.timeoutMs <= 0 ||
      !Number.isSafeInteger(value.healthTimeoutMs) || value.healthTimeoutMs <= 0) {
    throw new Error('Desktop device/timeout configuration is invalid.');
  }
  return Object.freeze({ ...value, apiBaseUrl: url.origin });
}

export const desktopConfig = validateConfig(source);
