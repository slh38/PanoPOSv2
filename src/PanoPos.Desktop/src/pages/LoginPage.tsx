import { useEffect, useRef, useState, useSyncExternalStore } from 'react';
import type { FormEvent, KeyboardEvent } from 'react';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { useAuth } from '../features/auth/AuthContext';
import { errorMessage } from '../api/errors';
import './auth.css';

const digits = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];

export function LoginPage() {
  const auth = useAuth();
  const { notice } = useSyncExternalStore(auth.session.subscribe, auth.session.getSnapshot);
  const [pin, setPin] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [health, setHealth] = useState<'checking' | 'connected' | 'offline'>('checking');
  const [healthError, setHealthError] = useState('');
  const pinInput = useRef<HTMLInputElement>(null);
  const pending = useRef(false);
  const checking = useRef(false);

  useEffect(() => {
    let active = true;
    auth.checkHealth().then(() => { if (active) setHealth('connected'); })
      .catch(e => { if (active) { setHealth('offline'); setHealthError(errorMessage(e)); } });
    return () => { active = false; };
  }, [auth]);

  async function retryHealth() {
    if (checking.current) return;
    checking.current = true;
    setHealth('checking');
    setHealthError('');
    try { await auth.checkHealth(); setHealth('connected'); }
    catch (e) { setHealth('offline'); setHealthError(errorMessage(e)); }
    finally { checking.current = false; pinInput.current?.focus(); }
  }

  function changePin(next: string) {
    if (pending.current) return;
    setPin(next.replace(/\D/g, ''));
    setError('');
    pinInput.current?.focus();
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (pending.current) return;
    if (!pin) { setError('PIN girin.'); pinInput.current?.focus(); return; }
    pending.current = true;
    setSubmitting(true);
    setError('');
    try { await auth.login(pin); }
    catch (e) { setError(errorMessage(e)); }
    finally {
      setPin('');
      pending.current = false;
      setSubmitting(false);
      pinInput.current?.focus();
    }
  }

  function keyboard(event: KeyboardEvent<HTMLFormElement>) {
    if (event.ctrlKey || event.metaKey || event.altKey || event.nativeEvent.isComposing) return;
    if (event.key === 'Escape') { event.preventDefault(); changePin(''); }
    if (event.target !== pinInput.current && /^\d$/.test(event.key)) {
      event.preventDefault(); changePin(pin + event.key);
    }
    if (event.target !== pinInput.current && event.key === 'Backspace') {
      event.preventDefault(); changePin(pin.slice(0, -1));
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-shell" aria-label="PanoPOS">
        <aside className="brand-panel">
          <div className="brand-wordmark">Pano<span>POS</span></div>
          <div className="brand-intro">
            <p className="eyebrow">PANOPOS DESKTOP</p>
            <h1>{'\u0130\u015fletmenizin\n\u00e7al\u0131\u015fma noktas\u0131.'}</h1>
            <p>{'Sat\u0131\u015fa ba\u015flamak i\u00e7in\noturumunuzu a\u00e7\u0131n.'}</p>
          </div>
          <svg className="terminal-art" viewBox="0 0 360 210" fill="none" aria-hidden="true">
            <rect x="48" y="20" width="232" height="142" rx="8" />
            <path d="M60 146h208M142 162v27m42-27v27m-72 6h103" />
            <rect x="72" y="42" width="76" height="83" rx="4" />
            <path d="M163 47h88m-88 15h68m-68 24h36m-36 17h36m-36 17h36" />
            <rect x="215" y="86" width="36" height="39" rx="3" />
            <rect x="270" y="112" width="56" height="80" rx="6" />
            <path d="M280 130h36m-36 14h8m8 0h8m-24 12h8m8 0h8m-24 12h24" />
          </svg>
          <div className="brand-footer"><span>{'Sade. H\u0131zl\u0131. Size ait.'}</span><span>01 / Desktop</span></div>
        </aside>
        <div className="login-panel">
          <form className="login-form" onSubmit={submit} onKeyDown={keyboard} aria-label="PIN ile oturum">
            <div className="user-mark" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="4" /><path d="M4 22v-3a8 8 0 0 1 16 0v3" /></svg>
            </div>
            <h2>{'Ho\u015f geldiniz'}</h2>
            <p className="login-caption">{'Devam etmek i\u00e7in PIN kodunuzu girin.'}</p>
            <Input ref={pinInput} autoFocus label="PIN" type="password" inputMode="numeric"
              autoComplete="off" spellCheck={false} value={pin} readOnly={submitting}
              onChange={e => changePin(e.target.value)} className="pin-input"
              aria-invalid={Boolean(error)} aria-describedby="login-message" />
            <div id="login-message" className="login-message" role="alert">{error || notice}</div>
            <div className="pin-keypad">
              {digits.map(digit => <Button key={digit} variant="secondary" disabled={submitting}
                onClick={() => changePin(pin + digit)}>{digit}</Button>)}
              <Button variant="secondary" disabled={submitting} aria-label="Son rakami sil"
                onClick={() => changePin(pin.slice(0, -1))}>Sil</Button>
              <Button variant="secondary" disabled={submitting} onClick={() => changePin(pin + '0')}>0</Button>
              <Button type="submit" disabled={submitting} aria-label="Giris yap">
                {submitting ? 'Bekleyin' : 'Giri\u015f'}
              </Button>
            </div>
            <p className="keyboard-hint">{submitting ? 'Oturum a\u00e7\u0131l\u0131yor...' : 'Enter: Giri\u015f   /   Esc: Temizle'}</p>
          </form>
          <div className="login-footer">
            <div className={`connection connection--${health}`} role="status">
              <span className="connection-dot" aria-hidden="true" />
              {health === 'checking' ? 'Sunucu kontrol ediliyor' : health === 'connected' ? 'Sunucu ba\u011fl\u0131' : 'Sunucuya ula\u015f\u0131lam\u0131yor'}
            </div>
            <span>Cihaz {auth.config.cihazId}</span>
            {health === 'offline' && <div className="connection-error">
              <p>{healthError}</p><Button variant="secondary" onClick={retryHealth}>Tekrar dene</Button>
            </div>}
          </div>
        </div>
      </section>
    </main>
  );
}
