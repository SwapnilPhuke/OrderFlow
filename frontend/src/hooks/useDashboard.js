import React, { useState, useEffect, useCallback } from 'react';
import axios from 'axios';

const BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000';

export function useDashboard() {
  const [stats,   setStats]   = useState(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState('');

  const fetchStats = useCallback(() => {
    setLoading(true);
    axios.get(`${BASE}/api/v1/admin/stats`)
      .then(res => setStats(res.data.data))
      .catch(() => setError('Could not load stats.'))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { fetchStats(); }, [fetchStats]);
  return { stats, loading, error, refresh: fetchStats };
}
