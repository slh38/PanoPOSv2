import { useEffect, useRef, useState } from 'react';
import { ChevronDown, LogOut, UserRound } from 'lucide-react';
import { useAuth } from '../features/auth/AuthContext';

export function UserMenu({ name }: { name: string }) {
  const auth = useAuth();
  const [open, setOpen] = useState(false);
  const [pending, setPending] = useState(false);
  const root = useRef<HTMLDivElement>(null);
  const trigger = useRef<HTMLButtonElement>(null);
  useEffect(() => {
    if (!open) return;
    const outside = (event: PointerEvent) => {
      if (event.target instanceof Node && !root.current?.contains(event.target)) setOpen(false);
    };
    const escape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') { setOpen(false); trigger.current?.focus(); }
    };
    document.addEventListener('pointerdown', outside);
    document.addEventListener('keydown', escape);
    return () => {
      document.removeEventListener('pointerdown', outside);
      document.removeEventListener('keydown', escape);
    };
  }, [open]);

  async function logout() {
    if (pending) return;
    setPending(true);
    try { await auth.logout(); } finally { setPending(false); }
  }

  return <div className="user-menu" ref={root} onBlur={event => {
    if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false);
  }}>
    <button ref={trigger} className="user-trigger" aria-label={`Kullan\u0131c\u0131 men\u00fcs\u00fc: ${name}`}
      aria-expanded={open} aria-controls="user-actions" onClick={() => setOpen(!open)}>
      <UserRound aria-hidden="true" /><span title={name}>{name}</span><ChevronDown aria-hidden="true" />
    </button>
    {open && <div id="user-actions" className="user-actions">
      <p>{name}</p>
      <button disabled={pending} onClick={logout}><LogOut aria-hidden="true" />
        {pending ? '\u00c7\u0131k\u0131\u015f yap\u0131l\u0131yor...' : '\u00c7\u0131k\u0131\u015f Yap'}</button>
    </div>}
  </div>;
}
