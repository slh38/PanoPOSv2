import { CircleUserRound, Pause, ListTodo, UserRoundSearch, Tag, Percent, X, Ellipsis } from 'lucide-react';

const actions = [
  { label: 'Borç', icon: CircleUserRound },
  { label: 'Beklet', icon: Pause },
  { label: 'Bekleyen Satışlar', icon: ListTodo },
  { label: 'Müşteri Seç', icon: UserRoundSearch },
  { label: 'Fiyat Gör', icon: Tag },
  { label: 'İskonto', icon: Percent },
  { label: 'Satış İptal', icon: X, danger: true },
  { label: 'Diğer İşlemler', icon: Ellipsis },
];

export function QuickSaleActions({ onAction }: { onAction: (label: string) => void }) {
  return <aside className="qs-actions" aria-label="Satış işlemleri">
    {actions.map(({ label, icon: Icon, danger }) => <button type="button" key={label}
      className={`qs-action${danger ? ' qs-action--danger' : ''}`} onClick={() => onAction(label)}>
      <Icon aria-hidden="true" /><span>{label}</span>
    </button>)}
  </aside>;
}
