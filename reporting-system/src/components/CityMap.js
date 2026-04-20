import React from 'react';
import '../styles/CityMap.css';

function CityMap() {
  return (
    <div className="city-map-container">
      <svg viewBox="0 0 800 400" className="city-map-svg">
        {/* Background */}
        <rect width="800" height="400" fill="#E8F4F8"/>

        {/* Water features */}
        <path d="M 200 100 Q 250 150 280 200 Q 300 250 320 300" stroke="#4A90E2" strokeWidth="20" fill="none"/>
        <path d="M 500 50 Q 550 100 600 200" stroke="#4A90E2" strokeWidth="15" fill="none"/>

        {/* Streets */}
        <line x1="50" y1="150" x2="750" y2="150" stroke="#CCCCCC" strokeWidth="8"/>
        <line x1="50" y1="250" x2="750" y2="250" stroke="#CCCCCC" strokeWidth="8"/>
        <line x1="150" y1="0" x2="150" y2="400" stroke="#CCCCCC" strokeWidth="8"/>
        <line x1="400" y1="0" x2="400" y2="400" stroke="#CCCCCC" strokeWidth="8"/>
        <line x1="650" y1="0" x2="650" y2="400" stroke="#CCCCCC" strokeWidth="8"/>

        {/* Green areas */}
        <circle cx="600" cy="80" r="40" fill="#7BC67B"/>
        <circle cx="150" cy="350" r="50" fill="#7BC67B"/>

        {/* Building blocks */}
        <rect x="80" y="60" width="40" height="40" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="80" y="180" width="40" height="40" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="80" y="300" width="40" height="40" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>

        <rect x="250" y="60" width="45" height="45" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="250" y="180" width="45" height="45" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="250" y="300" width="45" height="45" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>

        <rect x="500" y="280" width="50" height="50" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="500" y="100" width="50" height="50" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>

        <rect x="700" y="60" width="35" height="35" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="700" y="180" width="35" height="35" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>
        <rect x="700" y="300" width="35" height="35" fill="#F5F5F5" stroke="#999" strokeWidth="1"/>

        {/* Infrastructure points (reported issues) */}
        <circle cx="150" cy="150" r="12" fill="#4A90E2" opacity="0.9"/>
        <circle cx="350" cy="250" r="12" fill="#4A90E2" opacity="0.9"/>
        <circle cx="520" cy="180" r="12" fill="#4A90E2" opacity="0.9"/>
        <circle cx="400" cy="100" r="12" fill="#4A90E2" opacity="0.9"/>
        <circle cx="650" cy="280" r="12" fill="#4A90E2" opacity="0.9"/>
      </svg>
      <p className="map-credits">Map data © Powered by Maps API</p>
    </div>
  );
}

export default CityMap;

