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
    useAuthStore.setState({ user: null, isAuthenticated: false, isHydrated: false });
    window.location.href = '';
  });

  it('no renderiza nada mientras no está hidratado (estado inicial)', () => {
    // isHydrated es false por defecto
    render(
      <AuthGuard>
        <div data-testid="protected-content">Secret</div>
      </AuthGuard>
    );

    // Debería mostrar spinner o nada, pero NO el contenido protegido
    expect(screen.queryByTestId('protected-content')).toBeNull();
  });

  it('redirige a la raíz si está hidratado pero no autenticado', async () => {
    // Simular que Astro/SessionProvider ya corrió pero no había usuario
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
      user: { id: '1', role: 'Admin', email: 'test', name: 'test' } 
    });

    render(
      <AuthGuard>
        <div data-testid="protected-content">Secret</div>
      </AuthGuard>
    );

    expect(screen.getByTestId('protected-content')).toBeDefined();
  });
});