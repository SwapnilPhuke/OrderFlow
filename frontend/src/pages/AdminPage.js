import React, { useState } from 'react';
import Navbar from '../components/Navbar';
import { productsApi } from '../services/productsApi';

const EMPTY = { name: '', description: '', price: '', stock: '', category: '' };

export default function AdminPage() {
  const [form,    setForm]    = useState(EMPTY);
  const [msg,     setMsg]     = useState('');
  const [loading, setLoading] = useState(false);

  const set = f => e => setForm(prev => ({ ...prev, [f]: e.target.value }));

  const handleCreate = async e => {
    e.preventDefault();
    setLoading(true);
    setMsg('');
    try {
      await productsApi.createProduct({
        ...form,
        price: parseFloat(form.price),
        stock: parseInt(form.stock, 10)
      });
      setMsg(`✅ Product "${form.name}" created successfully.`);
      setForm(EMPTY);
    } catch (err) {
      setMsg(`❌ ${err.response?.data?.message || 'Failed to create product.'}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Navbar />
      <div className="container">
        <h2 className="page-title">⚙️ Admin — Create Product</h2>

        {msg && <div className={`alert ${msg.startsWith('✅') ? 'alert-success' : 'alert-error'}`}>{msg}</div>}

        <div className="card">
          <form onSubmit={handleCreate} className="admin-form">
            {[
              { id: 'name',        label: 'Product Name', type: 'text',   placeholder: 'e.g. Wireless Mouse' },
              { id: 'category',    label: 'Category',     type: 'text',   placeholder: 'e.g. Electronics' },
              { id: 'price',       label: 'Price ($)',    type: 'number', placeholder: '49.99', step: '0.01', min: '0' },
              { id: 'stock',       label: 'Stock',        type: 'number', placeholder: '100',  min: '0' },
            ].map(({ id, label, ...rest }) => (
              <div key={id} className="form-group">
                <label htmlFor={id}>{label}</label>
                <input id={id} value={form[id]} onChange={set(id)} required {...rest} />
              </div>
            ))}
            <div className="form-group">
              <label htmlFor="description">Description</label>
              <textarea id="description" value={form.description} onChange={set('description')}
                placeholder="Product description…" rows={3} required />
            </div>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Creating…' : '+ Create Product'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
