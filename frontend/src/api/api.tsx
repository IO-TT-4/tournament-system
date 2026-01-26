import axios from 'axios';

export const api = axios.create({ baseURL: import.meta.env.VITE_API_URL });

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
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
        // We might need to store refreshToken separately too if it's not in user object, 
        // but assuming for now it might be or we need to fix login to store it.
        // Looking at useAuth, 'user' object only has {username, email, id}.
        // We probably need to store refreshToken in localStorage as well to make this work.
        // For now, let's fix the immediate issue of Authorization header.
        // If refreshToken is missing, we can't refresh.
        
        const refreshToken = localStorage.getItem('refreshToken'); // Assuming we will start storing it
        if (!refreshToken) return Promise.reject(error);

        const response = await axios.post(`${import.meta.env.VITE_API_URL}/auth/refresh`, {
          refreshToken: refreshToken,
        });

        if (response.data) {
          const { token: newToken, refreshToken: newRefreshToken } = response.data;
          
          localStorage.setItem('accessToken', newToken);
          localStorage.setItem('refreshToken', newRefreshToken);

          // Update default header for future requests
          api.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
          originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
          
          return api(originalRequest);
        }
      } catch (refreshError) {
        // If refresh fails, logout
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);
