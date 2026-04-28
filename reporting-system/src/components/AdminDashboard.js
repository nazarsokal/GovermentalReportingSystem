import React, { useState, useEffect } from 'react';
import { Autocomplete, useJsApiLoader, GoogleMap, Marker } from '@react-google-maps/api';
import '../styles/AdminDashboard.css';
import AdminService from '../services/AdminService';
import { googleMapsLoaderOptions } from '../constants/googleMaps';

const mapContainerStyle = {
    width: '100%',
    height: '250px',
    borderRadius: '8px',
    marginTop: '10px'
};

// Початковий центр
const defaultCenter = {
    lat: 50.4501,
    lng: 30.5234
};

const getCouncilIdentifier = (council) => council?.councilId || council?.id || council?.Id;

function AdminDashboard() {
    const [activeTab, setActiveTab] = useState('users');

    // Дані з бекенду
    const [users, setUsers] = useState([]);
    const [councils, setCouncils] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // Стан модальних вікон
    const [showCouncilModal, setShowCouncilModal] = useState(false);
    const [showUserModal, setShowUserModal] = useState(false);
    const [showCSVModal, setShowCSVModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formError, setFormError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    // Поточний редагуємий об'єкт
    const [selectedUser, setSelectedUser] = useState(null);
    const [selectedCouncil, setSelectedCouncil] = useState(null);

    // Пошук голови ради (Head User)
    const [emailSearch, setEmailSearch] = useState('');
    const [suggestions, setSuggestions] = useState([]);

    // Стан Google Maps
    const [autocomplete, setAutocomplete] = useState(null);
    const [mapCenter, setMapCenter] = useState(defaultCenter);
    const [markerPos, setMarkerPos] = useState(null);

    const { isLoaded } = useJsApiLoader({
        ...googleMapsLoaderOptions
    });

    // Стан форми для ради
    const [councilFormData, setCouncilFormData] = useState({
        name: '', contactEmail: '', headUserId: '',
        latitude: 0, longitude: 0
    });

    // Стан форми для користувача
    const [userFormData, setUserFormData] = useState({
        fullName: '', email: '', password: '', role: 'Citizen', status: 'Active'
    });

    // CSV файл для завантаження
    const [csvFile, setCsvFile] = useState(null);

    const fetchData = async () => {
        setLoading(true);
        setError(null);

        const userResult = await AdminService.getUsers();
        if (userResult.success) setUsers(userResult.users);

        if (activeTab === 'councils') {
            const councilResult = await AdminService.getCityCouncils();
            if (councilResult.success) {
                setCouncils(councilResult.councils);
            } else {
                setError(councilResult.errors?.[0] || 'Помилка завантаження списку рад');
            }
        }
        setLoading(false);
    };

    useEffect(() => {
        fetchData();
    }, [activeTab]);

    // Пошук користувача за email
    const handleEmailChange = (e) => {
        const value = e.target.value;
        setEmailSearch(value);

        if (councilFormData.headUserId) {
            setCouncilFormData(prev => ({ ...prev, headUserId: '' }));
        }

        if (value.trim().length > 0) {
            const filtered = users.filter(u =>
                u.email && u.email.toLowerCase().includes(value.toLowerCase())
            );
            setSuggestions(filtered);
        } else {
            setSuggestions([]);
        }
    };

    const selectUser = (user) => {
        setEmailSearch(user.email);
        setCouncilFormData(prev => ({ ...prev, headUserId: user.id || user.userId || user.Id }));
        setSuggestions([]);
    };

    // Робота з Google Maps Autocomplete
    const onLoadAutocomplete = (instance) => setAutocomplete(instance);

    const onPlaceChanged = () => {
        if (autocomplete !== null) {
            const place = autocomplete.getPlace();
            if (place.geometry && place.geometry.location) {
                const lat = place.geometry.location.lat();
                const lng = place.geometry.location.lng();

                setMapCenter({ lat, lng });
                setMarkerPos({ lat, lng });

                setCouncilFormData(prev => ({
                    ...prev,
                    latitude: lat,
                    longitude: lng
                }));
            }
        }
    };

    const onMapClick = (e) => {
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();
        setMarkerPos({ lat, lng });
        setCouncilFormData(prev => ({ ...prev, latitude: lat, longitude: lng }));
    };

    const handleCouncilInputChange = (e) => {
        const { name, value } = e.target;
        setCouncilFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleUserInputChange = (e) => {
        const { name, value } = e.target;
        setUserFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleCouncilSubmit = async (e) => {
        e.preventDefault();
        setFormError('');

        if (!councilFormData.headUserId) {
            setFormError('Будь ласка, оберіть дійсного користувача зі списку підказок email.');
            return;
        }

        if (!councilFormData.latitude || !councilFormData.longitude) {
            setFormError('Будь ласка, оберіть адресу на карті або через пошук.');
            return;
        }

        setIsSubmitting(true);

        const payload = {
            name: councilFormData.name,
            contactEmail: councilFormData.contactEmail,
            headUserId: councilFormData.headUserId,
            latitude: parseFloat(councilFormData.latitude),
            longitude: parseFloat(councilFormData.longitude)
        };

        let result;
        if (selectedCouncil) {
            const councilIdentifier = getCouncilIdentifier(selectedCouncil);
            result = await AdminService.updateCityCouncil(councilIdentifier, {
                ...payload,
                councilId: councilIdentifier,
                addressId: selectedCouncil.addressId || null
            });
        } else {
            result = await AdminService.createCityCouncil(payload);
        }

        if (result.success) {
            setShowCouncilModal(false);
            setCouncilFormData({
                name: '', contactEmail: '', headUserId: '',
                latitude: 0, longitude: 0
            });
            setEmailSearch('');
            setMarkerPos(null);
            setSelectedCouncil(null);
            setSuccessMessage(selectedCouncil ? 'Раду оновлено успішно!' : 'Раду створено успішно!');
            setTimeout(() => setSuccessMessage(''), 3000);
            fetchData();
        } else {
            setFormError(result.errors?.[0] || 'Не вдалося зберегти раду.');
        }
        setIsSubmitting(false);
    };

    const handleUserSubmit = async (e) => {
        e.preventDefault();
        setFormError('');

        setIsSubmitting(true);

        let result;
        if (selectedUser) {
            const userIdentifier = selectedUser.id || selectedUser.userId || selectedUser.Id;
            result = await AdminService.updateUser(userIdentifier, userFormData);
        } else {
            result = await AdminService.createUser(userFormData);
        }

        if (result.success) {
            setShowUserModal(false);
            setUserFormData({
                fullName: '', email: '', password: '', role: 'Citizen', status: 'Active'
            });
            setSelectedUser(null);
            setSuccessMessage(selectedUser ? 'Користувача оновлено успішно!' : 'Користувача створено успішно!');
            setTimeout(() => setSuccessMessage(''), 3000);
            fetchData();
        } else {
            setFormError(result.errors?.[0] || 'Не вдалося зберегти користувача.');
        }
        setIsSubmitting(false);
    };

    const handleDeleteUser = async (userId) => {
        if (!window.confirm('Ви впевнені, що хочете видалити цього користувача?')) return;

        const result = await AdminService.deleteUser(userId);
        if (result.success) {
            setSuccessMessage('Користувача видалено успішно!');
            setTimeout(() => setSuccessMessage(''), 3000);
            fetchData();
        } else {
            setError(result.errors?.[0] || 'Не вдалося видалити користувача.');
        }
    };

    const handleDeleteCouncil = async (councilId) => {
        if (!window.confirm('Ви впевнені, що хочете видалити цю раду?')) return;

        const result = await AdminService.deleteCityCouncil(councilId);
        if (result.success) {
            setSuccessMessage('Раду видалено успішно!');
            setTimeout(() => setSuccessMessage(''), 3000);
            fetchData();
        } else {
            setError(result.errors?.[0] || 'Не вдалося видалити раду.');
        }
    };

    const handleCSVUpload = async (e) => {
        e.preventDefault();
        setFormError('');

        if (!csvFile) {
            setFormError('Будь ласка, оберіть CSV файл.');
            return;
        }

        setIsSubmitting(true);

        const result = await AdminService.uploadAppealsFromCSV(csvFile);

        if (result.success) {
            setShowCSVModal(false);
            setCsvFile(null);
            setSuccessMessage(result.message || 'Звернення завантажено успішно!');
            setTimeout(() => setSuccessMessage(''), 3000);
            fetchData();
        } else {
            setFormError(result.errors?.[0] || 'Не вдалося завантажити звернення з CSV.');
        }
        setIsSubmitting(false);
    };

    const openEditUserModal = (user) => {
        setSelectedUser(user);
        setUserFormData({
            fullName: user.fullName || user.name || '',
            email: user.email || '',
            password: '',
            role: user.role || 'Citizen',
            status: user.status || 'Active'
        });
        setShowUserModal(true);
    };

    const openEditCouncilModal = async (council) => {
        setSelectedCouncil(council);
        setCouncilFormData({
            name: council.name || '',
            contactEmail: council.contactEmail || '',
            headUserId: council.headUserId || '',
            latitude: council.latitude || 0,
            longitude: council.longitude || 0
        });
        if (council.latitude && council.longitude) {
            setMapCenter({ lat: council.latitude, lng: council.longitude });
            setMarkerPos({ lat: council.latitude, lng: council.longitude });
        }
        setEmailSearch('');
        setShowCouncilModal(true);
    };

    return (
        <div className="admin-dashboard-container">
            <div className="admin-header">
                <h1>Панель адміністратора</h1>
                <p>Керування користувачами, міськими радами та аналітика</p>
            </div>

            {successMessage && (
                <div style={{
                    padding: '12px 16px',
                    marginBottom: '20px',
                    backgroundColor: '#d1fae5',
                    color: '#065f46',
                    borderRadius: '6px',
                    border: '1px solid #a7f3d0'
                }}>
                    {successMessage}
                </div>
            )}

            <div className="admin-stats-grid">
                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Користувачі</h3>
                        <p className="stat-value">{Array.isArray(users) ? users.length : '-'}</p>
                        <p className="stat-subtext">зареєстровано</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#2563eb' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle></svg>
                    </div>
                </div>

                <div className="admin-stat-card">
                    <div className="stat-info">
                        <h3>Міські ради</h3>
                        <p className="stat-value">{Array.isArray(councils) ? councils.length : '-'}</p>
                        <p className="stat-subtext">активні об'єкти</p>
                    </div>
                    <div className="stat-icon" style={{ color: '#10b981' }}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path><polyline points="9 22 9 12 15 12 15 22"></polyline></svg>
                    </div>
                </div>
            </div>

            <div className="admin-tabs">
                <button className={`admin-tab ${activeTab === 'users' ? 'active' : ''}`} onClick={() => setActiveTab('users')}>
                    Користувачі
                </button>
                <button className={`admin-tab ${activeTab === 'councils' ? 'active' : ''}`} onClick={() => setActiveTab('councils')}>
                    Міські ради
                </button>
            </div>

            <div className="admin-content-card">
                {loading && <div style={{ padding: '40px 0', textAlign: 'center' }}>Завантаження...</div>}

                {activeTab === 'users' && !loading && (
                    <div style={{ overflowX: 'auto' }}>
                        <table className="admin-table">
                            <thead>
                            <tr><th>Ім'я</th><th>Email</th><th>Роль</th><th>Статус</th><th>Дії</th></tr>
                            </thead>
                            <tbody>
                            {users.map((u) => (
                                <tr key={u.id || u.userId || u.Id}>
                                    <td>{u.fullName || u.name}</td>
                                    <td>{u.email}</td>
                                    <td><span className={`badge badge-role-${(u.role || 'Citizen').toLowerCase()}`}>{u.role}</span></td>
                                    <td><span className={`badge badge-status-${(u.status || 'active').toLowerCase()}`}>{u.status}</span></td>
                                    <td>
                                        <button className="btn-small btn-edit" onClick={() => openEditUserModal(u)}>Редагувати</button>
                                        <button className="btn-small btn-delete" onClick={() => handleDeleteUser(u.id || u.userId || u.Id)}>Видалити</button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {activeTab === 'councils' && !loading && (
                    <>
                        <div className="content-header">
                            <h2>Список міських рад</h2>
                            <div className="button-group">
                                <button className="btn-primary" onClick={() => {
                                    setSelectedCouncil(null);
                                    setCouncilFormData({
                                        name: '', contactEmail: '', headUserId: '',
                                        latitude: 0, longitude: 0
                                    });
                                    setEmailSearch('');
                                    setMarkerPos(null);
                                    setShowCouncilModal(true);
                                }}>+ Додати раду</button>
                                <button className="btn-secondary" onClick={() => setShowCSVModal(true)}>⬆️ Завантажити звернення</button>
                            </div>
                        </div>
                        <div style={{ overflowX: 'auto' }}>
                            <table className="admin-table">
                                <thead>
                                <tr><th>Назва</th><th>Email</th><th>Місто</th><th>Район</th><th>Дії</th></tr>
                                </thead>
                                <tbody>
                                {councils.map((c) => (
                                    <tr key={getCouncilIdentifier(c)}>
                                        <td style={{ fontWeight: '500' }}>{c.name}</td>
                                        <td>{c.contactEmail}</td><td>{c.city || 'N/A'}</td><td>{c.district || 'N/A'}</td>
                                        <td>
                                            <button className="btn-small btn-edit" onClick={() => openEditCouncilModal(c)}>Редагувати</button>
                                            <button className="btn-small btn-delete" onClick={() => handleDeleteCouncil(getCouncilIdentifier(c))}>Видалити</button>
                                        </td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </>
                )}
            </div>

            {showCouncilModal && (
                <div className="admin-modal-overlay" onClick={() => setShowCouncilModal(false)}>
                    <div className="admin-modal-content" onClick={e => e.stopPropagation()}>
                        <h2>{selectedCouncil ? 'Редагування міської ради' : 'Створення міської ради'}</h2>
                        {formError && <div className="alert alert-error">{formError}</div>}

                        <form onSubmit={handleCouncilSubmit}>
                            <div className="form-grid">
                                <div className="form-group full-width">
                                    <label>Назва ради</label>
                                    <input type="text" name="name" value={councilFormData.name} onChange={handleCouncilInputChange} required />
                                </div>

                                <div className="form-group">
                                    <label>Контактний Email</label>
                                    <input type="email" name="contactEmail" value={councilFormData.contactEmail} onChange={handleCouncilInputChange} required />
                                </div>

                                <div className="form-group autocomplete-wrapper">
                                    <label>Голова ради (Пошук за Email)</label>
                                    <input
                                        type="text"
                                        value={emailSearch}
                                        onChange={handleEmailChange}
                                        placeholder="Введіть email..."
                                        required
                                        style={{ borderColor: councilFormData.headUserId ? '#10b981' : '#d1d5db' }}
                                    />
                                    {suggestions.length > 0 && (
                                        <div className="suggestions-dropdown">
                                            {suggestions.map(u => (
                                                <div key={u.id || u.userId || u.Id} className="suggestion-item" onClick={() => selectUser(u)}>
                                                    {u.email} ({u.fullName || u.name})
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>

                                <div className="form-group full-width" style={{ marginTop: '10px' }}>
                                    <label style={{ color: '#2563eb' }}>Пошук адреси або клік по карті</label>
                                    {isLoaded ? (
                                        <>
                                            <Autocomplete onLoad={onLoadAutocomplete} onPlaceChanged={onPlaceChanged}>
                                                <input
                                                    type="text"
                                                    placeholder="Шукати в Google Maps..."
                                                    style={{ border: '2px solid #2563eb', width: '100%', marginBottom: '10px' }}
                                                    onKeyDown={(e) => { e.key === 'Enter' && e.preventDefault(); }}
                                                />
                                            </Autocomplete>

                                            <GoogleMap
                                                mapContainerStyle={mapContainerStyle}
                                                center={mapCenter}
                                                zoom={14}
                                                options={{ streetViewControl: false, mapTypeControl: false }}
                                                onClick={onMapClick}
                                            >
                                                {markerPos && <Marker position={markerPos} />}
                                            </GoogleMap>
                                        </>
                                    ) : <div>Завантаження карт...</div>}
                                </div>

                                {councilFormData.latitude !== 0 && (
                                    <div className="form-group full-width" style={{ fontSize: '13px', color: '#10b981', marginTop: '-10px', fontWeight: '500' }}>
                                        ✓ Координати збережено: {councilFormData.latitude.toFixed(5)}, {councilFormData.longitude.toFixed(5)}
                                    </div>
                                )}
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setShowCouncilModal(false)}>Скасувати</button>
                                <button type="submit" className="btn-primary" disabled={isSubmitting || !councilFormData.headUserId || !councilFormData.latitude}>
                                    {isSubmitting ? 'Збереження...' : selectedCouncil ? 'Оновити раду' : 'Зберегти раду'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {showUserModal && (
                <div className="admin-modal-overlay" onClick={() => setShowUserModal(false)}>
                    <div className="admin-modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '500px' }}>
                        <h2>{selectedUser ? 'Редагування користувача' : 'Створення користувача'}</h2>
                        {formError && <div className="alert alert-error">{formError}</div>}

                        <form onSubmit={handleUserSubmit}>
                            <div className="form-grid">
                                <div className="form-group full-width">
                                    <label>Повне ім'я</label>
                                    <input type="text" name="fullName" value={userFormData.fullName} onChange={handleUserInputChange} required />
                                </div>

                                <div className="form-group full-width">
                                    <label>Email</label>
                                    <input type="email" name="email" value={userFormData.email} onChange={handleUserInputChange} required />
                                </div>

                                {!selectedUser && (
                                    <div className="form-group full-width">
                                        <label>Пароль</label>
                                        <input type="password" name="password" value={userFormData.password} onChange={handleUserInputChange} required />
                                    </div>
                                )}

                                <div className="form-group">
                                    <label>Роль</label>
                                    <select name="role" value={userFormData.role} onChange={handleUserInputChange}>
                                        <option value="Citizen">Громадянин</option>
                                        <option value="CouncilHead">Голова ради</option>
                                        <option value="CouncilEmployee">Робітник ради</option>
                                        <option value="Admin">Адміністратор</option>
                                    </select>
                                </div>

                                <div className="form-group">
                                    <label>Статус</label>
                                    <select name="status" value={userFormData.status} onChange={handleUserInputChange}>
                                        <option value="Active">Активний</option>
                                        <option value="Inactive">Неактивний</option>
                                    </select>
                                </div>
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setShowUserModal(false)}>Скасувати</button>
                                <button type="submit" className="btn-primary" disabled={isSubmitting}>
                                    {isSubmitting ? 'Збереження...' : selectedUser ? 'Оновити користувача' : 'Зберегти користувача'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {showCSVModal && (
                <div className="admin-modal-overlay" onClick={() => setShowCSVModal(false)}>
                    <div className="admin-modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '500px' }}>
                        <h2>Завантажити звернення з CSV</h2>
                        {formError && <div className="alert alert-error">{formError}</div>}

                        <form onSubmit={handleCSVUpload}>
                            <div className="form-grid">
                                <div className="form-group full-width">
                                    <label>Виберіть файл CSV зі зверненнями</label>
                                    <input
                                        type="file"
                                        accept=".csv"
                                        onChange={(e) => setCsvFile(e.target.files?.[0] || null)}
                                        required
                                    />
                                </div>
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setShowCSVModal(false)}>Скасувати</button>
                                <button type="submit" className="btn-primary" disabled={isSubmitting || !csvFile}>
                                    {isSubmitting ? 'Завантаження...' : 'Завантажити звернення'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default AdminDashboard;
