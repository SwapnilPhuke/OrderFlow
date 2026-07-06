import React from 'react';
import Navbar from '../components/Navbar';
import { useDashboard } from '../hooks/useDashboard';

const CARDS = [
  { key: 'totalOrders',    label: 'Total Orders',    icon: '📦', color: '#667eea' },
  { key: 'totalRevenue',   label: 'Revenue',         icon: '💰', color: '#4caf50', prefix: '$' },
  { key: 'pendingOrders',  label: 'Pending',         icon: '⏳', color: '#ff9800' },
  { key: 'processingOrders', label: 'Processing',   icon: '⚙️',  color: '#2196f3' },
  { key: 'shippedOrders',  label: 'Shipped',         icon: '🚚', color: '#9c27b0' },
  { key: 'deliveredOrders', label: 'Delivered',      icon: '✅', color: '#1b5e20' },
  { key: 'cancelledOrders', label: 'Cancelled',      icon: '❌', color: '#f44336' },
  { key: 'totalProducts',  label: 'Products',        icon: '🏷️', color: '#607d8b' },
  { key: 'lowStockProducts', label: 'Low Stock',     icon: '⚠️', color: '#e91e63' },
];

export default function DashboardPage() {
  const { stats, loading, error } = useDashboard();

  return (
    <div>
      <Navbar />
      <div className="container">
        <h2 className="page-title">📊 Admin Dashboard</h2>

        {error   && <div className="alert alert-error">{error}</div>}
        {loading && <div className="loading">Loading stats…</div>}

        {stats && (
          <div className="stats-grid">
            {CARDS.map(({ key, label, icon, color, prefix = '' }) => (
              <div key={key} className="stat-card" style={{ borderTop: `4px solid ${color}` }}>
                <span className="stat-icon">{icon}</span>
                <div>
                  <div className="stat-value" style={{ color }}>
                    {prefix}{key === 'totalRevenue' ? Number(stats[key]).toFixed(2) : stats[key]}
                  </div>
                  <div className="stat-label">{label}</div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
