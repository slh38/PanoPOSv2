const messages: Record<string, string> = {
  pin_required: 'PIN girin.',
  invalid_pin: 'PIN hatal\u0131.',
  cihaz_required: 'Terminal cihaz ayar\u0131 eksik.',
  cihaz_invalid: 'Cihaz bulunamad\u0131 veya aktif de\u011fil. Terminal ayar\u0131n\u0131 kontrol edin.',
  session_invalid: 'Oturum veya cihaz/\u015fube ba\u011flant\u0131s\u0131 ge\u00e7ersiz. Yeniden giri\u015f yap\u0131n.',
  duplicate_pin: 'PIN tan\u0131m\u0131nda \u00e7ak\u0131\u015fma var. Y\u00f6neticinize ba\u015fvurun.',
  user_passive: 'Kullan\u0131c\u0131 aktif de\u011fil.',
  user_locked: 'Kullan\u0131c\u0131 kilitli. Y\u00f6neticinize ba\u015fvurun.',
  sube_not_authorized: 'Bu cihaz\u0131n \u015fubesinde kullan\u0131m yetkiniz yok.',
};

export const networkMessage = 'Ana makineye ula\u015f\u0131lam\u0131yor. A\u011f ba\u011flant\u0131s\u0131n\u0131 kontrol edin.';
export const fallbackMessage = '\u0130\u015flem tamamlanamad\u0131. Tekrar deneyin.';

export class ApiError extends Error {
  constructor(message: string, public readonly status = 0) { super(message); }
}

export function problemError(status: number, problem: unknown): ApiError {
  const type = typeof problem === 'object' && problem !== null && 'type' in problem ? problem.type : null;
  const code = typeof type === 'string' && type.startsWith('https://panopos/errors/')
    ? type.slice('https://panopos/errors/'.length) : '';
  // Only known domain codes are displayed; raw detail/title may contain sensitive data.
  const message = messages[code] ?? (status === 401
    ? 'Oturum sona erdi. Yeniden giri\u015f yap\u0131n.' : fallbackMessage);
  return new ApiError(message, status);
}

export function errorMessage(error: unknown): string {
  return error instanceof ApiError ? error.message : fallbackMessage;
}
