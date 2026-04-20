import React, { useState } from 'react';
import '../styles/DashboardPage.css';
import Navbar from '../components/Navbar';
import StatCard from '../components/StatCard';
import RecentAppeals from '../components/RecentAppeals';
import CityMap from '../components/CityMap';

function DashboardPage({ user, onLogout }) {
  const [currentPage, setCurrentPage] = useState('dashboard');

  return (
    <div className="dashboard-container">
      <Navbar user={user} onLogout={onLogout} currentPage={currentPage} setCurrentPage={setCurrentPage} />

      <div className="dashboard-content">
        {currentPage === 'dashboard' && (
          <>
            <div className="dashboard-header">
              <h1>Citizen Dashboard</h1>
              <p>Track infrastructure problems in your city</p>
            </div>

            <div className="stats-section">
              <StatCard
                icon="total"
                title="Total Appeals"
                value={5}
                color="#E3F2FD"
              />
              <StatCard
                icon="pending"
                title="Pending"
                value={2}
                color="#FFF3E0"
              />
              <StatCard
                icon="progress"
                title="In Progress"
                value={2}
                color="#FFF3E0"
              />
              <StatCard
                icon="resolved"
                title="Resolved"
                value={1}
                color="#E8F5E9"
              />
            </div>

            <div className="main-content">
              <div className="map-section">
                <h2>City Infrastructure Map</h2>
                <CityMap />
              </div>

              <div className="recent-section">
                <RecentAppeals />
              </div>
            </div>
          </>
        )}

        {currentPage === 'report' && (
          <div className="report-page">
            <h1>Report Problem</h1>
            <div className="report-form">
              <p>Report new infrastructure problems here.</p>
              <button className="report-button">Report a Problem</button>
            </div>
          </div>
        )}

        {currentPage === 'stats' && (
          <div className="stats-page">
            <h1>Statistics</h1>
            <p>View detailed statistics about city infrastructure.</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default DashboardPage;

