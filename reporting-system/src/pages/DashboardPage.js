import React, { useState, useEffect } from 'react';
import '../styles/DashboardPage.css';
import Navbar from '../components/Navbar';
import StatCard from '../components/StatCard';
import RecentAppeals from '../components/RecentAppeals';
import CityMap from '../components/CityMap';
import AppealService from '../services/AppealService';
import ReportProblem from '../components/ReportProblem';
import AppealDetails from '../components/AppealDetails';
import AdminDashboard from '../components/AdminDashboard';
import CouncilDashboard from '../components/CouncilDashboard'; // <-- ДОДАНО ІМПОРТ
import AnalyticsReports from '../components/AnalyticsReports';

function DashboardPage({ user, onLogout }) {
  const [currentPage, setCurrentPage] = useState('dashboard');
  const [selectedAppealId, setSelectedAppealId] = useState(null);

  const getInitialFilter = () => {
    if (user?.district) return { type: 'district', value: user.district };
    if (user?.oblast) return { type: 'oblast', value: user.oblast };
    if (user?.city) return { type: 'city', value: user.city };
    return { type: 'city', value: 'Kyiv' };
  };

  const initial = getInitialFilter();
  const [filterType, setFilterType] = useState(initial.type);
  const [filterValue, setFilterValue] = useState(initial.value);
  const [stats, setStats] = useState({ total: 0, pending: 0, inProgress: 0, resolved: 0 });
  const [recentAppeals, setRecentAppeals] = useState([]);
  const [analyticsAppeals, setAnalyticsAppeals] = useState([]);
  const [statsDrilldownStatus, setStatsDrilldownStatus] = useState('all');
  const [statsLoading, setStatsLoading] = useState(true);
  const isAdmin = user?.roles?.includes('Admin');
  const isCouncil = user?.roles?.includes('CouncilEmployee');

  const handleViewDetails = (id) => {
    setSelectedAppealId(id);
    setCurrentPage('appealDetails');
  };

  useEffect(() => {
    let newValue = 'Kyiv';
    if (filterType === 'district' && user?.district) newValue = user.district;
    else if (filterType === 'oblast' && user?.oblast) newValue = user.oblast;
    else if (filterType === 'city' && user?.city) newValue = user.city;
    setFilterValue(newValue);
  }, [filterType, user]);

  useEffect(() => {
    const fetchData = async () => {
      setStatsLoading(true);
      const result = await AppealService.getAppealSummaryByType(filterType, filterValue);
      if (result.success) {
        setStats({
          total: result.summary.total || 0,
          pending: result.summary.pending || 0,
          inProgress: result.summary.inProgress || 0,
          resolved: result.summary.resolved || 0
        });

        if (result.summary.appeals && Array.isArray(result.summary.appeals)) {
          setAnalyticsAppeals(result.summary.appeals);
          const transformed = result.summary.appeals.map(appeal => ({
            ...appeal,
            id: appeal.AppealId || appeal.appealId,
            title: appeal.ProblemName || appeal.problemName || 'Issue',
            issue: appeal.Description || appeal.description || '',
            status: appeal.Status || appeal.status || 'Pending',
            date: appeal.DatePublished ? new Date(appeal.DatePublished).toLocaleDateString() : new Date().toLocaleDateString()
          })).slice(0, 5);
          setRecentAppeals(transformed);
        } else {
          setAnalyticsAppeals([]);
          setRecentAppeals([]);
        }
      } else {
        setStats({ total: 0, pending: 0, inProgress: 0, resolved: 0 });
        setAnalyticsAppeals([]);
        setRecentAppeals([]);
      }
      setStatsLoading(false);
    };

    if (filterValue.trim()) fetchData();
  }, [filterType, filterValue]);

  return (
      <div className="dashboard-container">
        <Navbar user={user} onLogout={onLogout} currentPage={currentPage} setCurrentPage={setCurrentPage} />

        <div className="dashboard-content">

          {/* CITIZEN DASHBOARD TAB */}
          {currentPage === 'dashboard' && (
              <>
                <div className="dashboard-header">
                  <div style={{ marginBottom: '20px' }}>
                    <h1>Citizen Dashboard</h1>
                    <p>Track infrastructure problems in your city</p>
                  </div>

                  <div style={{ display: 'flex', gap: '15px', alignItems: 'center', padding: '15px', backgroundColor: '#f5f5f5', borderRadius: '8px', marginBottom: '20px', flexWrap: 'wrap' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <label style={{ fontWeight: 'bold', color: '#333' }}>View by:</label>
                      <select
                          value={filterType}
                          onChange={(e) => setFilterType(e.target.value)}
                          style={{ padding: '8px 12px', borderRadius: '4px', border: '1px solid #ddd', fontSize: '14px', cursor: 'pointer', backgroundColor: 'white' }}
                      >
                        <option value="city">City</option>
                        <option value="oblast">Oblast</option>
                        <option value="district">District</option>
                      </select>
                    </div>

                    {statsLoading && <span style={{ color: '#999', fontSize: '14px' }}>Loading...</span>}

                    <span style={{ fontSize: '13px', color: '#333', marginLeft: 'auto', backgroundColor: '#e3f2fd', padding: '8px 12px', borderRadius: '4px', fontWeight: '500' }}>
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
                    onClick={() => {
                      setStatsDrilldownStatus('all');
                      setCurrentPage('stats');
                    }}
                  />
                  <StatCard
                    icon="pending"
                    title="Pending"
                    value={statsLoading ? '-' : stats.pending}
                    color="#FFF3E0"
                    onClick={() => {
                      setStatsDrilldownStatus('Pending');
                      setCurrentPage('stats');
                    }}
                  />
                  <StatCard
                    icon="progress"
                    title="In Progress"
                    value={statsLoading ? '-' : stats.inProgress}
                    color="#FFF3E0"
                    onClick={() => {
                      setStatsDrilldownStatus('In Progress');
                      setCurrentPage('stats');
                    }}
                  />
                  <StatCard
                    icon="resolved"
                    title="Resolved"
                    value={statsLoading ? '-' : stats.resolved}
                    color="#E8F5E9"
                    onClick={() => {
                      setStatsDrilldownStatus('Resolved');
                      setCurrentPage('stats');
                    }}
                  />
                </div>

                <div className="main-content">
                  <div className="map-section">
                    <h2>City Infrastructure Map</h2>
                    <CityMap user={user} onViewDetails={handleViewDetails} />
                  </div>

                  <div className="recent-section">
                    <RecentAppeals appeals={recentAppeals} loading={statsLoading} onViewDetails={handleViewDetails} />
                  </div>
                </div>
              </>
          )}

          {/* REPORT PROBLEM TAB */}
          {currentPage === 'report' && (
              <div className="report-page">
                <ReportProblem user={user} onSuccess={() => setCurrentPage('dashboard')} />
              </div>
          )}

          {/* APPEAL DETAILS VIEW */}
          {currentPage === 'appealDetails' && selectedAppealId && (
              <div className="appeal-details-page">
                <AppealDetails
                    appealId={selectedAppealId}
                    onBack={() => {
                      setSelectedAppealId(null);
                      setCurrentPage('dashboard');
                    }}
                />
              </div>
          )}

          {/* STATS TAB */}
          {currentPage === 'stats' && (
              <AnalyticsReports
                appeals={analyticsAppeals}
                loading={statsLoading}
                defaultStatus={statsDrilldownStatus}
                filterType={filterType}
                filterValue={filterValue}
              />
          )}

          {/* ADMIN DASHBOARD TAB */}
          {currentPage === 'adminDashboard' && (
              isAdmin ? (
                <div className="admin-page-wrapper">
                  <AdminDashboard />
                </div>
              ) : (
                <div className="stats-page">
                  <h1>Access denied</h1>
                  <p>Only administrators can open this page.</p>
                </div>
              )
          )}

          {/* COUNCIL DASHBOARD TAB <-- ДОДАНО */}
          {currentPage === 'councilDashboard' && (
              isCouncil ? (
                <div className="council-page-wrapper">
                  <CouncilDashboard />
                </div>
              ) : (
                <div className="stats-page">
                  <h1>Access denied</h1>
                  <p>Only council employees can open this page.</p>
                </div>
              )
          )}

        </div>
      </div>
  );
}

export default DashboardPage;