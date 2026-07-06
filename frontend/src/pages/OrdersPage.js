import React from 'react';
import Navbar from '../components/Navbar';
import { useOrders } from '../hooks/useOrders';
import { ordersApi } from '../services/ordersApi';

const STATUS_CLASS = {
  Pending: 'badge-pending', Processing: 'badge-processing',
  Shipped: 'badge-shipped', Delivered: 'badge-delivered', Cancelled: 'badge-cancelled'
};

export default function OrdersPage() {
  const { orders, total, page, setPage, pageSize, loading, error, refresh } = useOrders();

  const handleCancel = async id => {
    if (!window.confirm('Cancel this order?')) return;
    try { await ordersApi.cancelOrder(id); refresh(); }
    catch { alert('Could not cancel order.'); }
  };

  return (
    <div>
      <Navbar />
      <div className="container">
        <h2 className="page-title">📦 My Orders</h2>

        {error   && <div className="alert alert-error">{error}</div>}
        {loading && <div className="loading">Loading orders…</div>}

        {!loading && orders.length === 0 && (
          <div className="empty-state">No orders yet. <a href="/products">Start shopping →</a></div>
        )}

        {orders.length > 0 && (
          <table className="orders-table">
            <thead>
              <tr>
                <th>#</th><th>Status</th><th>Items</th><th>Total</th><th>Date</th><th>Action</th>
              </tr>
            </thead>
            <tbody>
              {orders.map(o => (
                <tr key={o.id}>
                  <td>#{o.id}</td>
                  <td><span className={`badge ${STATUS_CLASS[o.status] || ''}`}>{o.status}</span></td>
                  <td>{o.itemCount}</td>
                  <td>${Number(o.totalAmount).toFixed(2)}</td>
                  <td>{new Date(o.createdAt).toLocaleDateString()}</td>
                  <td>
                    {o.status === 'Pending' && (
                      <button className="btn btn-sm btn-danger" onClick={() => handleCancel(o.id)}>
                        Cancel
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {total > pageSize && (
          <div className="pagination">
            <button className="btn btn-sm" disabled={page === 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
            <span>Page {page} of {Math.ceil(total / pageSize)}</span>
            <button className="btn btn-sm" disabled={page * pageSize >= total} onClick={() => setPage(p => p + 1)}>Next →</button>
          </div>
        )}
      </div>
    </div>
  );
}
