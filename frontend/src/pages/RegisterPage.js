import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate     = useNavigate();
  const [form, setForm]   = useState({ username: '', email: '', password: '', fullName: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const set = field => e => setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async e => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await register(form.username, form.email, form.password, form.fullName);
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Username or email may already be taken.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-wrapper">
      <div className="auth-card">
        <div className="auth-header">
          <div className="auth-logo">🛒</div>
          <h1>Create Account</h1>
          <p>Join OrderFlow today</p>
        </div>

        {error && <div className="alert alert-error" role="alert">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          {[
            { id: 'fullName', label: 'Full Name', type: 'text', placeholder: 'Jane Doe' },
            { id: 'username', label: 'Username',  type: 'text', placeholder: 'janedoe' },
            { id: 'email',    label: 'Email',     type: 'email', placeholder: 'jane@example.com' },
            { id: 'password', label: 'Password',  type: 'password', placeholder: 'Min 8 chars, uppercase, digit, symbol' },
          ].map(({ id, label, type, placeholder }) => (
            <div key={id} className="form-group">
              <label htmlFor={id}>{label}</label>
              <input id={id} type={type} value={form[id]}
                onChange={set(id)} placeholder={placeholder}
                required disabled={loading} />
            </div>
          ))}
          <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
            {loading ? 'Creating account…' : 'Create Account'}
          </button>
        </form>

        <p className="auth-footer">Already have an account? <Link to="/login">Sign in</Link></p>
      </div>
    </div>
  );
}
