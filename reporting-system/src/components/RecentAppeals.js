import React, { useState, useEffect } from 'react';
import '../styles/RecentAppeals.css';
import AppealService from '../services/AppealService';

function RecentAppeals({ appeals = null, loading = false, onViewDetails }) {
  const [localAppeals, setLocalAppeals] = useState([]);
  const [localLoading, setLocalLoading] = useState(false);

  useEffect(() => {
    if (appeals !== null && Array.isArray(appeals)) {
      setLocalAppeals(appeals);
    } else if (appeals === null) {
      const fetchAppeals = async () => {
        setLocalLoading(true);
        const result = await AppealService.getRecentAppeals(5);
        if (result.success) {
          setLocalAppeals(result.appeals);
        } else {
          setLocalAppeals([]);
        }
        setLocalLoading(false);
      };
      fetchAppeals();
    }
  }, [appeals]);

  const getStatusClass = (status) => {
    return (status || 'pending').toLowerCase().replace(' ', '-');
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
                  <div
                      key={appeal.id}
                      className="appeal-item"
                      onClick={() => onViewDetails && onViewDetails(appeal.id)}
                      style={{ cursor: 'pointer' }} // Робимо картку клікабельною
                  >
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