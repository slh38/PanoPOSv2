import { useEffect, useState } from 'react';

export function LocalClock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => {
    const timer = window.setInterval(() => setNow(new Date()), 60_000);
    return () => window.clearInterval(timer);
  }, []);
  return <time className="local-clock" dateTime={now.toISOString()} aria-label="Yerel tarih ve saat">
    <span>{now.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' })}
      <small>{now.toLocaleDateString('tr-TR', { weekday: 'long' })}</small></span>
    <strong>{now.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</strong>
  </time>;
}
