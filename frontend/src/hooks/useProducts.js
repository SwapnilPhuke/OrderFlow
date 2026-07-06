import { useState, useEffect, useCallback } from 'react';
import { productsApi } from '../services/productsApi';

export function useProducts(initialParams = {}) {
  const [products, setProducts] = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');
  const [params,   setParams]   = useState({ page: 1, pageSize: 12, ...initialParams });

  const fetchProducts = useCallback(() => {
    setLoading(true);
    productsApi.getProducts(params)
      .then(res => {
        setProducts(res.data.data?.items      || []);
        setTotal(res.data.data?.totalCount    || 0);
      })
      .catch(() => setError('Could not load products.'))
      .finally(() => setLoading(false));
  }, [params]);

  useEffect(() => { fetchProducts(); }, [fetchProducts]);

  return { products, total, params, setParams, loading, error };
}
