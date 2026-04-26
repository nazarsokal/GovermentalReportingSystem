import React from 'react';
import '../styles/StatCard.css';

function StatCard({ icon, title, value, color, onClick }) {
  const getIcon = () => {
    switch (icon) {
      case 'total':
        return (
          <svg viewBox="0 0 24 24" className="stat-icon">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#1976D2"/>
          </svg>
        );
      case 'pending':
        return (
          <svg viewBox="0 0 24 24" className="stat-icon">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z" fill="#FF9800"/>
          </svg>
        );
      case 'progress':
        return (
          <svg viewBox="0 0 24 24" className="stat-icon">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z" fill="#FF9800"/>
          </svg>
        );
      case 'resolved':
        return (
          <svg viewBox="0 0 24 24" className="stat-icon">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#4CAF50"/>
          </svg>
        );
      default:
        return null;
    }
  };

  return (
    <button
      type="button"
      className={`stat-card ${onClick ? 'stat-card-clickable' : ''}`}
      style={{ backgroundColor: color }}
      onClick={onClick}
      disabled={!onClick}
    >
      <div className="stat-icon-container">
        {getIcon()}
      </div>
      <div className="stat-content">
        <h3>{title}</h3>
        <p className="stat-value">{value}</p>
      </div>
    </button>
  );
}

export default StatCard;

