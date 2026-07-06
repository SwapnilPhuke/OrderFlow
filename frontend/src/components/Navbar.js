import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { currentUser, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => { await logout(); navigate('/login'); };

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/products">🛒 OrderFlow</Link>
      </div>
      <div className="navbar-links">
        <Link to="/products">Products</Link>
        <Link to="/orders">My Orders</Link>
        {isAdmin && <Link to="/dashboard">Dashboard</Link>}
        {isAdmin && <Link to="/admin">Admin</Link>}
      </div>
      <div className="navbar-user">
        <span className="navbar-username">👤 {currentUser?.username}</span>
        {isAdmin && <span className="badge badge-admin">Admin</span>}
        <button onClick={handleLogout} className="btn btn-sm btn-outline">Logout</button>
      </div>
    </nav>
  );
}
