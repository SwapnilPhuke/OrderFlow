import axios from 'axios';

const BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000';
const api  = axios.create({ baseURL: `${BASE}/api/v1` });

export const authApi = {
  register: (data)          => api.post('/auth/register', data),
  login:    (data)          => api.post('/auth/login', data),
  refresh:  (refreshToken)  => api.post('/auth/refresh', { refreshToken }),
  logout:   (refreshToken)  => api.post('/auth/logout',  { refreshToken }),
};
