import axios from 'axios';
import { useAuthStore } from '@/store/useAuthStore';

interface ImportMetaEnv {
  readonly PUBLIC_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

const apiClient = axios.create({
  baseURL: (import.meta as any).env?.PUBLIC_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para añadir el token a las cabeceras
apiClient.interceptors.request.use(
  (config) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor para manejar errores (ej. token expirado)
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    // Si el error es 401 y no hemos reintentado ya
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      const { refreshToken, logout } = useAuthStore.getState();
      
      if (refreshToken) {
        try {
          // Intentar refrescar el token (el endpoint debe existir en la API)
          const response = await axios.post(`${apiClient.defaults.baseURL}/auth/refresh`, {
            refreshToken,
          });
          
          const { accessToken, newRefreshToken } = response.data;
          
          // Actualizar el store directamente
          // useAuthStore.getState().updateTokens(accessToken, newRefreshToken);
          
          apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        } catch (refreshError) {
          logout();
          window.location.href = '/login';
        }
      } else {
        logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;
