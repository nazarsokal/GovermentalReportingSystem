import React, { useEffect, useMemo, useState } from 'react';
import { ResponsiveContainer, BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';
import '../styles/AnalyticsReports.css';

const STATUS_OPTIONS = ['all', 'Pending', 'In Progress', 'Resolved'];

const normalizeStatus = (status) => {
  const s = String(status ?? '').trim().toLowerCase();
  if (s === '0' || s === 'pending') return 'Pending';
  if (s === '1' || s === 'in progress' || s === 'inprogress') return 'In Progress';
  if (s === '2' || s === 'resolved') return 'Resolved';
  return 'Pending';
};

const parseAppealDate = (appeal) => {
  const raw = appeal.datePublished || appeal.DatePublished || appeal.createdAt || appeal.CreatedAt || appeal.date;
  const d = raw ? new Date(raw) : null;
  return d && !Number.isNaN(d.getTime()) ? d : null;
};

const getRangeStart = (preset) => {
  const now = new Date();
  const start = new Date(now);
  if (preset === '7d') start.setDate(now.getDate() - 7);
  if (preset === '30d') start.setDate(now.getDate() - 30);
  if (preset === '90d') start.setDate(now.getDate() - 90);
  return start;
};

function AnalyticsReports({ appeals = [], loading = false, defaultStatus = 'all', filterType, filterValue }) {
  const [rangePreset, setRangePreset] = useState('30d');
  const [statusFilter, setStatusFilter] = useState(defaultStatus);
  const [keyword, setKeyword] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  useEffect(() => {
    setStatusFilter(defaultStatus || 'all');
  }, [defaultStatus]);

  const normalizedAppeals = useMemo(() => {
    return (appeals || []).map((a) => ({
      id: a.AppealId || a.appealId || a.id,
      title: a.ProblemName || a.problemName || a.title || 'Issue',
      description: a.Description || a.description || a.issue || '',
      status: normalizeStatus(a.Status || a.status),
      date: parseAppealDate(a),
    }));
  }, [appeals]);

  const filteredAppeals = useMemo(() => {
    const now = new Date();
    return normalizedAppeals.filter((a) => {
      if (!a.date) return false;

      if (rangePreset !== 'all') {
        if (rangePreset === 'custom') {
          if (fromDate) {
            const start = new Date(fromDate);
            if (a.date < start) return false;
          }
          if (toDate) {
            const end = new Date(toDate);
            end.setHours(23, 59, 59, 999);
            if (a.date > end) return false;
          }
        } else {
          const start = getRangeStart(rangePreset);
          if (a.date < start || a.date > now) return false;
        }
      }

      if (statusFilter !== 'all' && a.status !== statusFilter) return false;

      if (keyword.trim()) {
        const q = keyword.toLowerCase();
        const hay = `${a.title} ${a.description}`.toLowerCase();
        if (!hay.includes(q)) return false;
      }

      return true;
    });
  }, [normalizedAppeals, rangePreset, fromDate, toDate, statusFilter, keyword]);

  const statusData = useMemo(() => {
    const bucket = { Pending: 0, 'In Progress': 0, Resolved: 0 };
    filteredAppeals.forEach((a) => {
      bucket[a.status] = (bucket[a.status] || 0) + 1;
    });
    return Object.entries(bucket).map(([name, count]) => ({ name, count }));
  }, [filteredAppeals]);

  const trendData = useMemo(() => {
    const grouped = {};
    filteredAppeals.forEach((a) => {
      const key = a.date.toISOString().slice(0, 10);
      grouped[key] = (grouped[key] || 0) + 1;
    });
    return Object.entries(grouped)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([date, count]) => ({ date, count }));
  }, [filteredAppeals]);

  return (
    <div className="analytics-wrapper">
      <div className="analytics-header">
        <h1>Analytics & Reports</h1>
        <p>Aggregated metrics for {filterType}: <strong>{filterValue}</strong></p>
      </div>

      <div className="analytics-filters">
        <select value={rangePreset} onChange={(e) => setRangePreset(e.target.value)}>
          <option value="7d">Last 7 days</option>
          <option value="30d">Last 30 days</option>
          <option value="90d">Last 90 days</option>
          <option value="all">All time</option>
          <option value="custom">Custom range</option>
        </select>

        {rangePreset === 'custom' && (
          <>
            <input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
            <input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          </>
        )}

        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>

        <input
          type="text"
          placeholder="Keyword filter..."
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
        />
      </div>

      {loading ? (
        <div className="analytics-loading">Loading analytics...</div>
      ) : (
        <>
          <div className="analytics-kpi-row">
            <div className="analytics-kpi"><span>Total</span><b>{filteredAppeals.length}</b></div>
            <div className="analytics-kpi"><span>Pending</span><b>{statusData.find(x => x.name === 'Pending')?.count || 0}</b></div>
            <div className="analytics-kpi"><span>In Progress</span><b>{statusData.find(x => x.name === 'In Progress')?.count || 0}</b></div>
            <div className="analytics-kpi"><span>Resolved</span><b>{statusData.find(x => x.name === 'Resolved')?.count || 0}</b></div>
          </div>

          <div className="analytics-charts">
            <div className="analytics-chart-card">
              <h3>Status Distribution</h3>
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={statusData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="count" fill="#2563eb" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>

            <div className="analytics-chart-card">
              <h3>Trend Over Time</h3>
              <ResponsiveContainer width="100%" height={280}>
                <LineChart data={trendData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="count" stroke="#10b981" strokeWidth={3} dot={{ r: 3 }} />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>

          <div className="analytics-table-card">
            <h3>Filtered Appeals</h3>
            <div className="analytics-table-wrap">
              <table className="analytics-table">
                <thead>
                  <tr><th>Title</th><th>Status</th><th>Date</th></tr>
                </thead>
                <tbody>
                  {filteredAppeals.slice(0, 50).map((a) => (
                    <tr key={a.id}>
                      <td>{a.title}</td>
                      <td>{a.status}</td>
                      <td>{a.date ? a.date.toLocaleDateString() : '-'}</td>
                    </tr>
                  ))}
                  {filteredAppeals.length === 0 && (
                    <tr><td colSpan="3">No data for selected filters.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default AnalyticsReports;

