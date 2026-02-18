import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthGuard } from '@/components/auth/AuthGuard';
import { useAuthStore } from '@/store/useAuthStore';

// Mock de useAuthStore
vi.mock('@/store/useAuthStore', () => ({
  useAuthStore: vi.fn(),
}));

describe('AuthGuard component', () => {
  it('should render children visible if authenticated', () => {
    (useAuthStore as any).mockReturnValue({
      _hasHydrated: true,
      isAuthenticated: true,
      mustChangePassword: false,
      hasRole: () => true,
    });

    render(
      <AuthGuard>
        <div data-testid="protected-content">Protected</div>
      </AuthGuard>
    );

    const content = screen.getByTestId('protected-content');
    expect(content).toBeInTheDocument();
    // The wrapper uses display:contents when visible → content is accessible
    expect(content.closest('div[style]')?.getAttribute('style')).toContain('contents');
  });

  it('should hide children with CSS (not remove from DOM) when not authenticated', () => {
    (useAuthStore as any).mockReturnValue({
      _hasHydrated: true,
      isAuthenticated: false,
      mustChangePassword: false,
      hasRole: () => false,
    });

    // Mock de window.location.href
    const originalLocation = window.location;
    delete (window as any).location;
    window.location = { ...originalLocation, href: '' } as any;

    render(
      <AuthGuard>
        <div data-testid="protected-content">Protected</div>
      </AuthGuard>
    );

    // Children exist in DOM but are hidden via display:none
    const content = screen.getByTestId('protected-content');
    expect(content).toBeInTheDocument();
    expect(content.closest('div[style]')?.getAttribute('style')).toContain('none');

    window.location = originalLocation as any;
  });

  it('should hide children while hydrating (not remove from DOM)', () => {
    (useAuthStore as any).mockReturnValue({
      _hasHydrated: false,
      isAuthenticated: false,
      mustChangePassword: false,
      hasRole: () => false,
    });

    render(
      <AuthGuard>
        <div data-testid="protected-content">Protected</div>
      </AuthGuard>
    );

    // Children exist in DOM but are hidden — Astro can still hydrate nested islands
    const content = screen.getByTestId('protected-content');
    expect(content).toBeInTheDocument();
    expect(content.closest('div[style]')?.getAttribute('style')).toContain('none');
  });
});
