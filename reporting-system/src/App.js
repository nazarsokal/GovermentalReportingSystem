import React, { useState, useEffect } from 'react';
import './App.css';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import AuthService from './services/AuthService';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

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

  const handleLogin = (userData) => {
    setIsLoggedIn(true);
    setUser(userData);
  };

  const handleLogout = () => {
    AuthService.logout();
    setIsLoggedIn(false);
    setUser(null);
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
      {!isLoggedIn ? (
        <LoginPage onLogin={handleLogin} />
      ) : (
        <DashboardPage user={user} onLogout={handleLogout} />
      )}
    </div>
  );
}

export default App;
