import React, { useState, useEffect } from 'react';
import './App.css';
import { LoadScript } from '@react-google-maps/api';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import AuthService from './services/AuthService';
import { googleMapsLibraries } from './constants/googleMaps';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [scriptError, setScriptError] = useState(null);

  // Check if user is already logged in on mount
  useEffect(() => {
    const storedUser = AuthService.getUser();
    const accessToken = AuthService.getAccessToken();

    if (storedUser && accessToken) {
      setIsLoggedIn(true);
      setUser(storedUser);
    }
    setLoading(false);
  }, []);

  const apiKey = process.env.REACT_APP_GOOGLE_MAPS_API_KEY;
  const hasApiKey = apiKey && apiKey.trim() !== '';

  // Log for debugging
  useEffect(() => {
    if (hasApiKey) {
      console.log('Google Maps API Key is configured');
    } else {
      console.warn('Google Maps API Key is NOT configured');
    }
  }, [hasApiKey]);

  const handleLogin = (userData) => {
    setIsLoggedIn(true);
    // Get the user from AuthService which ensures all data is merged
    // (including district/oblast restored from localStorage if needed)
    const userWithAllData = AuthService.getUser();
    console.log('User logged in, full user object:', userWithAllData);
    setUser(userWithAllData || userData);
  };

  const handleLogout = () => {
    AuthService.logout();
    setIsLoggedIn(false);
    setUser(null);
  };

  const handleScriptError = (error) => {
    console.error('LoadScript Error:', error);
    setScriptError(error);
  };

  const handleScriptLoad = () => {
    console.log('Google Maps API loaded successfully');
    setScriptError(null);
  };

  if (loading) {
    return (
      <div className="App">
        <div style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100vh',
          backgroundColor: '#f5f5f5'
        }}>
          <div style={{
            fontSize: '18px',
            color: '#666',
            fontFamily: 'system-ui, -apple-system'
          }}>
            Loading...
          </div>
        </div>
      </div>
    );
  }


  return (
    <div className="App">
      {scriptError && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          padding: '16px',
          background: '#fee2e2',
          color: '#991b1b',
          zIndex: 9999,
          fontSize: '12px'
        }}>
          Google Maps Script Error: {scriptError.message || String(scriptError)}
        </div>
      )}

      {hasApiKey ? (
        <LoadScript
          googleMapsApiKey={apiKey}
          libraries={googleMapsLibraries}
          language="uk"
          region="UA"
          onError={handleScriptError}
          onLoad={handleScriptLoad}
        >
          {!isLoggedIn ? (
            <LoginPage onLogin={handleLogin} />
          ) : (
            <DashboardPage user={user} onLogout={handleLogout} />
          )}
        </LoadScript>
      ) : (
        <>
          {!isLoggedIn ? (
            <LoginPage onLogin={handleLogin} />
          ) : (
            <DashboardPage user={user} onLogout={handleLogout} />
          )}
        </>
      )}
    </div>
  );
}

export default App;
