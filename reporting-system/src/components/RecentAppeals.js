import React from 'react';
import '../styles/RecentAppeals.css';

function RecentAppeals() {
  const appeals = [
    {
      id: 1,
      title: 'Traffic signal',
      issue: 'Traffic signal malfunctioning',
      status: 'Pending',
      date: 'Jan 23, 2026'
    },
    {
      id: 2,
      title: 'Streetlight',
      issue: 'Streetlight not working for 2 weeks',
      status: 'Pending',
      date: 'Jan 22, 2026'
    },
    {
      id: 3,
      title: 'Pothole',
      issue: 'Large pothole causing traffic issues',
      status: 'In Progress',
      date: 'Jan 21, 2026'
    }
  ];

  const getStatusClass = (status) => {
    return status.toLowerCase().replace(' ', '-');
  };

  return (
    <div className="recent-appeals">
      <h2>Recent Appeals</h2>
      <div className="appeals-list">
        {appeals.map((appeal) => (
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
    </div>
  );
}

export default RecentAppeals;

