import axios from 'axios';

const BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000';
const api  = axios.create({ baseURL: `${BASE}/api/v1` });

export const ordersApi = {
  getOrders:    (params)        => api.get('/orders', { params }),
  getOrderById: (id)            => api.get(`/orders/${id}`),
  placeOrder:   (data)          => api.post('/orders', data),
  updateStatus: (id, status)    => api.put(`/orders/${id}/status`, { status }),
  cancelOrder:  (id)            => api.delete(`/orders/${id}`),
};
