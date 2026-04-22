import React, { useState, useEffect } from 'react';
import '../styles/LoginPage.css';
import AuthService from '../services/AuthService';
import AddressService from '../services/AddressService';

function LoginPage({ onLogin }) {
  const [email, setEmail] = useState('you@example.com');
  const [password, setPassword] = useState('');
  const [fullName, setFullName] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSignIn, setIsSignIn] = useState(true);
  const [rememberMe, setRememberMe] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Address selection state
  const [showAddressSelection, setShowAddressSelection] = useState(false);
  const [oblast, setOblast] = useState('');
  const [district, setDistrict] = useState('');
  const [oblasts, setOblasts] = useState([]);
  const [districts, setDistricts] = useState([]);
  const [addressLoading, setAddressLoading] = useState(false);

  // Store registration data for address selection step
  const [pendingRegistration, setPendingRegistration] = useState(null);

  // Load oblasts on component mount
  useEffect(() => {
    loadOblasts();
  }, []);

  // Load districts when oblast changes
  useEffect(() => {
    if (oblast && showAddressSelection) {
      loadDistricts(oblast);
    } else {
      setDistricts([]);
    }
    setDistrict('');
  }, [oblast, showAddressSelection]);

  const loadOblasts = async () => {
    setAddressLoading(true);
    const result = await AddressService.getOblasts();
    if (result.success) {
      setOblasts(result.oblasts);
    } else {
      setError(result.errors?.[0] || 'Failed to load regions');
    }
    setAddressLoading(false);
  };

  const loadDistricts = async (selectedOblast) => {
    setAddressLoading(true);
    const result = await AddressService.getDistricts(selectedOblast);
    if (result.success) {
      setDistricts(result.districts);
    } else {
      setError(result.errors?.[0] || 'Failed to load districts');
    }
    setAddressLoading(false);
  };

  const handleSignIn = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    if (!email || !password) {
      setError('Please fill in all fields');
      setLoading(false);
      return;
    }

    console.log('🔐 Logging in with:', email);
    const result = await AuthService.login(email, password, rememberMe);
    console.log('🔐 Login result:', result);
    console.log('🔐 User returned from login:', result.user);
    console.log('🔐 localStorage.userDistrict after login:', localStorage.getItem('userDistrict'));

    if (result.success) {
      setSuccess('Login successful!');
      setTimeout(() => {
        onLogin(result.user);
      }, 500);
    } else {
      setError(result.errors?.[0] || result.message || 'Login failed');
    }
    setLoading(false);
  };

  const handleGoogleSignIn = () => {
    // Placeholder for Google sign-in logic
    // In a real implementation, this would authenticate with Google
    // For now, we'll show a message that after Google auth, user needs to select address
    setError('');
    setSuccess('');
    setShowAddressSelection(true);
    setOblast('');
    setDistrict('');
    // In a real implementation, you would store the Google user data in pendingRegistration
    // setPendingRegistration({ ... google user data ... });
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    if (!email || !password || !fullName || !confirmPassword) {
      setError('Please fill in all fields');
      setLoading(false);
      return;
    }

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      setLoading(false);
      return;
    }

    // Store registration data for the address selection step
    setPendingRegistration({
      fullName,
      email,
      password,
      confirmPassword,
    });

    setSuccess('Please select your location to complete registration.');
    // Show address selection instead of immediately logging in
    setShowAddressSelection(true);
    setOblast('');
    setDistrict('');
    setLoading(false);
  };

  const handleAddressSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!oblast || !district) {
      setError('Please select both region and district');
      return;
    }

    if (!pendingRegistration) {
      setError('Registration data not found');
      return;
    }

    setLoading(true);
    const result = await AuthService.register(
      pendingRegistration.fullName,
      pendingRegistration.email,
      pendingRegistration.password,
      pendingRegistration.confirmPassword,
      district,
      oblast
    );

    if (result.success) {
      setSuccess('Registration and address saved! Logging in...');
      setPendingRegistration(null);
      setTimeout(() => {
        onLogin(result.user);
      }, 500);
    } else {
      setError(result.errors?.[0] || result.message || 'Failed to complete registration');
    }
    setLoading(false);
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="logo-section">
          <div className="logo-circle">
            <svg viewBox="0 0 24 24" className="logo-icon">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
          </div>
        </div>

        <h1>Urban Infrastructure Monitor</h1>
        <p className="subtitle">Report and track city infrastructure problems</p>

        {showAddressSelection ? (
          <>
            <h2 style={{ fontSize: '18px', marginBottom: '20px' }}>Select Your Location</h2>
            <form onSubmit={handleAddressSubmit} className="login-form">
              {error && <div className="alert alert-error">{error}</div>}
              {success && <div className="alert alert-success">{success}</div>}

              <div className="form-group">
                <label>Region (Oblast)</label>
                <div className="input-wrapper">
                  <svg className="input-icon" viewBox="0 0 24 24">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z" fill="currentColor"/>
                  </svg>
                  <select
                    value={oblast}
                    onChange={(e) => setOblast(e.target.value)}
                    className="form-input"
                    disabled={loading || addressLoading}
                  >
                    <option value="">-- Select Region --</option>
                    {oblasts.map((obl) => (
                      <option key={obl} value={obl}>
                        {obl}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="form-group">
                <label>District</label>
                <div className="input-wrapper">
                  <svg className="input-icon" viewBox="0 0 24 24">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z" fill="currentColor"/>
                  </svg>
                  <select
                    value={district}
                    onChange={(e) => setDistrict(e.target.value)}
                    className="form-input"
                    disabled={!oblast || loading || addressLoading}
                  >
                    <option value="">-- Select District --</option>
                    {districts.map((dist) => (
                      <option key={dist} value={dist}>
                        {dist}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <button type="submit" className="signin-button" disabled={loading || !oblast || !district}>
                {loading ? 'Saving Address...' : 'Continue'}
              </button>
            </form>
          </>
        ) : (
          <>
            <div className="tab-section">
              <button
                className={`tab-button ${isSignIn ? 'active' : ''}`}
                onClick={() => setIsSignIn(true)}
              >
                Sign In
              </button>
              <button
                className={`tab-button ${!isSignIn ? 'active' : ''}`}
                onClick={() => setIsSignIn(false)}
              >
                Register
              </button>
            </div>

            {isSignIn ? (
              <form onSubmit={handleSignIn} className="login-form">
                {error && <div className="alert alert-error">{error}</div>}
                {success && <div className="alert alert-success">{success}</div>}

                <div className="form-group">
                  <label>Email</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z" fill="currentColor"/>
                    </svg>
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="you@example.com"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Password</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 2.02c2.05.02 3.98.71 5.57 1.98H6.43c1.59-1.27 3.52-1.96 5.57-1.98zm0 14.48c-3.63-.91-6-4.05-6-7.5v-4.1h12v4.1c0 3.45-2.37 6.59-6 7.5z" fill="currentColor"/>
                    </svg>
                    <input
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      placeholder="••••••••"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <div className="form-group checkbox-group">
                  <input
                    type="checkbox"
                    id="remember"
                    checked={rememberMe}
                    onChange={(e) => setRememberMe(e.target.checked)}
                    disabled={loading}
                  />
                  <label htmlFor="remember">Remember me</label>
                </div>

                <button type="submit" className="signin-button" disabled={loading}>
                  {loading ? 'Signing In...' : 'Sign In'}
                </button>

                <div className="divider">Or continue with</div>

                <button type="button" className="google-button" onClick={handleGoogleSignIn} disabled={loading}>
                  <svg viewBox="0 0 24 24" className="google-icon">
                    <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
                    <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
                    <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
                    <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
                  </svg>
                  Sign in with Google
                </button>
              </form>
            ) : (
              <form onSubmit={handleRegister} className="login-form">
                {error && <div className="alert alert-error">{error}</div>}
                {success && <div className="alert alert-success">{success}</div>}

                <div className="form-group">
                  <label>Full Name</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" fill="currentColor"/>
                    </svg>
                    <input
                      type="text"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      placeholder="John Doe"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Email</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z" fill="currentColor"/>
                    </svg>
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="you@example.com"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Password</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 2.02c2.05.02 3.98.71 5.57 1.98H6.43c1.59-1.27 3.52-1.96 5.57-1.98zm0 14.48c-3.63-.91-6-4.05-6-7.5v-4.1h12v4.1c0 3.45-2.37 6.59-6 7.5z" fill="currentColor"/>
                    </svg>
                    <input
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      placeholder="••••••••"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Confirm Password</label>
                  <div className="input-wrapper">
                    <svg className="input-icon" viewBox="0 0 24 24">
                      <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 2.02c2.05.02 3.98.71 5.57 1.98H6.43c1.59-1.27 3.52-1.96 5.57-1.98zm0 14.48c-3.63-.91-6-4.05-6-7.5v-4.1h12v4.1c0 3.45-2.37 6.59-6 7.5z" fill="currentColor"/>
                    </svg>
                    <input
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      placeholder="••••••••"
                      className="form-input"
                      disabled={loading}
                    />
                  </div>
                </div>

                <button type="submit" className="signin-button" disabled={loading}>
                  {loading ? 'Creating Account...' : 'Create Account'}
                </button>
              </form>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default LoginPage;

