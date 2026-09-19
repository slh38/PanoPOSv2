import { House, ShoppingCart, Utensils, Package, Users, FileText, Wallet, ChartColumn, Settings } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';

type NavigationItem = { id: string; label: string; icon: LucideIcon; to?: string; tone: string };
export const navigationItems: readonly NavigationItem[] = [
  { id: 'home', label: 'Ana Sayfa', icon: House, to: '/', tone: 'primary' },
  { id: 'quick-sale', label: 'H\u0131zl\u0131 Sat\u0131\u015f', icon: ShoppingCart, to: '/hizli-satis', tone: 'primary' },
  { id: 'restaurant', label: 'Restaurant', icon: Utensils, tone: 'warning' },
  { id: 'stock', label: 'Stok Kartlar\u0131', icon: Package, tone: 'success' },
  { id: 'customer', label: 'Cari Kartlar', icon: Users, tone: 'primary' },
  { id: 'purchase', label: 'Al\u0131\u015f Faturalar\u0131', icon: FileText, tone: 'danger' },
  { id: 'cash', label: 'Kasa \u0130\u015flemleri', icon: Wallet, tone: 'success' },
  { id: 'reports', label: 'Raporlar', icon: ChartColumn, tone: 'primary' },
  { id: 'settings', label: 'Ayarlar', icon: Settings, tone: 'neutral' },
];
