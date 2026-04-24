import React, { useState, useEffect } from 'react';
import { Autocomplete, useJsApiLoader, GoogleMap, Marker } from '@react-google-maps/api';
import '../styles/AdminDashboard.css';
import AdminService from '../services/AdminService';

// Налаштування для карти
const libraries = ['places'];
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

function AdminDashboard() {
    const [activeTab, setActiveTab] = useState('users');

    // Дані з бекенду
    const [users, setUsers] = useState([]);
    const [councils, setCouncils] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // Стан модального вікна
    const [showModal, setShowModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formError, setFormError] = useState('');

    // Пошук голови ради (Head User)
    const [emailSearch, setEmailSearch] = useState('');
    const [suggestions, setSuggestions] = useState([]);

    // Стан Google Maps
    const [autocomplete, setAutocomplete] = useState(null);
    const [mapCenter, setMapCenter] = useState(defaultCenter);
    const [markerPos, setMarkerPos] = useState(null);

    // Ініціалізація карти
    const { isLoaded } = useJsApiLoader({
        googleMapsApiKey: process.env.REACT_APP_GOOGLE_MAPS_API_KEY,
        libraries,
        language: 'uk',
        region: 'UA'
    });

    // Стан форми (лише основні дані та координати)
    const [formData, setFormData] = useState({
        name: '', contactEmail: '', headUserId: '',
        latitude: 0, longitude: 0
    });

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

        if (formData.headUserId) {
            setFormData(prev => ({ ...prev, headUserId: '' }));
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
        setFormData(prev => ({ ...prev, headUserId: user.id || user.userId }));
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

                // Зберігаємо лише координати
                setFormData(prev => ({
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
        setFormData(prev => ({ ...prev, latitude: lat, longitude: lng }));
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setFormError('');

        if (!formData.headUserId) {
            setFormError('Будь ласка, оберіть дійсного користувача зі списку підказок email.');
            return;
        }

        if (!formData.latitude || !formData.longitude) {
            setFormError('Будь ласка, оберіть адресу на карті або через пошук.');
            return;
        }

        setIsSubmitting(true);

        const payload = {
            name: formData.name,
            contactEmail: formData.contactEmail,
            headUserId: formData.headUserId,
            latitude: parseFloat(formData.latitude),
            longitude: parseFloat(formData.longitude)
        };

        const result = await AdminService.createCityCouncil(payload);

        if (result.success) {
            setShowModal(false);
            setFormData({
                name: '', contactEmail: '', headUserId: '',
                latitude: 0, longitude: 0
            });
            setEmailSearch('');
            setMarkerPos(null);
            fetchData();
        } else {
            setFormError(result.errors?.[0] || 'Не вдалося створити раду.');
        }
        setIsSubmitting(false);
    };

    return (
        <div className="admin-dashboard-container">
            <div className="admin-header">
                <h1>Панель адміністратора</h1>
                <p>Керування користувачами, міськими радами та аналітика</p>
            </div>

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
                            <tr><th>Ім'я</th><th>Email</th><th>Роль</th><th>Статус</th></tr>
                            </thead>
                            <tbody>
                            {users.map((u) => (
                                <tr key={u.id || u.userId}>
                                    <td>{u.fullName || u.name}</td>
                                    <td>{u.email}</td>
                                    <td><span className={`badge badge-role-${(u.role || 'Citizen').toLowerCase()}`}>{u.role}</span></td>
                                    <td><span className={`badge badge-status-${(u.status || 'active').toLowerCase()}`}>{u.status}</span></td>
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
                            <button className="btn-primary" onClick={() => setShowModal(true)}>+ Додати раду</button>
                        </div>
                        <div style={{ overflowX: 'auto' }}>
                            <table className="admin-table">
                                <thead>
                                <tr><th>Назва</th><th>Email</th><th>Місто</th><th>Район</th></tr>
                                </thead>
                                <tbody>
                                {councils.map((c) => (
                                    <tr key={c.id}>
                                        <td style={{ fontWeight: '500' }}>{c.name}</td>
                                        <td>{c.contactEmail}</td><td>{c.city || 'N/A'}</td><td>{c.district || 'N/A'}</td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </>
                )}
            </div>

            {showModal && (
                <div className="admin-modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="admin-modal-content" onClick={e => e.stopPropagation()}>
                        <h2>Створення міської ради</h2>
                        {formError && <div className="alert alert-error">{formError}</div>}

                        <form onSubmit={handleSubmit}>
                            <div className="form-grid">
                                <div className="form-group full-width">
                                    <label>Назва ради</label>
                                    <input type="text" name="name" value={formData.name} onChange={handleInputChange} required />
                                </div>

                                <div className="form-group">
                                    <label>Контактний Email</label>
                                    <input type="email" name="contactEmail" value={formData.contactEmail} onChange={handleInputChange} required />
                                </div>

                                <div className="form-group autocomplete-wrapper">
                                    <label>Голова ради (Пошук за Email)</label>
                                    <input
                                        type="text"
                                        value={emailSearch}
                                        onChange={handleEmailChange}
                                        placeholder="Введіть email..."
                                        required
                                        style={{ borderColor: formData.headUserId ? '#10b981' : '#d1d5db' }}
                                    />
                                    {suggestions.length > 0 && (
                                        <div className="suggestions-dropdown">
                                            {suggestions.map(u => (
                                                <div key={u.id || u.userId} className="suggestion-item" onClick={() => selectUser(u)}>
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

                                {/* Відображення збережених координат */}
                                {formData.latitude !== 0 && (
                                    <div className="form-group full-width" style={{ fontSize: '13px', color: '#10b981', marginTop: '-10px', fontWeight: '500' }}>
                                        ✓ Координати збережено: {formData.latitude.toFixed(5)}, {formData.longitude.toFixed(5)}
                                    </div>
                                )}
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={() => setShowModal(false)}>Скасувати</button>
                                <button type="submit" className="btn-primary" disabled={isSubmitting || !formData.headUserId || !formData.latitude}>
                                    {isSubmitting ? 'Збереження...' : 'Зберегти раду'}
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