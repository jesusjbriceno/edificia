import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import type { ProblemDetails, ValidationProblemDetails } from '@/lib/types';

// ─── API Error class ─────────────────────────────────────

export class ApiError extends Error {
  status: number;
  code?: string;
  validationErrors?: { property: string; message: string }[];

  constructor(status: number, message: string, code?: string, validationErrors?: { property: string; message: string }[]) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.validationErrors = validationErrors;
  }
}

// ─── Client factory ──────────────────────────────────────

const API_BASE_URL = (typeof import.meta !== 'undefined' && import.meta.env?.PUBLIC_API_URL)
  || 'http://localhost:5000/api';

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

// ─── Request interceptor: inject JWT ─────────────────────

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  // Dynamic import to avoid circular deps at module init
  const { useAuthStore } = require('@/store/useAuthStore');
  const { accessToken } = useAuthStore.getState();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

// ─── Response interceptor: refresh + error normalisation ─

let isRefreshing = false;
let pendingQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null) {
  pendingQueue.forEach(({ resolve, reject }) => {
    error ? reject(error) : resolve(token!);
  });
  pendingQueue = [];
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ProblemDetails | ValidationProblemDetails>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // ── 401 → try token refresh ──
    if (error.response?.status === 401 && !originalRequest._retry) {
      const { useAuthStore } = require('@/store/useAuthStore');
      const { refreshToken, setTokens, logout } = useAuthStore.getState();

      if (!refreshToken) {
        logout();
        if (typeof window !== 'undefined') window.location.href = '/';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        // Queue concurrent requests while refresh is in flight
        return new Promise<string>((resolve, reject) => {
          pendingQueue.push({ resolve, reject });
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return apiClient(originalRequest);
        });
      }

      isRefreshing = true;
      originalRequest._retry = true;

      try {
        const { data } = await axios.post(`${API_BASE_URL}/auth/refresh`, { refreshToken });
        const newAccess: string = data.accessToken;
        const newRefresh: string | null = data.refreshToken;
        setTokens(newAccess, newRefresh);
        processQueue(null, newAccess);
        originalRequest.headers.Authorization = `Bearer ${newAccess}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        logout();
        if (typeof window !== 'undefined') window.location.href = '/';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    // ── Normalise API errors ──
    if (error.response) {
      const { status, data } = error.response;
      const problem = data as ProblemDetails | ValidationProblemDetails | undefined;

      const detail = problem?.detail || problem?.title || error.message;
      const code = problem?.code;
      const validationErrors = (problem as ValidationProblemDetails)?.errors;

      return Promise.reject(new ApiError(status, detail, code, validationErrors));
    }

    // Network / timeout errors
    return Promise.reject(new ApiError(0, error.message || 'Error de red'));
  },
);

export default apiClient;
