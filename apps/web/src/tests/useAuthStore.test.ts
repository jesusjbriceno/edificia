import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useAuthStore } from '@/store/useAuthStore';

// Mock de fetch para el logout
global.fetch = vi.fn();

// Mock de window.location
const mockLocation = {
  href: '',
  assign: vi.fn(),
  replace: vi.fn(),
  reload: vi.fn(),
};

Object.defineProperty(window, 'location', {
  value: mockLocation,
  writable: true,
});

describe('useAuthStore', () => {
  beforeEach(() => {
    // Resetear el store antes de cada test
    useAuthStore.setState({ user: null, isAuthenticated: false, isHydrated: false });
    vi.clearAllMocks();
  });

  it('debe iniciar con estado por defecto (no hidratado)', () => {
    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    expect(state.isHydrated).toBe(false);
  });

  it('syncFromServer debe hidratar el usuario desde el servidor', () => {
    const mockUser = {
      id: '123',
      name: 'Test User',
      email: 'test@edificia.com',
      role: 'Admin'
    };

    useAuthStore.getState().syncFromServer(mockUser);

    const state = useAuthStore.getState();
    expect(state.user).toEqual(mockUser);
    expect(state.isAuthenticated).toBe(true);
    expect(state.isHydrated).toBe(true);
  });

  it('syncFromServer con null debe marcar como hidratado pero no autenticado', () => {
    useAuthStore.getState().syncFromServer(null);

    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    expect(state.isHydrated).toBe(true);
  });

  it('login debe actualizar el estado del usuario', () => {
    const mockUser = { id: '1', name: 'User', email: 'u@test.com', role: 'User' };
    
    useAuthStore.getState().login(mockUser);
    
    const state = useAuthStore.getState();
    expect(state.user).toEqual(mockUser);
    expect(state.isAuthenticated).toBe(true);
  });

  it('logout debe limpiar el estado y llamar al endpoint de logout', async () => {
    // Setup estado inicial
    useAuthStore.setState({ 
      user: { id: '1', name: 'User', email: 'email', role: 'User' }, 
      isAuthenticated: true 
    });

    await useAuthStore.getState().logout();

    // Validar limpieza de estado
    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);

    // Validar llamada a API
    expect(global.fetch).toHaveBeenCalledWith('/api/auth/logout', { method: 'POST' });
    
    // Validar redirección
    expect(window.location.href).toBe('/');
  });
});