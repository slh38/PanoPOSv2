const keys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ',', 'Sil'];

export function nextDemoMultiplier(current: string, key: string): string {
  if (key === 'Sil') return current.slice(0, -1);
  if (current.length >= 8 || (key === ',' && current.includes(','))) return current;
  return current + (key === ',' && !current ? '0,' : key);
}

export function QuickSaleNumpad({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  return <section className="qs-numpad" aria-label="Sayısal tuş takımı">
    <div className="qs-multiplier"><span>Çarpan</span><output aria-label="Demo çarpan">{value || '1'} ×</output></div>
    {keys.map(key => <button type="button" key={key} className={key === 'Sil' ? 'qs-key qs-key--delete' : 'qs-key'}
      onClick={() => onChange(nextDemoMultiplier(value, key))}>{key}</button>)}
  </section>;
}
