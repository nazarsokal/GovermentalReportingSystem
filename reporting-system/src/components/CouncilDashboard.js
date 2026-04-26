import React, { useState, useEffect } from 'react';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import '../styles/CouncilDashboard.css';
import CouncilService from '../services/CouncilService';

const COLORS = ['#f59e0b', '#3b82f6', '#10b981', '#ef4444'];

function CouncilDashboard() {
    // 1. БЕЗПЕЧНИЙ ПАРСИНГ: Запобігаємо крашу, якщо в localStorage лежить "undefined"
    let user = {};
    try {
        const userStr = localStorage.getItem('user');
        if (userStr && userStr !== 'undefined') {
            user = JSON.parse(userStr);
        }
    } catch (e) {
        console.error("Помилка парсингу користувача з localStorage:", e);
    }

    const isHead = user?.isCouncilHead === true || user?.councilPosition === 'Head';
    const councilId = user?.councilId || localStorage.getItem('councilId');
    const localCouncilName = user?.councilName || localStorage.getItem('councilName');

    const [activeTab, setActiveTab] = useState('appeals');

    // Стан
    const [appeals, setAppeals] = useState([]);
    const [myAppeals, setMyAppeals] = useState([]);
    const [summary, setSummary] = useState({ total: 0, pending: 0, resolved: 0, avgResolution: 0 });
    const [distributionData, setDistributionData] = useState([]);
    const [trendData, setTrendData] = useState([]);
    const [councilName, setCouncilName] = useState(localCouncilName || '');
    const [employees, setEmployees] = useState([]);
    const [selectedEmployeeByAppeal, setSelectedEmployeeByAppeal] = useState({});
    const [loading, setLoading] = useState(true);

    // Стан модалки
    const [showRegisterModal, setShowRegisterModal] = useState(false);
    const [employeeForm, setEmployeeForm] = useState({ fullName: '', email: '', password: '', position: '', role: 'CouncilEmployee', councilId: councilId });
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        console.log("CouncilDashboard змонтовано! Council ID:", councilId);
        if (councilId) {
            fetchDashboardData();
        } else {
            console.warn("Council ID відсутній, зупиняємо завантаження.");
            setLoading(false);
        }
    }, [councilId]);

    const fetchDashboardData = async () => {
        setLoading(true);
        try {
            const appealsResult = await CouncilService.getCouncilAppeals(councilId);
            if (appealsResult.success) setAppeals(appealsResult.data || []);

            if (!isHead) {
                const myResult = await CouncilService.getMyAppeals();
                if (myResult.success) setMyAppeals(myResult.data || []);
            }

            const statsResult = await CouncilService.getCouncilStats();
            if (statsResult.success && statsResult.data) {
                setSummary({
                    total: statsResult.data.totalAppeals || (appealsResult.data?.length || 0),
                    pending: statsResult.data.pendingAppeals || 0,
                    resolved: statsResult.data.resolvedAppeals || 0,
                    avgResolution: statsResult.data.averageResolutionDays || 0
                });
            }

            const efficiencyResult = await CouncilService.getCouncilEfficiency();
            const nameFromApi = efficiencyResult?.data?.councilName || efficiencyResult?.data?.CouncilName;
            if (efficiencyResult.success && nameFromApi) {
                setCouncilName(nameFromApi);
                localStorage.setItem('councilName', nameFromApi);
            }

            if (isHead) {
                const employeesResult = await CouncilService.getCouncilEmployees();
                if (employeesResult.success) setEmployees(employeesResult.data || []);

                const distResult = await CouncilService.getStatusDistribution(councilId);
                if (distResult.success && distResult.data) {
                    const formattedDist = Object.keys(distResult.data).map(key => ({
                        name: key,
                        value: distResult.data[key]
                    }));
                    setDistributionData(formattedDist);
                }

                const trendResult = await CouncilService.getTrends();
                if (trendResult.success && trendResult.data) setTrendData(trendResult.data);
            }
        } catch (error) {
            console.error("Помилка завантаження даних дашборду:", error);
        }
        setLoading(false);
    };

    const handleAssignEmployee = async (appealId) => {
        const employeeId = selectedEmployeeByAppeal[appealId];
        if (!employeeId) {
            alert('Оберіть працівника для призначення.');
            return;
        }

        const result = await CouncilService.assignAppeal(appealId, employeeId);
        if (!result.success) {
            alert(result.errors?.[0] || 'Не вдалося призначити працівника.');
            return;
        }

        await fetchDashboardData();
    };

    const handleStatusChange = async (appealId, status) => {
        const result = await CouncilService.updateAppealStatus(appealId, status);
        if (!result.success) {
            alert(result.errors?.[0] || 'Не вдалося змінити статус.');
            return;
        }

        await fetchDashboardData();
    };

    const handleRegisterEmployee = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        const result = await CouncilService.registerEmployee(employeeForm);
        if (result.success) {
            setShowRegisterModal(false);
            setEmployeeForm({ fullName: '', email: '', password: '', position: '', role: 'CouncilEmployee', councilId: councilId });
            alert('Працівника успішно зареєстровано!');
        } else {
            alert(result.errors?.[0] || 'Помилка реєстрації працівника.');
        }
        setIsSubmitting(false);
    };

    // 2. БЕЗПЕЧНА ФІЛЬТРАЦІЯ: Запобігаємо крашу, якщо appeals === null/undefined
    const safeAppeals = appeals || [];
    const pending = safeAppeals.filter(a => a.status === 'Pending' || a.status === 0);
    const inProgress = safeAppeals.filter(a => a.status === 'In Progress' || a.status === 'InProgress' || a.status === 1);
    const resolved = safeAppeals.filter(a => a.status === 'Resolved' || a.status === 2);

    const safeMyAppeals = myAppeals || [];

    // 3. Якщо немає ID ради, показуємо чітке повідомлення
    if (!councilId) {
        return (
            <div className="council-dashboard-wrapper" style={{ minHeight: '80vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <div style={{ padding: '40px', textAlign: 'center', background: 'white', borderRadius: '12px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)' }}>
                    <h2 style={{ color: '#ef4444' }}>Доступ обмежено</h2>
                    <p style={{ color: '#6b7280' }}>Ваш обліковий запис не прив'язаний до жодної міської ради.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="council-dashboard-wrapper">
            <div className="council-header-container">
                <div>
                    <h1 className="council-title">{councilName || 'Міська рада'}</h1>
                </div>
                {isHead && (
                    <button className="btn-add-employee" onClick={() => setShowRegisterModal(true)}>
                        + Додати працівника
                    </button>
                )}
            </div>

            <div className="stats-cards-grid">
                <div className="stat-card">
                    <div className="stat-card-header"><span className="stat-title">Всього звернень</span><span className="stat-icon blue">📋</span></div>
                    <div className="stat-value">{summary.total}</div>
                    <div className="stat-subtext">За весь час</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-header"><span className="stat-title">В очікуванні</span><span className="stat-icon red">🕒</span></div>
                    <div className="stat-value">{summary.pending || pending.length}</div>
                    <div className="stat-subtext red-text">Потребують уваги</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-header"><span className="stat-title">Вирішено</span><span className="stat-icon green">✅</span></div>
                    <div className="stat-value">{summary.resolved || resolved.length}</div>
                    <div className="stat-subtext green-text">Успішно закрито</div>
                </div>
                <div className="stat-card">
                    <div className="stat-card-header"><span className="stat-title">Сер. час вирішення</span><span className="stat-icon purple">📈</span></div>
                    <div className="stat-value">{Number(summary.avgResolution || 0).toFixed(2)}</div>
                    <div className="stat-subtext green-text">Днів</div>
                </div>
            </div>

            <div className="council-tabs">
                <button className={`council-tab ${activeTab === 'appeals' ? 'active' : ''}`} onClick={() => setActiveTab('appeals')}>Звернення</button>
                {!isHead && (
                    <button className={`council-tab ${activeTab === 'myTasks' ? 'active' : ''}`} onClick={() => setActiveTab('myTasks')}>Мої задачі</button>
                )}
                <button className={`council-tab ${activeTab === 'polls' ? 'active' : ''}`} onClick={() => setActiveTab('polls')}>Опитування</button>
                {isHead && (
                    <button className={`council-tab ${activeTab === 'efficiency' ? 'active' : ''}`} onClick={() => setActiveTab('efficiency')}>Аналітика відділу</button>
                )}
            </div>

            {loading ? (
                <div style={{ textAlign: 'center', padding: '60px', color: '#6b7280', fontSize: '18px' }}>Завантаження даних...</div>
            ) : (
                <>
                    {/* TAB: Appeals */}
                    {activeTab === 'appeals' && (
                        <div className="kanban-section">
                            <div className="kanban-header">
                                <h2>Канбан дошка звернень</h2>
                                <p>Керуйте статусами звернень громадян</p>
                            </div>

                            <div className="kanban-board">
                                <div className="kanban-column">
                                    <div className="column-header"><h3>Нові (Pending)</h3><span className="badge badge-pending">{pending.length}</span></div>
                                    <div className="column-body">
                                        {pending.map(appeal => (
                                            <div key={appeal.id} className="kanban-card">
                                                <div className="card-top">
                                                    <h4>{appeal.title || appeal.category || 'Звернення'}</h4>
                                                    <span className="card-date">{new Date(appeal.createdAt).toLocaleDateString()}</span>
                                                </div>
                                                {appeal.assignedEmployeeName && (
                                                    <div style={{ fontSize: '12px', color: '#6b7280', marginBottom: '6px' }}>
                                                        Виконавець: <b>{appeal.assignedEmployeeName}</b>
                                                    </div>
                                                )}
                                                <p className="card-address">{appeal.address}</p>
                                                <p className="card-desc">{appeal.description}</p>

                                                {isHead && (
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
                                                        <select
                                                            value={selectedEmployeeByAppeal[appeal.appealId] || ''}
                                                            onChange={(e) => setSelectedEmployeeByAppeal(prev => ({ ...prev, [appeal.appealId]: e.target.value }))}
                                                            style={{ padding: '8px', borderRadius: '8px', border: '1px solid #e5e7eb' }}
                                                        >
                                                            <option value="">Призначити працівника…</option>
                                                            {employees.map(emp => (
                                                                <option key={emp.employeeId} value={emp.employeeId}>
                                                                    {(emp.fullName || emp.email || 'Працівник')} {emp.position ? `(${emp.position})` : ''}
                                                                </option>
                                                            ))}
                                                        </select>
                                                        <button className="btn-primary" type="button" onClick={() => handleAssignEmployee(appeal.appealId)} style={{ padding: '8px 12px' }}>
                                                            Призначити
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'In Progress')} style={{ padding: '8px 12px' }}>
                                                            В роботу
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Resolved')} style={{ padding: '8px 12px' }}>
                                                            Вирішено
                                                        </button>
                                                    </div>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                </div>

                                <div className="kanban-column">
                                    <div className="column-header"><h3>В роботі (In Progress)</h3><span className="badge badge-progress">{inProgress.length}</span></div>
                                    <div className="column-body">
                                        {inProgress.map(appeal => (
                                            <div key={appeal.id} className="kanban-card">
                                                <div className="card-top">
                                                    <h4>{appeal.title || appeal.category || 'Звернення'}</h4>
                                                    <span className="card-date">{new Date(appeal.createdAt).toLocaleDateString()}</span>
                                                </div>
                                                {appeal.assignedEmployeeName && (
                                                    <div style={{ fontSize: '12px', color: '#6b7280', marginBottom: '6px' }}>
                                                        Виконавець: <b>{appeal.assignedEmployeeName}</b>
                                                    </div>
                                                )}
                                                <p className="card-address">{appeal.address}</p>
                                                <p className="card-desc">{appeal.description}</p>

                                                {isHead && (
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
                                                        <select
                                                            value={selectedEmployeeByAppeal[appeal.appealId] || ''}
                                                            onChange={(e) => setSelectedEmployeeByAppeal(prev => ({ ...prev, [appeal.appealId]: e.target.value }))}
                                                            style={{ padding: '8px', borderRadius: '8px', border: '1px solid #e5e7eb' }}
                                                        >
                                                            <option value="">Перепризначити…</option>
                                                            {employees.map(emp => (
                                                                <option key={emp.employeeId} value={emp.employeeId}>
                                                                    {(emp.fullName || emp.email || 'Працівник')} {emp.position ? `(${emp.position})` : ''}
                                                                </option>
                                                            ))}
                                                        </select>
                                                        <button className="btn-primary" type="button" onClick={() => handleAssignEmployee(appeal.appealId)} style={{ padding: '8px 12px' }}>
                                                            Зберегти
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Pending')} style={{ padding: '8px 12px' }}>
                                                            В Pending
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Resolved')} style={{ padding: '8px 12px' }}>
                                                            Вирішено
                                                        </button>
                                                    </div>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                </div>

                                <div className="kanban-column">
                                    <div className="column-header"><h3>Вирішено (Resolved)</h3><span className="badge badge-resolved">{resolved.length}</span></div>
                                    <div className="column-body">
                                        {resolved.map(appeal => (
                                            <div key={appeal.id} className="kanban-card">
                                                <div className="card-top">
                                                    <h4>{appeal.title || appeal.category || 'Звернення'}</h4>
                                                    <span className="card-date">{new Date(appeal.resolvedAt || appeal.updatedAt).toLocaleDateString()}</span>
                                                </div>
                                                {appeal.assignedEmployeeName && (
                                                    <div style={{ fontSize: '12px', color: '#6b7280', marginBottom: '6px' }}>
                                                        Виконавець: <b>{appeal.assignedEmployeeName}</b>
                                                    </div>
                                                )}
                                                <p className="card-address">{appeal.address}</p>
                                                <p className="card-desc">{appeal.description}</p>

                                                {isHead && (
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'In Progress')} style={{ padding: '8px 12px' }}>
                                                            Повернути в роботу
                                                        </button>
                                                    </div>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* TAB: My Tasks (Employees) */}
                    {activeTab === 'myTasks' && !isHead && (
                        <div className="kanban-section">
                            <div className="kanban-header">
                                <h2>Мої задачі</h2>
                                <p>Звернення, які призначені саме вам</p>
                            </div>

                            {safeMyAppeals.length === 0 ? (
                                <div style={{ textAlign: 'center', padding: '60px', color: '#6b7280', fontSize: '16px' }}>
                                    Наразі немає призначених задач.
                                </div>
                            ) : (
                                <div className="kanban-board">
                                    <div className="kanban-column">
                                        <div className="column-header"><h3>Pending</h3><span className="badge badge-pending">{safeMyAppeals.filter(a => a.status === 'Pending' || a.status === 0).length}</span></div>
                                        <div className="column-body">
                                            {safeMyAppeals.filter(a => a.status === 'Pending' || a.status === 0).map(appeal => (
                                                <div key={appeal.id} className="kanban-card">
                                                    <div className="card-top">
                                                        <h4>{appeal.title || 'Звернення'}</h4>
                                                        <span className="card-date">{new Date(appeal.createdAt).toLocaleDateString()}</span>
                                                    </div>
                                                    <p className="card-address">{appeal.address}</p>
                                                    <p className="card-desc">{appeal.description}</p>
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'In Progress')} style={{ padding: '8px 12px' }}>
                                                            В роботу
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Resolved')} style={{ padding: '8px 12px' }}>
                                                            Вирішено
                                                        </button>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>

                                    <div className="kanban-column">
                                        <div className="column-header"><h3>In Progress</h3><span className="badge badge-progress">{safeMyAppeals.filter(a => a.status === 'In Progress' || a.status === 'InProgress' || a.status === 1).length}</span></div>
                                        <div className="column-body">
                                            {safeMyAppeals.filter(a => a.status === 'In Progress' || a.status === 'InProgress' || a.status === 1).map(appeal => (
                                                <div key={appeal.id} className="kanban-card">
                                                    <div className="card-top">
                                                        <h4>{appeal.title || 'Звернення'}</h4>
                                                        <span className="card-date">{new Date(appeal.createdAt).toLocaleDateString()}</span>
                                                    </div>
                                                    <p className="card-address">{appeal.address}</p>
                                                    <p className="card-desc">{appeal.description}</p>
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Pending')} style={{ padding: '8px 12px' }}>
                                                            В Pending
                                                        </button>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'Resolved')} style={{ padding: '8px 12px' }}>
                                                            Вирішено
                                                        </button>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>

                                    <div className="kanban-column">
                                        <div className="column-header"><h3>Resolved</h3><span className="badge badge-resolved">{safeMyAppeals.filter(a => a.status === 'Resolved' || a.status === 2).length}</span></div>
                                        <div className="column-body">
                                            {safeMyAppeals.filter(a => a.status === 'Resolved' || a.status === 2).map(appeal => (
                                                <div key={appeal.id} className="kanban-card">
                                                    <div className="card-top">
                                                        <h4>{appeal.title || 'Звернення'}</h4>
                                                        <span className="card-date">{new Date(appeal.updatedAt).toLocaleDateString()}</span>
                                                    </div>
                                                    <p className="card-address">{appeal.address}</p>
                                                    <p className="card-desc">{appeal.description}</p>
                                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                                        <button className="btn-secondary" type="button" onClick={() => handleStatusChange(appeal.appealId, 'In Progress')} style={{ padding: '8px 12px' }}>
                                                            Повернути в роботу
                                                        </button>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>
                    )}

                    {/* TAB: Polls */}
                    {activeTab === 'polls' && (
                        <div className="kanban-section" style={{ padding: '40px', textAlign: 'center', color: '#6b7280' }}>
                            <h2>Створення опитувань</h2>
                            <p>Модуль знаходиться в розробці.</p>
                        </div>
                    )}

                    {/* TAB: Efficiency Reports (ТІЛЬКИ ДЛЯ HEAD) */}
                    {activeTab === 'efficiency' && isHead && (
                        <div className="kanban-section">
                            <div className="kanban-header">
                                <h2>Ефективність роботи</h2>
                                <p>Візуальна аналітика обробки звернень</p>
                            </div>
                            <div className="charts-grid">
                                <div className="chart-container">
                                    <h3>Тенденція вирішення</h3>
                                    <ResponsiveContainer width="100%" height={300}>
                                        <LineChart data={trendData.length > 0 ? trendData : [{ date: 'Немає даних', count: 0 }]}>
                                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
                                            <XAxis dataKey="date" axisLine={false} tickLine={false} />
                                            <YAxis axisLine={false} tickLine={false} />
                                            <Tooltip contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px rgba(0,0,0,0.1)' }} />
                                            <Legend />
                                            <Line type="monotone" dataKey="count" name="Оброблено" stroke="#2563eb" strokeWidth={3} dot={{ r: 4 }} activeDot={{ r: 6 }} />
                                        </LineChart>
                                    </ResponsiveContainer>
                                </div>
                                <div className="chart-container">
                                    <h3>Розподіл статусів</h3>
                                    <ResponsiveContainer width="100%" height={300}>
                                        <PieChart>
                                            <Pie data={distributionData.length > 0 ? distributionData : [{ name: 'Порожньо', value: 1 }]} cx="50%" cy="50%" innerRadius={60} outerRadius={100} paddingAngle={5} dataKey="value">
                                                {(distributionData.length > 0 ? distributionData : [{}]).map((entry, index) => (
                                                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                                ))}
                                            </Pie>
                                            <Tooltip contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px rgba(0,0,0,0.1)' }} />
                                            <Legend />
                                        </PieChart>
                                    </ResponsiveContainer>
                                </div>
                            </div>
                        </div>
                    )}
                </>
            )}

            {/* МОДАЛКА */}
            {showRegisterModal && (
                <div className="modal-overlay" onClick={() => setShowRegisterModal(false)}>
                    <div className="modal-content" onClick={e => e.stopPropagation()}>
                        <h2>Реєстрація працівника</h2>
                        <form onSubmit={handleRegisterEmployee}>
                            <div className="form-group"><label>ПІБ</label><input type="text" required value={employeeForm.fullName} onChange={e => setEmployeeForm({...employeeForm, fullName: e.target.value})} /></div>
                            <div className="form-group"><label>Службовий Email</label><input type="email" required value={employeeForm.email} onChange={e => setEmployeeForm({...employeeForm, email: e.target.value})} /></div>
                            <div className="form-group"><label>Посада</label><input type="text" required value={employeeForm.position} onChange={e => setEmployeeForm({...employeeForm, position: e.target.value})} /></div>
                            <div className="form-group"><label>Тимчасовий пароль</label><input type="password" required value={employeeForm.password} onChange={e => setEmployeeForm({...employeeForm, password: e.target.value})} /></div>
                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setShowRegisterModal(false)}>Скасувати</button>
                                <button type="submit" className="btn-primary" disabled={isSubmitting}>{isSubmitting ? 'Збереження...' : 'Зареєструвати'}</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default CouncilDashboard;