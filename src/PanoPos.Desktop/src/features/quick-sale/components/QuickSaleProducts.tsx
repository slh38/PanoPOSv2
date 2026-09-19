import { useRef, useState } from 'react';
import { Apple, BottleWine, ChevronRight, Coffee, Cookie, Croissant, Grid2X2, ImageOff, Milk, Package, Search } from 'lucide-react';
import { formatMoney } from '../../../components/grid/formatters';
import { demoCategories, demoProducts } from '../demoQuickSaleData';
import type { DemoProduct } from '../demoQuickSaleData';

const artwork = { bottle: BottleWine, carton: Milk, coffee: Coffee, bread: Croissant, snack: Cookie, fruit: Apple, package: Package };

function ProductCard({ product, selected, onSelect }: { product: DemoProduct; selected: boolean; onSelect: () => void }) {
  const Art = product.art ? artwork[product.art] : ImageOff;
  return <button type="button" className="qs-product" aria-pressed={selected} onClick={onSelect}>
    <span className={`qs-product-art qs-product-art--${product.tone}`} aria-hidden="true"><Art strokeWidth={1.25} /></span>
    <span className="qs-product-name">{product.name}</span>
    <span className="qs-product-pack">{product.pack}</span>
    <strong>{formatMoney(product.price)} TL</strong>
  </button>;
}

export function QuickSaleProducts({ onSelect }: { onSelect: (product: DemoProduct) => void }) {
  const [category, setCategory] = useState('Tümü');
  const [search, setSearch] = useState('');
  const [selected, setSelected] = useState<string | null>(null);
  const categories = useRef<HTMLDivElement>(null);
  const products = demoProducts.filter(p => (category === 'Tümü' || p.category === category)
    && p.name.toLocaleLowerCase('tr-TR').includes(search.trim().toLocaleLowerCase('tr-TR')));
  return <section className="qs-catalog" aria-label="Demo ürünler">
    <div className="qs-category-bar">
      <div className="qs-categories" ref={categories} role="group" aria-label="Kategoriler">
        {demoCategories.map(label => <button type="button" key={label} aria-pressed={label === category}
          onClick={() => setCategory(label)}>{label}</button>)}
      </div>
      <button type="button" className="qs-icon-button" aria-label="Sonraki kategoriler"
        onClick={() => categories.current?.scrollBy({ left: 200, behavior: 'instant' })}><ChevronRight aria-hidden="true" /></button>
    </div>
    <div className="qs-search"><Search aria-hidden="true" /><input aria-label="Ürün ara" placeholder="Ürün ara..."
      value={search} onChange={e => setSearch(e.target.value)} /><Grid2X2 aria-hidden="true" /></div>
    <div className="qs-product-scroll" tabIndex={0} role="region" aria-label="Ürün kartları">
      <div className="qs-products">{products.map(product => <ProductCard key={product.id} product={product} selected={product.id === selected}
        onSelect={() => { setSelected(product.id); onSelect(product); }} />)}</div>
      {products.length === 0 && <p className="qs-empty" role="status">Demo ürün bulunamadı.</p>}
    </div>
    <span className="qs-catalog-footnote">{products.length} demo ürün · Temsili görseller</span>
  </section>;
}
