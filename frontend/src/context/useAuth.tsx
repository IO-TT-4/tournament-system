import React, { createContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router';

import { loginAPI, registerApi } from '../services/AuthService';
import { toast } from 'react-toastify';
import axios from 'axios';
import { useTranslation } from 'react-i18next';
import { handleError } from '../helpers/ErrorHandler';

interface User {
  username: string;
  email: string;
  id: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  loginUser: (username: string, password: string) => Promise<void>;
  registerUser: (email: string, username: string, password: string) => Promise<void>;
  logout: () => void;
  isLoggedIn: () => boolean;
}

const UserContext = createContext<AuthContextType | undefined>(undefined);

export function UserProvider({ children }: { children: React.ReactNode }) {
  const { t } = useTranslation('toast');

  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    const userStr = localStorage.getItem('user');
    const tokenStr = localStorage.getItem('accessToken');

    if (userStr && tokenStr) {
      setUser(JSON.parse(userStr));
      setToken(tokenStr);
      axios.defaults.headers.common['Authorization'] = `Bearer ${tokenStr}`;
    }

    setIsReady(true);
  }, []);

  async function registerUser(
    email: string,
    username: string,
    password: string,
  ) {
    await registerApi(email, username, password)
      .then((res) => {
        if (res) {
          localStorage.setItem('accessToken', res?.data.accessToken);
          const userObj = {
            username: res?.data.username,
            email: res?.data.email,
            id: res?.data.id,
          };
          localStorage.setItem('user', JSON.stringify(userObj));
          setToken(res?.data.accessToken);
          setUser(userObj);
          toast.success(`${t('registerSuccess')}!`);
          navigate('/');
        }
      })
      .catch((e) => {
        handleError(e);
      });
  }

  async function loginUser(username: string, password: string) {
    await loginAPI(username, password)
      .then((res) => {
        if (res) {
          localStorage.setItem('accessToken', res?.data.accessToken);
          const userObj = {
            username: res?.data.username,
            email: res?.data.email,
            id: res?.data.id,
          };
          localStorage.setItem('user', JSON.stringify(userObj));
          setToken(res?.data.accessToken);
          setUser(userObj);
          toast.success(`${t('loginSuccess')}!`);
          navigate('/');
        }
      })
      .catch((e) => {
        handleError(e);
      });
  }

  function isLoggedIn() {
    return !!user;
  }

  function logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
    setUser(null);
    setToken(null);
    delete axios.defaults.headers.common['Authorization'];
    navigate('/');
    toast.success(t('logoutSuccess'));
  }

  return (
    <UserContext.Provider
      value={{ loginUser, user, token, logout, isLoggedIn, registerUser }}>
      {isReady ? children : null}
    </UserContext.Provider>
  );
}

export function useAuth() {
  const context = React.useContext(UserContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within a UserProvider');
  }
  return context;
}
