import React, { useState, useRef, useEffect } from 'react';
import { GoogleMap, Marker } from '@react-google-maps/api';
import AddressService from '../services/AddressService';
import AppealService from '../services/AppealService';
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
  const [appeals, setAppeals] = useState([]);
  const [appealsLoading, setAppealsLoading] = useState(false);
  const [appealsError, setAppealsError] = useState(null);
  const [mapCenter, setMapCenter] = useState(null);
  const [selectedAppeal, setSelectedAppeal] = useState(null);
  const [appealSummary, setAppealSummary] = useState(null);
  const [summaryLoading, setSummaryLoading] = useState(false);
  const mapRef = useRef(null);
  const appealsFetchedRef = useRef(false);
  const fetchedDistrictsRef = useRef(new Set());
  const centerSetRef = useRef(false);

  // ...existing code...

  // Set the center only once on initial load
  useEffect(() => {
    if (centerSetRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) {
      return;
    }

    console.log('🗺️ CityMap user object:', user);
    console.log('🗺️ User district (from user.district):', user?.district);
    console.log('🗺️ Available district coordinates:', districtCoordinates);

    // Check localStorage as fallback
    const storedDistrict = user?.district || localStorage.getItem('userDistrict');
    console.log('🗺️ Final district to use:', storedDistrict);
    console.log('🗺️ localStorage.userDistrict:', localStorage.getItem('userDistrict'));

    let initialCenter = DEFAULT_COORDINATES;
    if (storedDistrict && districtCoordinates[storedDistrict]) {
      const coordinates = districtCoordinates[storedDistrict];
      console.log('✅ Map centering on district:', storedDistrict, coordinates);
      initialCenter = coordinates;
    } else {
      console.log('⚠️ Map defaulting to Kyiv (district not found)');
    }

    setMapCenter(initialCenter);
    centerSetRef.current = true;
  }, [districtCoordinates]);

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

  // Helper function to check if a point is within map bounds
  const isDistrictVisible = (districtCoord, bounds) => {
    if (!bounds || !districtCoord) return false;
    return bounds.contains(new window.google.maps.LatLng(districtCoord.lat, districtCoord.lng));
  };

  // Helper function to get all visible districts in current map bounds
  const getVisibleDistricts = () => {
    if (!mapRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) {
      return [];
    }

    const bounds = mapRef.current.getBounds();
    if (!bounds) return [];

    return Object.keys(districtCoordinates).filter(districtName =>
      isDistrictVisible(districtCoordinates[districtName], bounds)
    );
  };

  // Fetch appeals when map is loaded or moved
  useEffect(() => {
    if (!mapLoaded || Object.keys(districtCoordinates).length === 0) return;

    const fetchAppealsForVisibleDistricts = async () => {
      const visibleDistricts = getVisibleDistricts();

      if (visibleDistricts.length === 0) {
        console.log('No districts visible in current map view');
        setAppeals([]);
        return;
      }

      // Find new districts that we haven't fetched yet
      const newDistricts = visibleDistricts.filter(
        district => !fetchedDistrictsRef.current.has(district)
      );

      if (newDistricts.length === 0) {
        console.log('All visible districts already fetched');
        return;
      }

      console.log('Fetching appeals for new visible districts:', newDistricts);
      setAppealsLoading(true);

      // Add new districts to the fetched set
      newDistricts.forEach(district => fetchedDistrictsRef.current.add(district));

      // Fetch appeals from all visible districts (including previously fetched ones)
      const result = await AppealService.getAppealLocationsFromMultipleDistricts(visibleDistricts);

       if (result.success) {
         console.log('✅ Appeals loaded for districts:', visibleDistricts);
         console.log('📍 Total appeals received:', result.appeals.length);
         result.appeals.forEach((appeal, index) => {
           console.log(`   [${index}] ID: ${appeal.appealId}, Lat: ${appeal.latitude}, Lng: ${appeal.longitude}, Icon: ${appeal.categoryIconUrl}`);
         });
         setAppeals(result.appeals);
         setAppealsError(null);
       } else {
         console.warn('❌ Failed to fetch appeals:', result.errors);
         setAppealsError(result.errors?.[0] || 'Failed to load appeals');
       }
       setAppealsLoading(false);
    };

    // Only fetch if this is the initial load
    if (!appealsFetchedRef.current) {
      fetchAppealsForVisibleDistricts();
      appealsFetchedRef.current = true;
    }
  }, [mapLoaded, districtCoordinates]);

  // Handle map movements (drag, zoom) to refetch appeals for newly visible districts
  const handleMapMove = () => {
    if (!mapRef.current || !districtCoordinates || Object.keys(districtCoordinates).length === 0) return;

    const visibleDistricts = getVisibleDistricts();
    const newDistricts = visibleDistricts.filter(
      district => !fetchedDistrictsRef.current.has(district)
    );

    if (newDistricts.length > 0) {
      console.log('Map moved, fetching appeals for new districts:', newDistricts);

      const fetchNewAppeals = async () => {
        setAppealsLoading(true);

        // Add new districts to the fetched set
        newDistricts.forEach(district => fetchedDistrictsRef.current.add(district));

        // Fetch appeals from all visible districts
        const result = await AppealService.getAppealLocationsFromMultipleDistricts(visibleDistricts);

         if (result.success) {
           console.log('✅ Appeals updated with new districts:', newDistricts);
           console.log('📍 Total appeals after update:', result.appeals.length);
           result.appeals.forEach((appeal, index) => {
             console.log(`   [${index}] ID: ${appeal.appealId}, Lat: ${appeal.latitude}, Lng: ${appeal.longitude}, Icon: ${appeal.categoryIconUrl}`);
           });
           setAppeals(result.appeals);
           setAppealsError(null);
         } else {
           console.warn('❌ Failed to fetch appeals for new districts:', result.errors);
           setAppealsError(result.errors?.[0] || 'Failed to load appeals');
         }
         setAppealsLoading(false);
      };

      fetchNewAppeals();
    }
  };



  const mapOptions = {
    zoom: 12,
    center: mapCenter || DEFAULT_COORDINATES,
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

  // Handle marker click to show appeal details
  const handleMarkerClick = async (appeal) => {
    console.log('📍 Marker clicked for appeal:', appeal.appealId);
    setSelectedAppeal(appeal);
    setSummaryLoading(true);
    
    const result = await AppealService.getAppealSummary(appeal.appealId);
    if (result.success) {
      console.log('✅ Appeal summary loaded:', result.appeal);
      setAppealSummary(result.appeal);
    } else {
      console.warn('❌ Failed to load appeal summary:', result.errors);
      setAppealSummary(null);
    }
    setSummaryLoading(false);
  };

  // Close the appeal dialog
  const closeAppealDialog = () => {
    setSelectedAppeal(null);
    setAppealSummary(null);
  };

  // ...existing code...

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
              center={mapCenter || DEFAULT_COORDINATES}
              zoom={12}
              options={mapOptions}
              onLoad={handleMapLoad}
              onError={handleMapError}
              onDragEnd={handleMapMove}
              onZoomChanged={handleMapMove}
             >

                {/* Custom markers for appeals with icons */}
                {appeals.map((appeal) => {
                  // Construct absolute icon URL
                  let iconUrl = appeal.categoryIconUrl;

                  // Convert relative path to absolute URL if needed
                  if (iconUrl && !iconUrl.startsWith('http')) {
                    // Remove leading slash if present
                    const cleanPath = iconUrl.startsWith('/') ? iconUrl.slice(1) : iconUrl;
                    iconUrl = `${process.env.PUBLIC_URL || ''}/${cleanPath}`;
                  }

                  return (
                  <Marker
                    key={appeal.appealId}
                    position={{
                      lat: parseFloat(appeal.latitude),
                      lng: parseFloat(appeal.longitude)
                    }}
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

       {/* Appeal Summary Dialog */}
       {selectedAppeal && (
         <div style={{
           position: 'fixed',
           top: 0,
           left: 0,
           right: 0,
           bottom: 0,
           backgroundColor: 'rgba(0, 0, 0, 0.5)',
           display: 'flex',
           justifyContent: 'center',
           alignItems: 'center',
           zIndex: 1000
         }} onClick={closeAppealDialog}>
           <div style={{
             backgroundColor: 'white',
             borderRadius: '8px',
             padding: '30px',
             maxWidth: '500px',
             width: '90%',
             maxHeight: '80vh',
             overflowY: 'auto',
             boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)',
             position: 'relative'
           }} onClick={(e) => e.stopPropagation()}>

             {/* Close button */}
             <button
               onClick={closeAppealDialog}
               style={{
                 position: 'absolute',
                 top: '15px',
                 right: '15px',
                 backgroundColor: '#f0f0f0',
                 border: 'none',
                 borderRadius: '4px',
                 padding: '8px 12px',
                 cursor: 'pointer',
                 fontSize: '18px',
                 fontWeight: 'bold',
                 color: '#666'
               }}
             >
               ×
             </button>

              {/* Appeal Header */}
              <h2 style={{ marginTop: 0, marginBottom: '20px' }}>Appeal Details</h2>

              {/* Loading state */}
              {summaryLoading ? (
                <div style={{ textAlign: 'center', padding: '20px' }}>
                  <p>Loading appeal details...</p>
                </div>
              ) : appealSummary ? (
                <>
                  {/* Summary Content */}
                  <div style={{ marginBottom: '20px' }}>
                    {appealSummary.problemName && (
                      <div style={{ marginBottom: '15px' }}>
                        <strong style={{ fontSize: '16px', color: '#333' }}>Problem:</strong>
                        <p style={{ color: '#444', margin: '8px 0 0 0', fontSize: '15px' }}>
                          {appealSummary.problemName}
                        </p>
                      </div>
                    )}
                    {appealSummary.description && (
                      <div style={{ marginBottom: '15px' }}>
                        <strong style={{ fontSize: '16px', color: '#333' }}>Description:</strong>
                        <p style={{ color: '#555', margin: '8px 0 0 0', fontSize: '14px', lineHeight: '1.5' }}>
                          {appealSummary.description}
                        </p>
                      </div>
                    )}
                    {appealSummary.status && (
                      <div style={{ marginBottom: '15px' }}>
                        <strong style={{ fontSize: '16px', color: '#333' }}>Status:</strong>
                        <p style={{
                          color: '#0066cc',
                          margin: '8px 0 0 0',
                          fontSize: '15px',
                          fontWeight: '500'
                        }}>
                          {appealSummary.status}
                        </p>
                      </div>
                    )}
                    {appealSummary.datePublished && (
                      <div style={{ marginBottom: '15px' }}>
                        <strong style={{ fontSize: '16px', color: '#333' }}>Published:</strong>
                        <p style={{ color: '#777', margin: '8px 0 0 0', fontSize: '14px' }}>
                          {new Date(appealSummary.datePublished).toLocaleDateString('en-US', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                          })}
                        </p>
                      </div>
                    )}
                  </div>

                 {/* Details Button */}
                 <button
                   style={{
                     backgroundColor: '#0066cc',
                     color: 'white',
                     border: 'none',
                     padding: '10px 20px',
                     borderRadius: '4px',
                     cursor: 'pointer',
                     fontSize: '14px',
                     fontWeight: 'bold',
                     width: '100%'
                   }}
                   onMouseOver={(e) => e.target.style.backgroundColor = '#0052a3'}
                   onMouseOut={(e) => e.target.style.backgroundColor = '#0066cc'}
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

