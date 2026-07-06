import axios from 'axios';

const BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000';
const api  = axios.create({ baseURL: `${BASE}/api/v1` });

export const productsApi = {
  getProducts:    (params) => api.get('/products', { params }),
  getProductById: (id)     => api.get(`/products/${id}`),
  createProduct:  (data)   => api.post('/products', data),
};
