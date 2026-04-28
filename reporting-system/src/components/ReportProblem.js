import React, { useState, useEffect, useCallback } from 'react';
import { GoogleMap, Marker, Autocomplete, useJsApiLoader } from '@react-google-maps/api';
import AppealService from '../services/AppealService';
import '../styles/ReportProblem.css';
import { googleMapsLoaderOptions } from '../constants/googleMaps';

const mapContainerStyle = {
    width: '100%',
    height: '240px',
    borderRadius: '14px',
    marginTop: '10px'
};

const defaultCenter = {
    lat: 49.8397,
    lng: 24.0297 // Default to Lviv
};

function ReportProblem({ user, onSuccess }) {
    const [categories, setCategories] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [successMsg, setSuccessMsg] = useState('');

    const [mapCenter, setMapCenter] = useState(defaultCenter);
    const [markerPos, setMarkerPos] = useState(null);
    const [autocomplete, setAutocomplete] = useState(null);

    const { isLoaded, loadError } = useJsApiLoader({
        ...googleMapsLoaderOptions
    });

    const [formData, setFormData] = useState({
        categoryId: '',
        title: '',
        description: '',
        latitude: '',
        longitude: '',
        photos: []
    });

    // Fetch categories on mount
    useEffect(() => {
        const loadCategories = async () => {
            const result = await AppealService.getCategories();
            if (result.success && result.categories.length > 0) {
                setCategories(result.categories);
                setFormData(prev => ({ ...prev, categoryId: result.categories[0].id || result.categories[0].categoryId }));
            }
        };
        loadCategories();
    }, []);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleFileChange = (e) => {
        const newFiles = Array.from(e.target.files || []);
        setFormData(prev => {
            const existing = prev.photos || [];
            const merged = [...existing, ...newFiles];
            const unique = merged.filter((file, index, arr) =>
                index === arr.findIndex(f =>
                    f.name === file.name &&
                    f.size === file.size &&
                    f.lastModified === file.lastModified
                )
            );
            return { ...prev, photos: unique };
        });
        // Allow selecting the same file again if user removed it
        e.target.value = '';
    };

    const handleRemovePhoto = (indexToRemove) => {
        setFormData(prev => ({
            ...prev,
            photos: (prev.photos || []).filter((_, idx) => idx !== indexToRemove)
        }));
    };

    // --- GOOGLE MAPS LOGIC ---

    const onMapClick = useCallback((e) => {
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();

        setMarkerPos({ lat, lng });

        setFormData(prev => ({
            ...prev,
            latitude: lat,
            longitude: lng
        }));
    }, []);

    const onLoadAutocomplete = (autocompleteInstance) => {
        setAutocomplete(autocompleteInstance);
    };

    const onPlaceChanged = () => {
        if (autocomplete !== null) {
            const place = autocomplete.getPlace();
            if (place.geometry && place.geometry.location) {
                const lat = place.geometry.location.lat();
                const lng = place.geometry.location.lng();

                setMapCenter({ lat, lng });
                setMarkerPos({ lat, lng });
                setFormData(prev => ({ ...prev, latitude: lat, longitude: lng }));
            }
        }
    };

    // --- SUBMISSION ---

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccessMsg('');

        if (!formData.latitude || !formData.longitude) {
            setError('Please select a location on the map.');
            return;
        }

        setLoading(true);

        const userId = user?.id || user?.userId;
        if (!userId) {
            setError('User ID not found. Please log in again.');
            setLoading(false);
            return;
        }

        const payload = {
            ...formData,
            userId: userId,
            latitude: parseFloat(formData.latitude),
            longitude: parseFloat(formData.longitude)
        };

        const result = await AppealService.createAppeal(payload);

        if (result.success) {
            setSuccessMsg('Problem reported successfully!');

            // Reset the form
            setFormData({
                categoryId: categories.length > 0 ? categories[0].id || categories[0].categoryId : '',
                title: '',
                description: '',
                latitude: '',
                longitude: '',
                photos: []
            });
            setMarkerPos(null);

            // Trigger navigation back
            if (onSuccess) setTimeout(onSuccess, 1500);
        } else {
            setError(result.errors?.[0] || 'An error occurred while reporting the problem.');
        }
        setLoading(false);
    };

    if (loadError) return <div>Error loading maps</div>;

    return (
        <div className="report-problem-container">
            <div className="report-problem-header">
                <h2>Report an Infrastructure Problem</h2>
                <p>Select a location and describe the issue.</p>
            </div>

            {error && <div className="report-alert report-alert-error">{error}</div>}
            {successMsg && <div className="report-alert report-alert-success">{successMsg}</div>}

            <form onSubmit={handleSubmit} className="report-form">

                {/* Map Section */}
                <div className="report-map-card">
                    <label className="report-label">Location (Search or Click Map)</label>

                    {isLoaded ? (
                        <>
                            <Autocomplete onLoad={onLoadAutocomplete} onPlaceChanged={onPlaceChanged}>
                                <input
                                    type="text"
                                    placeholder="Search for an address..."
                                    className="report-input"
                                    onKeyDown={(e) => { e.key === 'Enter' && e.preventDefault(); }} // Prevent form submission
                                />
                            </Autocomplete>

                            <GoogleMap
                                mapContainerStyle={mapContainerStyle}
                                center={mapCenter}
                                zoom={13}
                                onClick={onMapClick}
                                options={{ streetViewControl: false, mapTypeControl: false }}
                            >
                                {markerPos && <Marker position={markerPos} />}
                            </GoogleMap>
                        </>
                    ) : (
                        <div className="report-muted">Loading Map...</div>
                    )}

                    {/* Read-only Coordinates */}
                    <div className="report-coords">
                        <input className="report-input report-input-readonly" type="text" readOnly value={formData.latitude ? `Lat: ${formData.latitude.toFixed(6)}` : 'Latitude'} />
                        <input className="report-input report-input-readonly" type="text" readOnly value={formData.longitude ? `Lng: ${formData.longitude.toFixed(6)}` : 'Longitude'} />
                    </div>
                </div>

                {/* Form Details (No 'required' tags) */}
                <div className="report-grid">
                    <div className="report-field">
                        <label className="report-label">Category</label>
                        <select className="report-input" name="categoryId" value={formData.categoryId} onChange={handleInputChange} required>
                        <option value="">-- Select Category --</option>
                        {categories.map(cat => (
                            <option key={cat.id || cat.categoryId} value={cat.id || cat.categoryId}>
                                {cat.name || cat.title || 'Category'}
                            </option>
                        ))}
                        </select>
                    </div>

                    <div className="report-field">
                        <label className="report-label">Title</label>
                        <input className="report-input" type="text" name="title" value={formData.title} onChange={handleInputChange} required placeholder="e.g., Deep pothole on main road" />
                    </div>

                    <div className="report-field report-field-full">
                        <label className="report-label">Description</label>
                        <textarea className="report-input report-textarea" name="description" value={formData.description} onChange={handleInputChange} required rows="4" placeholder="Provide details about the problem..." />
                    </div>

                    <div className="report-field report-field-full">
                        <label className="report-label">Photos (Optional)</label>
                        <input
                            className="report-file"
                            type="file"
                            name="Photos"
                            multiple
                            accept="image/*"
                            onChange={handleFileChange}
                        />
                        {formData.photos?.length > 0 && (
                            <div className="report-photos-list">
                                <div className="report-photos-title">Attached: {formData.photos.length}</div>
                                {formData.photos.map((photo, index) => (
                                    <div key={`${photo.name}-${photo.size}-${index}`} className="report-photo-item">
                                        <span className="report-photo-name">{photo.name}</span>
                                        <button
                                            type="button"
                                            className="report-photo-remove"
                                            onClick={() => handleRemovePhoto(index)}
                                        >
                                            Remove
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                <button type="submit" disabled={loading} className="report-submit">
                    {loading ? 'Submitting...' : 'Submit Appeal'}
                </button>

            </form>
        </div>
    );
}

export default ReportProblem;
