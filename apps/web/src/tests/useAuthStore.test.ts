import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAuthStore } from '@/store/useAuthStore';

// Mock window.location
Object.defineProperty(window, 'location', {
  value: { href: '' },
  writable: true,
});

// Mock fetch
const fetchMock = vi.fn();
global.fetch = fetchMock;

describe('useAuthStore', () => {
  beforeEach(() => {
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isHydrated: false,
    });
    window.location.href = '';
    fetchMock.mockReset();
  });

  it('estado inicial debe estar vacío', () => {
    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.accessToken).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('login debe establecer usuario, tokens y isAuthenticated', () => {
    const mockUser = { id: '1', email: 'test@test.com', fullName: 'Test User', roles: ['Admin'] };

    useAuthStore.getState().login({
      user: mockUser,
      accessToken: 'access-123',
      refreshToken: 'refresh-456',
    });

    const state = useAuthStore.getState();
    expect(state.user).toEqual(mockUser);
    expect(state.accessToken).toBe('access-123');
    expect(state.refreshToken).toBe('refresh-456');
    expect(state.isAuthenticated).toBe(true);
  });

  it('setTokens debe actualizar los tokens', () => {
    useAuthStore.getState().setTokens('new-access', 'new-refresh');

    const state = useAuthStore.getState();
    expect(state.accessToken).toBe('new-access');
    expect(state.refreshToken).toBe('new-refresh');
  });

  it('logout debe limpiar el estado', () => {
    // Setup
    useAuthStore.setState({
      user: { id: '1', email: 'e', fullName: 'User', roles: ['Admin'] },
      accessToken: 'token',
      refreshToken: 'refresh',
      isAuthenticated: true,
    });

    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.accessToken).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('syncFromServer debe establecer usuario y marcar hidratado', () => {
    const serverUser = { id: '1', email: 'test@test.com', fullName: 'Test', roles: ['Admin'] };

    useAuthStore.getState().syncFromServer(serverUser);

    const state = useAuthStore.getState();
    expect(state.user).toEqual(serverUser);
    expect(state.isAuthenticated).toBe(true);
    expect(state.isHydrated).toBe(true);
  });

  it('syncFromServer con null debe marcar como no autenticado', () => {
    useAuthStore.getState().syncFromServer(null);

    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    expect(state.isHydrated).toBe(true);
  });
});