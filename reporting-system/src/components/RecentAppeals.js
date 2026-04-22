import React, { useState, useEffect } from 'react';
import '../styles/RecentAppeals.css';
import AppealService from '../services/AppealService';

function RecentAppeals({ appeals = null, loading = false }) {
  const [localAppeals, setLocalAppeals] = useState([]);
  const [localLoading, setLocalLoading] = useState(false);

  // If appeals are passed as props, use them. Otherwise fetch on mount.
  useEffect(() => {
    if (appeals !== null && Array.isArray(appeals)) {
      setLocalAppeals(appeals);
    } else if (appeals === null) {
      // Fetch only if no appeals are passed as props
      const fetchAppeals = async () => {
        setLocalLoading(true);
        const result = await AppealService.getRecentAppeals(5);
        if (result.success) {
          console.log('✅ Recent appeals loaded:', result.appeals);
          setLocalAppeals(result.appeals);
        } else {
          console.warn('Failed to load recent appeals:', result.errors);
          setLocalAppeals([]);
        }
        setLocalLoading(false);
      };

      fetchAppeals();
    }
  }, [appeals]);

  const getStatusClass = (status) => {
    return status.toLowerCase().replace(' ', '-');
  };

  const displayAppeals = appeals !== null && Array.isArray(appeals) ? appeals : localAppeals;
  const displayLoading = appeals !== null ? loading : localLoading;

  return (
    <div className="recent-appeals">
      <h2>Recent Appeals</h2>
      {displayLoading ? (
        <div style={{ padding: '20px', textAlign: 'center', color: '#999' }}>
          <p>Loading recent appeals...</p>
        </div>
      ) : displayAppeals.length === 0 ? (
        <div style={{ padding: '20px', textAlign: 'center', color: '#999' }}>
          <p>No recent appeals</p>
        </div>
      ) : (
        <div className="appeals-list">
          {displayAppeals.map((appeal) => (
            <div key={appeal.id} className="appeal-item">
              <div className="appeal-content">
                <h3>{appeal.title}</h3>
                <p className="appeal-issue">{appeal.issue}</p>
              </div>
              <div className="appeal-meta">
                <span className={`appeal-status ${getStatusClass(appeal.status)}`}>
                  {appeal.status}
                </span>
                <span className="appeal-date">{appeal.date}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default RecentAppeals;

