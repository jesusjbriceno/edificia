import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useAuthStore } from '../store/useAuthStore';

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

describe('AuthStore', () => {
  beforeEach(() => {
    localStorage.clear();
    // Reiniciar el estado del store manualmente ya que Zustand persiste entre tests
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    });
  });

  it('should initialize with default values', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });

  it('should set login data correctly', () => {
    const mockUser = { id: '1', name: 'Alvaro', email: 'alvaro@edificia.es', role: 'Architect' };
    const mockAccessToken = 'access-token';
    const mockRefreshToken = 'refresh-token';

    useAuthStore.getState().login(mockUser, mockAccessToken, mockRefreshToken);

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.user).toEqual(mockUser);
    expect(state.accessToken).toBe(mockAccessToken);
    expect(state.refreshToken).toBe(mockRefreshToken);
  });

  it('should clear data on logout', () => {
    const state = useAuthStore.getState();
    state.login({ id: '1' } as any, 'at', 'rt');
    state.logout();

    const newState = useAuthStore.getState();
    expect(newState.isAuthenticated).toBe(false);
    expect(newState.user).toBeNull();
    expect(newState.accessToken).toBeNull();
  });
});
