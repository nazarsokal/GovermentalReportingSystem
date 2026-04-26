import React, { useState, useRef, useEffect } from 'react';
import { GoogleMap, Marker } from '@react-google-maps/api';
import AddressService from '../services/AddressService';
import AppealService from '../services/AppealService';
import '../styles/CityMap.css';

const DEFAULT_COORDINATES = { lat: 50.4501, lng: 30.5234 };

const containerStyle = {
  width: '100%',
  height: '350px',
  borderRadius: '8px',
};

function CityMap({ user, onViewDetails }) {
  const [mapError, setMapError] = useState(null);
  const [mapLoaded, setMapLoaded] = useState(false);
  const [districtCoordinates, setDistrictCoordinates] = useState({});
  const [coordinatesLoading, setCoordinatesLoading] = useState(true);
  const [coordinatesError, setCoordinatesError] = useState(null);
  const [appeals, setAppeals] = useState([]);
  const [appealsLoading, setAppealsLoading] = useState(false);
  const [appealsError, setAppealsError] = useState(null);
  const [mapCenter, setMapCenter] = useState(DEFAULT_COORDINATES);
  const [selectedAppeal, setSelectedAppeal] = useState(null);
  const [appealSummary, setAppealSummary] = useState(null);
  const [summaryLoading, setSummaryLoading] = useState(false);
  const mapRef = useRef(null);
  const appealsFetchedRef = useRef(false);
  const fetchedDistrictsRef = useRef(new Set());
  const centerSetRef = useRef(false);

  useEffect(() => {
    if (centerSetRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) {
      return;
    }

    const storedDistrict = user?.district || localStorage.getItem('userDistrict');
    let initialCenter = DEFAULT_COORDINATES;
    if (storedDistrict && districtCoordinates[storedDistrict]) {
      initialCenter = districtCoordinates[storedDistrict];
    }

    setMapCenter(initialCenter);
    centerSetRef.current = true;
  }, [districtCoordinates, user]);

  useEffect(() => {
    const fetchCoordinates = async () => {
      setCoordinatesLoading(true);
      const result = await AddressService.getAllDistrictCoordinates();

      if (result.success) {
        setDistrictCoordinates(result.coordinates);
        setCoordinatesError(null);
      } else {
        setCoordinatesError(result.errors?.[0] || 'Failed to load coordinates');
      }
      setCoordinatesLoading(false);
    };

    fetchCoordinates();
  }, []);

  const isDistrictVisible = (districtCoord, bounds) => {
    if (!bounds || !districtCoord) return false;
    return bounds.contains(new window.google.maps.LatLng(districtCoord.lat, districtCoord.lng));
  };

  const getVisibleDistricts = () => {
    if (!mapRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) return [];
    const bounds = mapRef.current.getBounds();
    if (!bounds) return [];
    return Object.keys(districtCoordinates).filter(districtName => isDistrictVisible(districtCoordinates[districtName], bounds));
  };

  useEffect(() => {
    if (!mapLoaded || Object.keys(districtCoordinates).length === 0) return;

    const fetchAppealsForVisibleDistricts = async () => {
      const visibleDistricts = getVisibleDistricts();
      if (visibleDistricts.length === 0) {
        // Keep existing markers during transient map states (e.g. while bounds are recalculating)
        return;
      }

      const newDistricts = visibleDistricts.filter(district => !fetchedDistrictsRef.current.has(district));
      if (newDistricts.length === 0) return;

      setAppealsLoading(true);
      newDistricts.forEach(district => fetchedDistrictsRef.current.add(district));

      const result = await AppealService.getAppealLocationsFromMultipleDistricts(visibleDistricts);
      if (result.success) {
        setAppeals(result.appeals);
        setAppealsError(null);
      } else {
        setAppealsError(result.errors?.[0] || 'Failed to load appeals');
      }
      setAppealsLoading(false);
    };

    if (!appealsFetchedRef.current) {
      fetchAppealsForVisibleDistricts();
      appealsFetchedRef.current = true;
    }
  }, [mapLoaded, districtCoordinates]);

  const handleMapMove = () => {
    if (!mapRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) return;

    const currentCenter = mapRef.current.getCenter();
    if (currentCenter) {
      setMapCenter({
        lat: currentCenter.lat(),
        lng: currentCenter.lng()
      });
    }

    const visibleDistricts = getVisibleDistricts();
    if (visibleDistricts.length === 0) return;
    const newDistricts = visibleDistricts.filter(district => !fetchedDistrictsRef.current.has(district));

    if (newDistricts.length > 0) {
      const fetchNewAppeals = async () => {
        setAppealsLoading(true);
        newDistricts.forEach(district => fetchedDistrictsRef.current.add(district));
        const result = await AppealService.getAppealLocationsFromMultipleDistricts(visibleDistricts);
        if (result.success) {
          setAppeals(result.appeals);
          setAppealsError(null);
        } else {
          setAppealsError(result.errors?.[0] || 'Failed to load appeals');
        }
        setAppealsLoading(false);
      };
      fetchNewAppeals();
    }
  };

  const mapOptions = {
    zoom: 12,
    mapTypeId: 'roadmap',
    fullscreenControl: true,
    zoomControl: true,
    streetViewControl: false,
    styles: [
      { featureType: 'administrative', elementType: 'labels.text.fill', stylers: [{ color: '#444444' }] },
      { featureType: 'landscape', elementType: 'all', stylers: [{ color: '#f2f2f2' }] },
      { featureType: 'poi', elementType: 'all', stylers: [{ visibility: 'off' }] },
      { featureType: 'road', elementType: 'all', stylers: [{ saturation: -100 }, { lightness: 45 }] },
      { featureType: 'road.highway', elementType: 'all', stylers: [{ visibility: 'simplified' }] },
      { featureType: 'road.arterial', elementType: 'labels.icon', stylers: [{ visibility: 'off' }] },
      { featureType: 'transit', elementType: 'all', stylers: [{ visibility: 'off' }] },
      { featureType: 'water', elementType: 'all', stylers: [{ color: '#46bcda' }, { visibility: 'on' }] }
    ]
  };

  const handleMapLoad = (map) => {
    mapRef.current = map;
    setMapLoaded(true);
    setMapError(null);
  };

  const handleMapError = (error) => {
    setMapError('Failed to load Google Maps. Please check your API key and browser console.');
  };

  const handleMarkerClick = async (appeal) => {
    setSelectedAppeal(appeal);
    setSummaryLoading(true);

    const result = await AppealService.getAppealSummary(appeal.appealId);
    if (result.success) {
      setAppealSummary(result.appeal);
    } else {
      setAppealSummary(null);
    }
    setSummaryLoading(false);
  };

  const closeAppealDialog = () => {
    setSelectedAppeal(null);
    setAppealSummary(null);
  };

  const hasApiKey = process.env.REACT_APP_GOOGLE_MAPS_API_KEY && process.env.REACT_APP_GOOGLE_MAPS_API_KEY.trim() !== '';

  if (!hasApiKey) {
    return (
        <div className="city-map-container">
          <div className="map-error">
            <div className="error-content">
              <p><strong>Google Maps API key not configured</strong></p>
              <p>Please add your Google Maps API key to the .env file.</p>
            </div>
          </div>
        </div>
    );
  }

  return (
      <div className="city-map-container">
        {mapError && (
            <div className="map-error">
              <div className="error-content"><p><strong>Map Loading Error</strong></p><p>{mapError}</p></div>
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
                  center={mapCenter || DEFAULT_COORDINATES}
                  zoom={12}
                  options={mapOptions}
                  onLoad={handleMapLoad}
                  onError={handleMapError}
                  onIdle={handleMapMove}
              >
                {appeals.map((appeal) => {
                  let iconUrl = appeal.categoryIconUrl;
                  if (iconUrl && !iconUrl.startsWith('http')) {
                    const cleanPath = iconUrl.startsWith('/') ? iconUrl.slice(1) : iconUrl;
                    iconUrl = `${process.env.PUBLIC_URL || ''}/${cleanPath}`;
                  }

                  return (
                      <Marker
                          key={appeal.appealId}
                          position={{ lat: parseFloat(appeal.latitude), lng: parseFloat(appeal.longitude) }}
                          title={appeal.appealId}
                          icon={{
                            url: iconUrl,
                            scaledSize: new window.google.maps.Size(32, 32),
                            origin: new window.google.maps.Point(0, 0),
                            anchor: new window.google.maps.Point(16, 32)
                          }}
                          onClick={() => handleMarkerClick(appeal)}
                      />
                  );
                })}
              </GoogleMap>

              {!mapLoaded && (
                  <div className="map-loading-overlay">
                    <div className="loading-spinner"></div>
                    <p>Loading map...</p>
                  </div>
              )}
            </>
        )}

        {selectedAppeal && (
            <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0, 0, 0, 0.5)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000 }} onClick={closeAppealDialog}>
              <div style={{ backgroundColor: 'white', borderRadius: '8px', padding: '30px', maxWidth: '500px', width: '90%', maxHeight: '80vh', overflowY: 'auto', boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)', position: 'relative' }} onClick={(e) => e.stopPropagation()}>
                <button onClick={closeAppealDialog} style={{ position: 'absolute', top: '15px', right: '15px', backgroundColor: '#f0f0f0', border: 'none', borderRadius: '4px', padding: '8px 12px', cursor: 'pointer', fontSize: '18px', fontWeight: 'bold', color: '#666' }}>×</button>
                <h2 style={{ marginTop: 0, marginBottom: '20px' }}>Appeal Details</h2>

                {summaryLoading ? (
                    <div style={{ textAlign: 'center', padding: '20px' }}><p>Loading appeal details...</p></div>
                ) : appealSummary ? (
                    <>
                      <div style={{ marginBottom: '20px' }}>
                        <h3 style={{ marginTop: 0, marginBottom: '15px', fontSize: '18px', color: '#333' }}>{appealSummary.title}</h3>
                        <div style={{ marginBottom: '10px' }}>
                          <strong style={{ display: 'block', marginBottom: '4px', color: '#333' }}>Description:</strong>
                          <p style={{ margin: 0, color: '#555', fontSize: '14px', lineHeight: '1.4' }}>{appealSummary.description}</p>
                        </div>
                        <div style={{ marginBottom: '10px', fontSize: '14px' }}>
                          <strong style={{ color: '#333' }}>Reported by: </strong><span style={{ color: '#555' }}>{appealSummary.userFullName}</span>
                        </div>
                        <div style={{ marginBottom: '10px', fontSize: '14px' }}>
                          <strong style={{ color: '#333' }}>Date: </strong><span style={{ color: '#555' }}>{appealSummary.date}</span>
                        </div>
                        <div style={{ marginBottom: '20px', fontSize: '14px' }}>
                          <strong style={{ color: '#333' }}>Status: </strong>
                          <span style={{ fontWeight: 'bold', color: appealSummary.status === 'PENDING' ? '#f39c12' : appealSummary.status === 'RESOLVED' ? '#27ae60' : '#2980b9' }}>
                        {appealSummary.status}
                      </span>
                        </div>
                      </div>

                      <button
                          onClick={() => {
                            closeAppealDialog(); // Close modal when navigating
                            onViewDetails(selectedAppeal.appealId);
                          }}
                          style={{ backgroundColor: '#0066cc', color: 'white', border: 'none', padding: '10px 20px', borderRadius: '4px', cursor: 'pointer', fontSize: '14px', fontWeight: 'bold', width: '100%' }}
                      >
                        View Full Details
                      </button>
                    </>
                ) : (
                    <p style={{ color: '#d32f2f' }}>Failed to load appeal summary</p>
                )}
              </div>
            </div>
        )}
      </div>
  );
}

export default CityMap;