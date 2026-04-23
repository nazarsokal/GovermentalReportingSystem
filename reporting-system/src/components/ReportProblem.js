import React, { useState, useEffect, useCallback } from 'react';
import { GoogleMap, Marker, Autocomplete, useJsApiLoader } from '@react-google-maps/api';
import AppealService from '../services/AppealService';

// Defining outside component to avoid re-renders
const libraries = ['places'];

const mapContainerStyle = {
    width: '100%',
    height: '300px',
    borderRadius: '8px',
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
        googleMapsApiKey: process.env.REACT_APP_GOOGLE_MAPS_API_KEY,
        libraries,
    });

    const [formData, setFormData] = useState({
        categoryId: '',
        title: '',
        description: '',
        city: '', // Initialize empty, respect map coordinates
        street: '',
        buildingNumber: '',
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
        const files = Array.from(e.target.files);
        setFormData(prev => ({ ...prev, photos: files }));
    };

    // --- GOOGLE MAPS LOGIC ---

    const extractAddressComponents = (components) => {
        let city = '', street = '', building = '';

        components.forEach(component => {
            const types = component.types;
            if (types.includes('locality')) city = component.long_name;
            if (types.includes('route')) street = component.long_name;
            if (types.includes('street_number')) building = component.long_name;
        });

        setFormData(prev => ({
            ...prev,
            city: city || '',
            street: street || '',
            buildingNumber: building || ''
        }));
    };

    const onMapClick = useCallback((e) => {
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();

        setMarkerPos({ lat, lng });

        // Set coordinates but clear optional address fields because the user clicked manually
        setFormData(prev => ({
            ...prev,
            latitude: lat,
            longitude: lng,
            city: '',
            street: '',
            buildingNumber: ''
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

                // Extract and populate address fields because user used the search bar
                if (place.address_components) {
                    extractAddressComponents(place.address_components);
                }
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
                city: '',
                street: '',
                buildingNumber: '',
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
        <div className="report-problem-container" style={{ maxWidth: '600px', margin: '0 auto', padding: '20px', backgroundColor: 'white', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
            <h2 style={{ marginBottom: '20px' }}>Report an Infrastructure Problem</h2>

            {error && <div style={{ color: 'red', marginBottom: '15px', padding: '10px', backgroundColor: '#ffebee', borderRadius: '4px' }}>{error}</div>}
            {successMsg && <div style={{ color: 'green', marginBottom: '15px', padding: '10px', backgroundColor: '#e8f5e9', borderRadius: '4px' }}>{successMsg}</div>}

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>

                {/* Map Section */}
                <div style={{ border: '1px solid #ccc', padding: '10px', borderRadius: '4px', backgroundColor: '#f9f9f9' }}>
                    <label style={{ display: 'block', marginBottom: '10px', fontWeight: 'bold' }}>Location (Search or Click Map)</label>

                    {isLoaded ? (
                        <>
                            <Autocomplete onLoad={onLoadAutocomplete} onPlaceChanged={onPlaceChanged}>
                                <input
                                    type="text"
                                    placeholder="Search for an address..."
                                    style={{ width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box' }}
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
                        <div>Loading Map...</div>
                    )}

                    {/* Read-only Coordinates */}
                    <div style={{ display: 'flex', gap: '10px', marginTop: '10px' }}>
                        <div style={{ flex: 1 }}>
                            <input type="text" readOnly value={formData.latitude ? `Lat: ${formData.latitude.toFixed(6)}` : 'Latitude'} style={{ width: '100%', padding: '8px', backgroundColor: '#eee', border: '1px solid #ccc', borderRadius: '4px' }} />
                        </div>
                        <div style={{ flex: 1 }}>
                            <input type="text" readOnly value={formData.longitude ? `Lng: ${formData.longitude.toFixed(6)}` : 'Longitude'} style={{ width: '100%', padding: '8px', backgroundColor: '#eee', border: '1px solid #ccc', borderRadius: '4px' }} />
                        </div>
                    </div>
                </div>

                {/* Form Details (No 'required' tags) */}
                <div style={{ display: 'flex', gap: '10px' }}>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>City (Optional)</label>
                        <input type="text" name="city" value={formData.city} onChange={handleInputChange} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                    <div style={{ flex: 1 }}>
                        <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Street (Optional)</label>
                        <input type="text" name="street" value={formData.street} onChange={handleInputChange} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                    <div style={{ flex: 0.5 }}>
                        <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Building</label>
                        <input type="text" name="buildingNumber" value={formData.buildingNumber} onChange={handleInputChange} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Category</label>
                    <select name="categoryId" value={formData.categoryId} onChange={handleInputChange} required style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', backgroundColor: 'white' }}>
                        <option value="">-- Select Category --</option>
                        {categories.map(cat => (
                            <option key={cat.id || cat.categoryId} value={cat.id || cat.categoryId}>
                                {cat.name || cat.title || 'Category'}
                            </option>
                        ))}
                    </select>
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Title</label>
                    <input type="text" name="title" value={formData.title} onChange={handleInputChange} required placeholder="e.g., Deep pothole on main road" style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }} />
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Description</label>
                    <textarea name="description" value={formData.description} onChange={handleInputChange} required rows="4" placeholder="Provide details about the problem..." style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', resize: 'vertical' }} />
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Photos (Optional)</label>
                    <input type="file" multiple accept="image/*" onChange={handleFileChange} style={{ width: '100%', padding: '8px' }} />
                </div>

                <button type="submit" disabled={loading} style={{ marginTop: '10px', padding: '12px', backgroundColor: '#4285F4', color: 'white', border: 'none', borderRadius: '4px', cursor: loading ? 'not-allowed' : 'pointer', fontWeight: 'bold', fontSize: '16px' }}>
                    {loading ? 'Submitting...' : 'Submit Appeal'}
                </button>

            </form>
        </div>
    );
}

export default ReportProblem;