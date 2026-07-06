import React, { useState } from 'react';
import Navbar from '../components/Navbar';
import { useProducts } from '../hooks/useProducts';
import { ordersApi } from '../services/ordersApi';

const CATEGORIES = ['All', 'Electronics', 'Accessories', 'Audio', 'Storage', 'Office'];

export default function ProductsPage() {
  const [search,   setSearch]   = useState('');
  const [category, setCategory] = useState('');
  const [msg,      setMsg]      = useState('');

  const { products, loading, error, setParams, params } = useProducts();

  const handleSearch = e => {
    e.preventDefault();
    setParams(p => ({ ...p, search, category: category === 'All' ? '' : category, page: 1 }));
  };

  const handleOrder = async product => {
    try {
      await ordersApi.placeOrder({ items: [{ productId: product.id, quantity: 1 }], notes: '' });
      setMsg(`✅ Order placed for "${product.name}"!`);
      setTimeout(() => setMsg(''), 4000);
    } catch (err) {
      setMsg(`❌ ${err.response?.data?.message || 'Could not place order.'}`);
      setTimeout(() => setMsg(''), 4000);
    }
  };

  return (
    <div>
      <Navbar />
      <div className="container">
        <h2 className="page-title">🏷️ Products</h2>

        {msg && <div className={`alert ${msg.startsWith('✅') ? 'alert-success' : 'alert-error'}`}>{msg}</div>}

        <form onSubmit={handleSearch} className="search-bar">
          <input type="text" placeholder="Search products…"
            value={search} onChange={e => setSearch(e.target.value)} />
          <select value={category} onChange={e => setCategory(e.target.value)}>
            {CATEGORIES.map(c => <option key={c} value={c === 'All' ? '' : c}>{c}</option>)}
          </select>
          <button type="submit" className="btn btn-primary">Search</button>
        </form>

        {error   && <div className="alert alert-error">{error}</div>}
        {loading && <div className="loading">Loading products…</div>}

        <div className="products-grid">
          {products.map(p => (
            <div key={p.id} className="product-card">
              <div className="product-category">{p.category}</div>
              <h3 className="product-name">{p.name}</h3>
              <p className="product-desc">{p.description}</p>
              <div className="product-footer">
                <span className="product-price">${p.price.toFixed(2)}</span>
                <span className={`product-stock ${p.stock < 10 ? 'low-stock' : ''}`}>
                  {p.stock < 10 ? `⚠️ Only ${p.stock} left` : `${p.stock} in stock`}
                </span>
              </div>
              <button
                className="btn btn-primary btn-full"
                onClick={() => handleOrder(p)}
                disabled={p.stock === 0}>
                {p.stock === 0 ? 'Out of Stock' : '🛒 Order Now'}
              </button>
            </div>
          ))}
        </div>

        {products.length === 0 && !loading && (
          <div className="empty-state">No products found.</div>
        )}
      </div>
    </div>
  );
}
