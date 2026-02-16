import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthGuard } from '@/components/auth/AuthGuard';
import { useAuthStore } from '@/store/useAuthStore';

// Mock de useAuthStore
vi.mock('@/store/useAuthStore', () => ({
  useAuthStore: vi.fn(),
}));

describe('AuthGuard component', () => {
  it('should render children if authenticated', () => {
    (useAuthStore as any).mockReturnValue({ isAuthenticated: true });
    
    render(
      <AuthGuard>
        <div data-testid="protected-content">Protected</div>
      </AuthGuard>
    );

    expect(screen.getByTestId('protected-content')).toBeInTheDocument();
  });

  it('should not render children and redirect if not authenticated', () => {
    (useAuthStore as any).mockReturnValue({ isAuthenticated: false });
    
    // Mock de window.location.href
    const originalLocation = window.location;
    delete (window as any).location;
    window.location = { ...originalLocation, href: '' } as any;

    render(
      <AuthGuard>
        <div data-testid="protected-content">Protected</div>
      </AuthGuard>
    );

    expect(screen.queryByTestId('protected-content')).not.toBeInTheDocument();
    expect(window.location.href).toBe('/login');

    window.location = originalLocation as any;
  });
});
