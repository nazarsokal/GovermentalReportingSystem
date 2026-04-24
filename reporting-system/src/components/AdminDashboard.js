import React, { useState, useEffect } from 'react';
import '../styles/AdminDashboard.css';
import AdminService from '../services/AdminService';

function AdminDashboard() {
    const [activeTab, setActiveTab] = useState('users');

    // State for dynamic data
    const [users, setUsers] = useState([]);
    const [councils, setCouncils] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // Fetch data based on the active tab
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            setError(null);

            if (activeTab === 'users') {
                const result = await AdminService.getUsers();
                if (result.success) {
                    setUsers(result.users);
                } else {
                    setError(result.errors?.[0] || 'Failed to load users');
                }
            } else if (activeTab === 'councils') {
                const result = await AdminService.getCityCouncils();
                if (result.success) {
                    setCouncils(result.councils);
                } else {
                    setError(result.errors?.[0] || 'Failed to load city councils');
                }
            }

            setLoading(false);
        };

        fetchData();
    }, [activeTab]);

    return (
        <div className="admin-dashboard-container">
            <div className="admin-header">
                <h1>Admin Dashboard</h1>
                <p>Manage users, city councils, and view system analytics</p>
            </div>

            {/* Stats Cards */}
            <div className="admin-stats-grid">
                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Total Users</h3>
                        {/* Dynamically calculate total users if on the users tab */}
                        <p className="stat-value">{users.length > 0 ? users.length : '-'}</p>
                        <p className="stat-subtext">Registered accounts</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#2563eb' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
                    </div>
                </div>

                {/* Static placeholders for other stats until API endpoints are available */}
                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Total Appeals</h3>
                        <p className="stat-value">-</p>
                        <p className="stat-subtext">All time</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#d97706' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
                    </div>
                </div>

                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Resolved</h3>
                        <p className="stat-value">-</p>
                        <p className="stat-subtext positive">Rate</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#10b981' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                    </div>
                </div>

                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Efficiency</h3>
                        <p className="stat-value">-</p>
                        <p className="stat-subtext positive">This month</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#8b5cf6' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="22 7 13.5 15.5 8.5 10.5 2 17"></polyline><polyline points="16 7 22 7 22 13"></polyline></svg>
                    </div>
                </div>
            </div>

            {/* Tabs */}
            <div className="admin-tabs">
                <button
                    className={`admin-tab ${activeTab === 'users' ? 'active' : ''}`}
                    onClick={() => setActiveTab('users')}
                >
                    User Management
                </button>
                <button
                    className={`admin-tab ${activeTab === 'councils' ? 'active' : ''}`}
                    onClick={() => setActiveTab('councils')}
                >
                    City Councils Management
                </button>
                <button
                    className={`admin-tab ${activeTab === 'analytics' ? 'active' : ''}`}
                    onClick={() => setActiveTab('analytics')}
                >
                    Analytics
                </button>
            </div>

            {/* Tab Content */}
            <div className="admin-content-card">

                {/* Error State */}
                {error && (
                    <div style={{ padding: '15px', backgroundColor: '#fee2e2', color: '#991b1b', borderRadius: '6px', marginBottom: '20px' }}>
                        {error}
                    </div>
                )}

                {/* Loading State */}
                {loading && (
                    <div style={{ padding: '40px 0', textAlign: 'center', color: '#6b7280' }}>
                        Loading data...
                    </div>
                )}

                {/* USERS TAB */}
                {activeTab === 'users' && !loading && !error && (
                    <>
                        <div className="content-header">
                            <div>
                                <h2>User Management</h2>
                                <p>Manage user accounts and permissions</p>
                            </div>
                            <button className="btn-primary">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="8.5" cy="7" r="4"></circle><line x1="20" y1="8" x2="20" y2="14"></line><line x1="23" y1="11" x2="17" y2="11"></line></svg>
                                Add User
                            </button>
                        </div>

                        <div style={{ overflowX: 'auto' }}>
                            <table className="admin-table">
                                <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Email</th>
                                    <th>Role</th>
                                    <th>Status</th>
                                    <th>Created</th>
                                    <th>Actions</th>
                                </tr>
                                </thead>
                                <tbody>
                                {users.length === 0 ? (
                                    <tr><td colSpan="6" style={{ textAlign: 'center' }}>No users found.</td></tr>
                                ) : (
                                    users.map((u) => {
                                        // Defensive mapping in case backend DTO keys vary slightly
                                        const name = u.fullName || u.name || u.userName || 'N/A';
                                        const role = u.role || (u.roles && u.roles[0]) || 'Citizen';
                                        const status = u.status || (u.isActive === false ? 'inactive' : 'active');
                                        const createdDate = u.createdAt || u.created || u.dateCreated
                                            ? new Date(u.createdAt || u.created || u.dateCreated).toLocaleDateString()
                                            : 'Unknown';

                                        return (
                                            <tr key={u.id || u.userId}>
                                                <td>{name}</td>
                                                <td>{u.email}</td>
                                                <td>
                            <span className={`badge badge-role-${role.toLowerCase()}`}>
                              {role}
                            </span>
                                                </td>
                                                <td>
                            <span className={`badge badge-status-${status.toLowerCase()}`}>
                              {status}
                            </span>
                                                </td>
                                                <td>{createdDate}</td>
                                                <td>
                                                    <div className="action-btns">
                                                        <button className="action-btn" title="Edit">
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                                                        </button>
                                                        <button className="action-btn" title={status.toLowerCase() === 'active' ? 'Deactivate' : 'Activate'}>
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"></line></svg>
                                                        </button>
                                                    </div>
                                                </td>
                                            </tr>
                                        );
                                    })
                                )}
                                </tbody>
                            </table>
                        </div>
                    </>
                )}

                {/* CITY COUNCILS TAB */}
                {activeTab === 'councils' && !loading && !error && (
                    <>
                        <div className="content-header">
                            <div>
                                <h2>City Councils Management</h2>
                                <p>Manage municipal entities and jurisdictions</p>
                            </div>
                            <button className="btn-primary">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                                Add Council
                            </button>
                        </div>

                        <div style={{ overflowX: 'auto' }}>
                            <table className="admin-table">
                                <thead>
                                <tr>
                                    <th>Council Name</th>
                                    <th>City</th>
                                    <th>District</th>
                                    <th>Oblast</th>
                                    <th>Actions</th>
                                </tr>
                                </thead>
                                <tbody>
                                {councils.length === 0 ? (
                                    <tr><td colSpan="5" style={{ textAlign: 'center' }}>No city councils found.</td></tr>
                                ) : (
                                    councils.map((c) => {
                                        // Defensive mapping for councils
                                        const name = c.name || c.councilName || 'N/A';
                                        const city = c.city || c.cityName || 'N/A';
                                        const district = c.district || c.districtName || 'N/A';
                                        const oblast = c.oblast || c.region || 'N/A';

                                        return (
                                            <tr key={c.id || c.councilId}>
                                                <td style={{ fontWeight: '500' }}>{name}</td>
                                                <td>{city}</td>
                                                <td>{district}</td>
                                                <td>{oblast}</td>
                                                <td>
                                                    <div className="action-btns">
                                                        <button className="action-btn" title="Edit">
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                                                        </button>
                                                        <button className="action-btn" title="Delete" style={{ color: '#ef4444' }}>
                                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                                        </button>
                                                    </div>
                                                </td>
                                            </tr>
                                        );
                                    })
                                )}
                                </tbody>
                            </table>
                        </div>
                    </>
                )}

                {/* ANALYTICS TAB */}
                {activeTab === 'analytics' && (
                    <div style={{ padding: '40px 0', textAlign: 'center', color: '#6b7280' }}>
                        Analytics Module - Coming Soon
                    </div>
                )}
            </div>
        </div>
    );
}

export default AdminDashboard;