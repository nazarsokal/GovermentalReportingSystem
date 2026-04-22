import React, { useMemo, useState, useRef, useEffect } from 'react';
import { GoogleMap, Marker } from '@react-google-maps/api';
import AddressService from '../services/AddressService';
import '../styles/CityMap.css';

// Default Kyiv coordinates (fallback if API fails)
const DEFAULT_COORDINATES = { lat: 50.4501, lng: 30.5234 };

const containerStyle = {
  width: '100%',
  height: '350px',
  borderRadius: '8px',
};

function CityMap({ user }) {
  const [mapError, setMapError] = useState(null);
  const [mapLoaded, setMapLoaded] = useState(false);
  const [districtCoordinates, setDistrictCoordinates] = useState({});
  const [coordinatesLoading, setCoordinatesLoading] = useState(true);
  const [coordinatesError, setCoordinatesError] = useState(null);
  const mapRef = useRef(null);

  // Fetch all district coordinates on component mount
  useEffect(() => {
    const fetchCoordinates = async () => {
      setCoordinatesLoading(true);
      const result = await AddressService.getAllDistrictCoordinates();

      if (result.success) {
        console.log('District coordinates loaded:', result.coordinates);
        setDistrictCoordinates(result.coordinates);
        setCoordinatesError(null);
      } else {
        console.warn('Failed to fetch district coordinates, using defaults:', result.errors);
        setCoordinatesError(result.errors?.[0] || 'Failed to load coordinates');
      }
      setCoordinatesLoading(false);
    };

    fetchCoordinates();
  }, []);

  // Get the center coordinates based on user's district
  const center = useMemo(() => {
    console.log('🗺️ CityMap user object:', user);
    console.log('🗺️ User district (from user.district):', user?.district);
    console.log('🗺️ Available district coordinates:', districtCoordinates);

    // Check localStorage as fallback
    const storedDistrict = user?.district || localStorage.getItem('userDistrict');
    console.log('🗺️ Final district to use:', storedDistrict);
    console.log('🗺️ localStorage.userDistrict:', localStorage.getItem('userDistrict'));

    if (storedDistrict && districtCoordinates[storedDistrict]) {
      const coordinates = districtCoordinates[storedDistrict];
      console.log('✅ Map centering on district:', storedDistrict, coordinates);
      return coordinates;
    }

    console.log('⚠️ Map defaulting to Kyiv (district not found)');
    return DEFAULT_COORDINATES;
  }, [user, districtCoordinates]);

  const mapOptions = {
    zoom: 12,
    center: center,
    mapTypeId: 'roadmap',
    fullscreenControl: true,
    zoomControl: true,
    streetViewControl: false,
    styles: [
      {
        featureType: 'administrative',
        elementType: 'labels.text.fill',
        stylers: [{ color: '#444444' }]
      },
      {
        featureType: 'landscape',
        elementType: 'all',
        stylers: [{ color: '#f2f2f2' }]
      },
      {
        featureType: 'poi',
        elementType: 'all',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'road',
        elementType: 'all',
        stylers: [{ saturation: -100 }, { lightness: 45 }]
      },
      {
        featureType: 'road.highway',
        elementType: 'all',
        stylers: [{ visibility: 'simplified' }]
      },
      {
        featureType: 'road.arterial',
        elementType: 'labels.icon',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'transit',
        elementType: 'all',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'water',
        elementType: 'all',
        stylers: [{ color: '#46bcda' }, { visibility: 'on' }]
      }
    ]
  };

  const handleMapLoad = (map) => {
    console.log('Google Map loaded successfully');
    mapRef.current = map;
    setMapLoaded(true);
    setMapError(null);
  };

  const handleMapError = (error) => {
    console.error('Google Map Error:', error);
    setMapError('Failed to load Google Maps. Please check your API key and browser console.');
  };

  const hasApiKey = process.env.REACT_APP_GOOGLE_MAPS_API_KEY &&
                    process.env.REACT_APP_GOOGLE_MAPS_API_KEY.trim() !== '';

  if (!hasApiKey) {
    return (
      <div className="city-map-container">
        <div className="map-error">
          <div className="error-content">
            <p><strong>Google Maps API key not configured</strong></p>
            <p>Please add your Google Maps API key to the .env file:</p>
            <p><code>REACT_APP_GOOGLE_MAPS_API_KEY=your_key_here</code></p>
            <p style={{ fontSize: '12px', marginTop: '8px' }}>Get your key from: <a href="https://console.cloud.google.com/" target="_blank" rel="noopener noreferrer">Google Cloud Console</a></p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="city-map-container">
      {mapError && (
        <div className="map-error">
          <div className="error-content">
            <p><strong>Map Loading Error</strong></p>
            <p>{mapError}</p>
            <p style={{ fontSize: '12px', marginTop: '8px' }}>Check the browser console (F12) for more details</p>
          </div>
        </div>
      )}

      {coordinatesLoading && (
        <div className="map-loading-overlay">
          <div className="loading-spinner"></div>
          <p>Loading map coordinates...</p>
        </div>
      )}

      {!mapError && !coordinatesLoading && (
        <>
          <GoogleMap
            mapContainerStyle={containerStyle}
            center={center}
            zoom={12}
            options={mapOptions}
            onLoad={handleMapLoad}
            onError={handleMapError}
          >
            {/* Marker for user's district */}
            {(user?.district || localStorage.getItem('userDistrict')) && (
              <Marker
                position={center}
                title={user?.district || localStorage.getItem('userDistrict')}
              />
            )}
          </GoogleMap>

          {!mapLoaded && (
            <div className="map-loading-overlay">
              <div className="loading-spinner"></div>
              <p>Loading map...</p>
            </div>
          )}
        </>
      )}

      {coordinatesError && !coordinatesLoading && (
        <div style={{
          position: 'absolute',
          top: '10px',
          right: '10px',
          backgroundColor: '#fff3cd',
          border: '1px solid #ffc107',
          padding: '10px',
          borderRadius: '4px',
          fontSize: '12px',
          maxWidth: '200px',
          zIndex: 10
        }}>
          <p><strong>⚠️ Using default coordinates</strong></p>
          <p>{coordinatesError}</p>
        </div>
      )}
    </div>
  );
}

export default CityMap;

