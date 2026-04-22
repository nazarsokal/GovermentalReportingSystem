import React, { useState, useEffect } from 'react';
import '../styles/DashboardPage.css';
import Navbar from '../components/Navbar';
import StatCard from '../components/StatCard';
import RecentAppeals from '../components/RecentAppeals';
import CityMap from '../components/CityMap';
import AppealService from '../services/AppealService';

function DashboardPage({ user, onLogout }) {
  const [currentPage, setCurrentPage] = useState('dashboard');

  // Determine initial filter type and value from user's address
  const getInitialFilter = () => {
    // Priority: district > oblast > city
    if (user?.district) {
      return { type: 'district', value: user.district };
    } else if (user?.oblast) {
      return { type: 'oblast', value: user.oblast };
    } else if (user?.city) {
      return { type: 'city', value: user.city };
    } else if (user?.address) {
      // Parse address string if available
      // Assuming address format might be "City, Oblast, District"
      return { type: 'city', value: 'Kyiv' }; // Default fallback
    }
    return { type: 'city', value: 'Kyiv' }; // Ultimate fallback
  };

  const initial = getInitialFilter();
  const [filterType, setFilterType] = useState(initial.type);
  const [filterValue, setFilterValue] = useState(initial.value);
  const [stats, setStats] = useState({
    total: 0,
    pending: 0,
    inProgress: 0,
    resolved: 0
  });
  const [recentAppeals, setRecentAppeals] = useState([]);
  const [statsLoading, setStatsLoading] = useState(true);

  // Update filterValue when filterType changes to match user's address
  useEffect(() => {
    let newValue = 'Kyiv'; // default fallback

    if (filterType === 'district' && user?.district) {
      newValue = user.district;
    } else if (filterType === 'oblast' && user?.oblast) {
      newValue = user.oblast;
    } else if (filterType === 'city' && user?.city) {
      newValue = user.city;
    }

    console.log(`📍 Filter type changed to "${filterType}", updating value to:`, newValue);
    setFilterValue(newValue);
  }, [filterType, user]);

  // Fetch statistics and appeals when filter changes
  useEffect(() => {
    const fetchData = async () => {
      setStatsLoading(true);

      const result = await AppealService.getAppealSummaryByType(filterType, filterValue);
      if (result.success) {
        console.log('✅ Appeal summary loaded:', result.summary);
        setStats({
          total: result.summary.total || 0,
          pending: result.summary.pending || 0,
          inProgress: result.summary.inProgress || 0,
          resolved: result.summary.resolved || 0
        });

        // Transform appeals for recent appeals display
        if (result.summary.appeals && Array.isArray(result.summary.appeals)) {
          const transformed = result.summary.appeals.map(appeal => ({
            ...appeal,
            id: appeal.AppealId || appeal.appealId,
            title: appeal.ProblemName || appeal.problemName || 'Issue',
            issue: appeal.Description || appeal.description || '',
            status: appeal.Status || appeal.status || 'Pending',
            date: appeal.DatePublished ? new Date(appeal.DatePublished).toLocaleDateString('en-US', {
              year: 'numeric',
              month: 'short',
              day: 'numeric'
            }) : new Date().toLocaleDateString()
          })).slice(0, 5); // Limit to 5 recent appeals
          setRecentAppeals(transformed);
        } else {
          setRecentAppeals([]);
        }
      } else {
        console.warn('Failed to load summary:', result.errors);
        setStats({ total: 0, pending: 0, inProgress: 0, resolved: 0 });
        setRecentAppeals([]);
      }
      setStatsLoading(false);
    };

    if (filterValue.trim()) {
      fetchData();
    }
  }, [filterType, filterValue]);

  return (
    <div className="dashboard-container">
      <Navbar user={user} onLogout={onLogout} currentPage={currentPage} setCurrentPage={setCurrentPage} />

      <div className="dashboard-content">
        {currentPage === 'dashboard' && (
          <>
            <div className="dashboard-header">
              <div style={{ marginBottom: '20px' }}>
                <h1>Citizen Dashboard</h1>
                <p>Track infrastructure problems in your city</p>
              </div>

              {/* Filter Controls */}
              <div style={{
                display: 'flex',
                gap: '15px',
                alignItems: 'center',
                padding: '15px',
                backgroundColor: '#f5f5f5',
                borderRadius: '8px',
                marginBottom: '20px',
                flexWrap: 'wrap'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                  <label style={{ fontWeight: 'bold', color: '#333' }}>View by:</label>
                  <select
                    value={filterType}
                    onChange={(e) => setFilterType(e.target.value)}
                    style={{
                      padding: '8px 12px',
                      borderRadius: '4px',
                      border: '1px solid #ddd',
                      fontSize: '14px',
                      cursor: 'pointer',
                      backgroundColor: 'white'
                    }}
                  >
                    <option value="city">City</option>
                    <option value="oblast">Oblast</option>
                    <option value="district">District</option>
                  </select>
                </div>

                {statsLoading && <span style={{ color: '#999', fontSize: '14px' }}>Loading...</span>}
                
                {/* Show which address is being used */}
                <span style={{
                  fontSize: '13px',
                  color: '#333',
                  marginLeft: 'auto',
                  backgroundColor: '#e3f2fd',
                  padding: '8px 12px',
                  borderRadius: '4px',
                  fontWeight: '500'
                }}>
                  📍 <strong>{filterValue}</strong>
                </span>
              </div>
            </div>

            <div className="stats-section">
              <StatCard
                icon="total"
                title="Total Appeals"
                value={statsLoading ? '-' : stats.total}
                color="#E3F2FD"
              />
              <StatCard
                icon="pending"
                title="Pending"
                value={statsLoading ? '-' : stats.pending}
                color="#FFF3E0"
              />
              <StatCard
                icon="progress"
                title="In Progress"
                value={statsLoading ? '-' : stats.inProgress}
                color="#FFF3E0"
              />
              <StatCard
                icon="resolved"
                title="Resolved"
                value={statsLoading ? '-' : stats.resolved}
                color="#E8F5E9"
              />
            </div>

            <div className="main-content">
              <div className="map-section">
                <h2>City Infrastructure Map</h2>
                <CityMap user={user} />
              </div>

               <div className="recent-section">
                 <RecentAppeals appeals={recentAppeals} loading={statsLoading} />
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

