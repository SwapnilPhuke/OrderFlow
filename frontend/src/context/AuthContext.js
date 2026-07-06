import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { authApi } from '../services/authApi';

const TOKEN_KEY   = 'of_token';
const REFRESH_KEY = 'of_refresh';

export const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }) {
  const [token,       setToken]       = useState(() => localStorage.getItem(TOKEN_KEY));
  const [currentUser, setCurrentUser] = useState(null);
  const [loadingUser, setLoadingUser] = useState(true);

  const applyTokens = useCallback((access, refresh) => {
    localStorage.setItem(TOKEN_KEY, access);
    if (refresh) localStorage.setItem(REFRESH_KEY, refresh);
    axios.defaults.headers.common['Authorization'] = `Bearer ${access}`;
    setToken(access);
  }, []);

  const logout = useCallback(async () => {
    const stored = localStorage.getItem(REFRESH_KEY);
    if (stored) { try { await authApi.logout(stored); } catch (_) {} }
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    delete axios.defaults.headers.common['Authorization'];
    setToken(null);
    setCurrentUser(null);
  }, []);

  // Sync axios header with stored token
  useEffect(() => {
    if (token) axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    else       delete axios.defaults.headers.common['Authorization'];
  }, [token]);

  // 401 interceptor — auto-refresh
  useEffect(() => {
    const id = axios.interceptors.response.use(
      res => res,
      async err => {
        const original = err.config;
        if (err.response?.status === 401 && !original._retry) {
          original._retry = true;
          const stored = localStorage.getItem(REFRESH_KEY);
          if (stored) {
            try {
              const res = await authApi.refresh(stored);
              applyTokens(res.data.token, res.data.refreshToken);
              original.headers['Authorization'] = `Bearer ${res.data.token}`;
              return axios(original);
            } catch (_) {}
          }
          logout();
        }
        return Promise.reject(err);
      }
    );
    return () => axios.interceptors.response.eject(id);
  }, [applyTokens, logout]);

  // Decode user from stored token on mount
  useEffect(() => {
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        setCurrentUser({
          id:       parseInt(payload.sub),
          username: payload.unique_name,
          email:    payload.email,
          role:     payload.role,
        });
      } catch (_) { logout(); }
    }
    setLoadingUser(false);
  }, [token, logout]);

  const login = async (username, password) => {
    const res = await authApi.login({ username, password });
    applyTokens(res.data.token, res.data.refreshToken);
    const payload = JSON.parse(atob(res.data.token.split('.')[1]));
    setCurrentUser({ id: res.data.userId, username: res.data.username, email: res.data.email, role: res.data.role });
  };

  const register = async (username, email, password, fullName) => {
    await authApi.register({ username, email, password, fullName });
  };

  return (
    <AuthContext.Provider value={{
      token, currentUser, loadingUser,
      isAuthenticated: !!token,
      isAdmin: currentUser?.role === 'Admin',
      login, logout, register
    }}>
      {children}
    </AuthContext.Provider>
  );
}
