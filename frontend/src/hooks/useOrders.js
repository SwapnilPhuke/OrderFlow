import React, { useState, useEffect, useCallback } from 'react';
import { ordersApi } from '../services/ordersApi';

export function useOrders(params = {}) {
  const [orders,   setOrders]   = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');
  const [page,     setPage]     = useState(params.page || 1);
  const pageSize = params.pageSize || 10;

  const fetchOrders = useCallback(() => {
    setLoading(true);
    ordersApi.getOrders({ page, pageSize, ...params })
      .then(res => {
        setOrders(res.data.data?.items  || []);
        setTotal(res.data.data?.totalCount || 0);
      })
      .catch(() => setError('Could not load orders.'))
      .finally(() => setLoading(false));
  }, [page, pageSize]);

  useEffect(() => { fetchOrders(); }, [fetchOrders]);

  return { orders, total, page, setPage, pageSize, loading, error, refresh: fetchOrders };
}
