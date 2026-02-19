import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AuthGuard } from '@/components/auth/AuthGuard';
import { useAuthStore } from '@/store/useAuthStore';

// Mock simple para window.location
Object.defineProperty(window, 'location', {
  value: { href: '' },
  writable: true,
});

describe('AuthGuard', () => {
  beforeEach(() => {
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isHydrated: false,
    });
    window.location.href = '';
  });

  it('no renderiza nada mientras no está hidratado (estado inicial)', () => {
    render(
      <AuthGuard>
        <div data-testid="protected-content">Secret</div>
      </AuthGuard>
    );

    expect(screen.queryByTestId('protected-content')).toBeNull();
  });

  it('redirige a la raíz si está hidratado pero no autenticado', async () => {
    useAuthStore.setState({ isHydrated: true, isAuthenticated: false });

    render(
      <AuthGuard>
        <div>Secret</div>
      </AuthGuard>
    );

    await waitFor(() => {
      expect(window.location.href).toBe('/');
    });
  });

  it('renderiza el contenido si está autenticado e hidratado', () => {
    useAuthStore.setState({
      isHydrated: true,
      isAuthenticated: true,
      accessToken: 'test-token',
      user: { id: '1', email: 'test@test.com', fullName: 'Test User', roles: ['Admin'] },
    });

    render(
      <AuthGuard>
        <div data-testid="protected-content">Secret</div>
      </AuthGuard>
    );

    expect(screen.getByTestId('protected-content')).toBeDefined();
  });

  it('muestra mensaje de permisos si el rol no coincide', () => {
    useAuthStore.setState({
      isHydrated: true,
      isAuthenticated: true,
      accessToken: 'test-token',
      user: { id: '1', email: 'test@test.com', fullName: 'Test User', roles: ['Collaborator'] },
    });

    render(
      <AuthGuard allowedRoles={['Root', 'Admin']}>
        <div data-testid="protected-content">Secret</div>
      </AuthGuard>
    );

    expect(screen.queryByTestId('protected-content')).toBeNull();
    expect(screen.getByText(/no tienes permisos/i)).toBeDefined();
  });
});