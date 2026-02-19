import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useAuthStore } from '@/store/useAuthStore';
import type { LoginResponse } from '@/lib/types';

// Mock de localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => { store[key] = value.toString(); },
    removeItem: (key: string) => { delete store[key]; },
    clear: () => { store = {}; },
  };
})();

Object.defineProperty(window, 'localStorage', { value: localStorageMock });

const mockLoginResponse: LoginResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresInMinutes: 60,
  mustChangePassword: false,
  user: {
    id: '1',
    email: 'alvaro@edificia.es',
    fullName: 'Alvaro Arquitecto',
    collegiateNumber: null,
    roles: ['Architect'],
  },
};

describe('AuthStore', () => {
  beforeEach(() => {
    localStorage.clear();
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      mustChangePassword: false,
      isAuthenticated: false,
    });
  });

  it('should initialize with default values', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });

  it('should set auth data correctly via setAuth', () => {
    useAuthStore.getState().setAuth(mockLoginResponse);

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.user).toEqual(mockLoginResponse.user);
    expect(state.accessToken).toBe('access-token');
    expect(state.refreshToken).toBe('refresh-token');
    expect(state.mustChangePassword).toBe(false);
  });

  it('should update tokens via setTokens', () => {
    useAuthStore.getState().setAuth(mockLoginResponse);
    useAuthStore.getState().setTokens('new-access', 'new-refresh');

    const state = useAuthStore.getState();
    expect(state.accessToken).toBe('new-access');
    expect(state.refreshToken).toBe('new-refresh');
  });

  it('should check roles via hasRole', () => {
    useAuthStore.getState().setAuth(mockLoginResponse);
    expect(useAuthStore.getState().hasRole('Architect')).toBe(true);
    expect(useAuthStore.getState().hasRole('Admin')).toBe(false);
    expect(useAuthStore.getState().hasRole('Admin', 'Architect')).toBe(true);
  });

  it('should clear data on logout', () => {
    useAuthStore.getState().setAuth(mockLoginResponse);
    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
    expect(state.accessToken).toBeNull();
  });
});
