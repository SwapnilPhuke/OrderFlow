import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute  from './components/ProtectedRoute';
import LoginPage       from './pages/LoginPage';
import RegisterPage    from './pages/RegisterPage';
import DashboardPage   from './pages/DashboardPage';
import ProductsPage    from './pages/ProductsPage';
import OrdersPage      from './pages/OrdersPage';
import AdminPage       from './pages/AdminPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login"    element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route path="/products" element={
            <ProtectedRoute><ProductsPage /></ProtectedRoute>
          } />
          <Route path="/orders" element={
            <ProtectedRoute><OrdersPage /></ProtectedRoute>
          } />
          <Route path="/dashboard" element={
            <ProtectedRoute adminOnly><DashboardPage /></ProtectedRoute>
          } />
          <Route path="/admin" element={
            <ProtectedRoute adminOnly><AdminPage /></ProtectedRoute>
          } />

          <Route path="/"  element={<Navigate to="/products" replace />} />
          <Route path="*"  element={<Navigate to="/products" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
