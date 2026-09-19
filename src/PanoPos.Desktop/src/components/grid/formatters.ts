type DisplayNumber = number | null | undefined;
const money = new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const quantity = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 4 });
const decimal = new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 4, maximumFractionDigits: 4 });

function display(value: DisplayNumber, formatter: Intl.NumberFormat): string {
  return typeof value === 'number' && Number.isFinite(value) ? formatter.format(value) : '';
}

// Display only. Backend amounts are never recalculated or mutated here.
export function formatMoney(value: DisplayNumber): string { return display(value, money); }
export function formatQuantity(value: DisplayNumber): string { return display(value, quantity); }
export function formatDecimal(value: DisplayNumber): string { return display(value, decimal); }
