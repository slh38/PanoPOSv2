// Local visual fixtures only. No identifiers or amounts are sent to the API.
export interface DemoCartRow {
  id: string;
  name: string;
  quantity: number;
  unit: string;
  price: number;
  discount: number;
  total: number;
}

export type ProductArt = 'bottle' | 'carton' | 'coffee' | 'bread' | 'snack' | 'fruit' | 'package';
export interface DemoProduct {
  id: string;
  name: string;
  pack: string;
  price: number;
  category: string;
  art?: ProductArt;
  tone: 'primary' | 'success' | 'warning' | 'danger' | 'neutral';
}

export const demoCartRows: DemoCartRow[] = [
  { id: 'demo-cola', name: 'Coca Cola 1.5 L', quantity: 2, unit: 'Adet', price: 60, discount: 0, total: 120 },
  { id: 'demo-lays', name: 'Lays Klasik 100 g', quantity: 1, unit: 'Adet', price: 45, discount: 0, total: 45 },
  { id: 'demo-eti', name: 'Eti Çikolata 60 g', quantity: 3, unit: 'Adet', price: 40, discount: 0, total: 120 },
];

// Display fixture, not a second pricing/tax/discount engine.
export const demoTotals = { subtotal: 285, discount: 0, total: 285 };
export const demoCategories = ['Tümü', 'İçecekler', 'Atıştırmalık', 'Temizlik', 'Gıda', 'Fırın', 'Manav'];
export const demoProducts: DemoProduct[] = [
  { id: 'cola', name: 'Coca Cola', pack: '1.5 L', price: 60, category: 'İçecekler', art: 'bottle', tone: 'danger' },
  { id: 'fanta', name: 'Fanta', pack: '1.5 L', price: 60, category: 'İçecekler', art: 'bottle', tone: 'warning' },
  { id: 'sprite', name: 'Sprite', pack: '1.5 L', price: 60, category: 'İçecekler', art: 'bottle', tone: 'success' },
  { id: 'water', name: 'Uludağ Su', pack: '0.5 L', price: 10, category: 'İçecekler', art: 'bottle', tone: 'primary' },
  { id: 'lays', name: 'Lays Klasik', pack: '100 g', price: 45, category: 'Atıştırmalık', art: 'snack', tone: 'warning' },
  { id: 'doritos', name: 'Doritos', pack: '100 g', price: 45, category: 'Atıştırmalık', art: 'snack', tone: 'success' },
  { id: 'eti', name: 'Eti Çikolata', pack: '60 g', price: 40, category: 'Atıştırmalık', art: 'snack', tone: 'danger' },
  { id: 'biscuit', name: 'Ülker Bisküvi', pack: '80 g', price: 25, category: 'Atıştırmalık', art: 'snack', tone: 'warning' },
  { id: 'simit', name: 'Simit', pack: 'Adet', price: 15, category: 'Fırın', art: 'bread', tone: 'warning' },
  { id: 'pogaca', name: 'Poğaça', pack: 'Adet', price: 20, category: 'Fırın', art: 'bread', tone: 'warning' },
  { id: 'acma', name: 'Açma', pack: 'Adet', price: 20, category: 'Fırın', art: 'bread', tone: 'warning' },
  { id: 'tea', name: 'Çay', pack: 'Bardak', price: 12, category: 'İçecekler', art: 'coffee', tone: 'danger' },
  { id: 'coffee', name: 'Türk Kahvesi', pack: 'Fincan', price: 25, category: 'İçecekler', art: 'coffee', tone: 'neutral' },
  { id: 'milk', name: 'Süt', pack: '1 L', price: 30, category: 'Gıda', art: 'carton', tone: 'primary' },
  { id: 'apple', name: 'Elma', pack: 'Kg', price: 35, category: 'Manav', art: 'fruit', tone: 'success' },
  { id: 'rice', name: 'Pirinç', pack: '1 Kg', price: 65, category: 'Gıda', art: 'package', tone: 'warning' },
  { id: 'soap', name: 'Sıvı Sabun', pack: '500 ml', price: 55, category: 'Temizlik', art: 'bottle', tone: 'primary' },
  { id: 'tissue', name: 'Kağıt Havlu', pack: '2 rulo', price: 40, category: 'Temizlik', tone: 'neutral' },
];
