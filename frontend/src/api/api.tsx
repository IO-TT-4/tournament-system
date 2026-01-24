import axios from 'axios';

export const api = axios.create({ baseURL: import.meta.env.VITE_API_URL });

api.interceptors.request.use(
  (config) => {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      if (user?.token) {
        config.headers.Authorization = `Bearer ${user.token}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Prevent infinite loops
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const userString = localStorage.getItem('user');
        if (!userString) return Promise.reject(error);
        
        const user = JSON.parse(userString);
        if (!user?.refreshToken) return Promise.reject(error);

        const response = await axios.post(`${import.meta.env.VITE_API_URL}/auth/refresh`, {
          refreshToken: user.refreshToken,
        });

        if (response.data) {
          user.token = response.data.token;
          user.refreshToken = response.data.refreshToken;
          localStorage.setItem('user', JSON.stringify(user));

          // Update default header for future requests
          api.defaults.headers.common['Authorization'] = `Bearer ${user.token}`;
          originalRequest.headers['Authorization'] = `Bearer ${user.token}`;
          
          return api(originalRequest);
        }
      } catch (refreshError) {
        // If refresh fails, logout
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);
