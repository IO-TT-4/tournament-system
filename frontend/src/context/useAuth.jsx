import React, { createContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router';

import { loginAPI, registerApi } from '../services/AuthService';
import { toast } from 'react-toastify';
import axios from 'axios';
import { useTranslation } from 'react-i18next';
import { handleError } from '../helpers/ErrorHandler';

const UserContex = createContext();

export function UserProvider({ children }) {
  const { t } = useTranslation('toast');

  const navigate = useNavigate();
  const [token, setToken] = useState(null);
  const [user, setUser] = useState(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    const user = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');

    if (user && token) {
      setUser(JSON.parse(user));
      setToken(token);
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }

    setIsReady(true);
  }, []);

  async function registerUser(email, username, password) {
    await registerApi(email, username, password)
      .then((res) => {
        if (res) {
          localStorage.setItem('token', res?.data.accessToken);
          const userObj = {
            username: res?.data.username,
            email: res?.data.email,
            id: res?.data.id,
          };
          localStorage.setItem('user', JSON.stringify(userObj));
          setToken(res?.data.accessToken);
          setUser(userObj);
          const theme = localStorage.getItem('theme');
          toast.success(`${t('loginSuccess')}!`, {
            theme: theme,
          });
          navigate('/');
        }
      })
      .catch((e) => {
        handleError(e);
      });
  }

  async function loginUser(username, password) {
    await loginAPI(username, password)
      .then((res) => {
        if (res) {
          localStorage.setItem('token', res?.data.accessToken);
          const userObj = {
            username: res?.data.username,
            email: res?.data.email,
            id: res?.data.id,
          };
          localStorage.setItem('user', JSON.stringify(userObj));
          setToken(res?.data.accessToken);
          setUser(userObj);
          const theme = localStorage.getItem('theme');
          toast.success(`${t('loginSuccess')}!`, {
            theme: theme,
          });
          navigate('/');
        }
      })
      .catch((e) => {
        const theme = localStorage.getItem('theme');
        handleError(e);
      });
  }

  function isLoggedIn() {
    return !!user;
  }

  function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    setToken(null);
    navigate('/');
    const theme = localStorage.getItem('theme');
    toast.success(t('logoutSuccess'), {
      theme: theme,
    });
  }

  return (
    <UserContex.Provider
      value={{ loginUser, user, token, logout, isLoggedIn, registerUser }}>
      {isReady ? children : null}
    </UserContex.Provider>
  );
}

export function useAuth() {
  return React.useContext(UserContex);
}
