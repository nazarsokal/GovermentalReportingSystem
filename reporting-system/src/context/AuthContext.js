import React, { createContext, useContext, useState, useEffect } from 'react';
import AuthService from '../services/AuthService';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [loading, setLoading] = useState(true);
  const [accessToken, setAccessToken] = useState(null);

  // Check if user is already logged in on mount
  useEffect(() => {
    const storedUser = AuthService.getUser();
    const token = AuthService.getAccessToken();

    if (storedUser && token) {
      setIsLoggedIn(true);
      setUser(storedUser);
      setAccessToken(token);
    }
    setLoading(false);
  }, []);

  const login = async (email, password, rememberMe = false) => {
    const result = await AuthService.login(email, password, rememberMe);
    if (result.success) {
      setIsLoggedIn(true);
      setUser(result.user);
      setAccessToken(result.accessToken);
    }
    return result;
  };

  const register = async (fullName, email, password, confirmPassword) => {
    const result = await AuthService.register(fullName, email, password, confirmPassword);
    if (result.success) {
      setIsLoggedIn(true);
      setUser(result.user);
      setAccessToken(result.accessToken);
    }
    return result;
  };

  const logout = () => {
    AuthService.logout();
    setIsLoggedIn(false);
    setUser(null);
    setAccessToken(null);
  };

  const value = {
    user,
    isLoggedIn,
    loading,
    accessToken,
    login,
    register,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;

