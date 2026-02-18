import React, { useEffect } from 'react';
import { useAuthStore } from '@/store/useAuthStore';
import type { Role } from '@/lib/types';

interface AuthGuardProps {
  children: React.ReactNode;
  /** If set, at least one of these roles is required. */
  allowedRoles?: Role[];
}

/**
 * AuthGuard – protects pages behind authentication + RBAC.
 *
 * IMPORTANT: In Astro's island architecture, nested `client:load` components
 * must always be present in the DOM for Astro to hydrate them. If this
 * component returns `null`, Astro removes the child island DOM nodes and
 * never hydrates them → blank page. We use CSS `display: none` instead so
 * the children always exist in the DOM but are invisible until auth is
 * confirmed.
 */
export const AuthGuard: React.FC<AuthGuardProps> = ({ children, allowedRoles }) => {
  const { _hasHydrated, isAuthenticated, mustChangePassword, hasRole } = useAuthStore();

  useEffect(() => {
    // Wait for Zustand persist to finish rehydrating from localStorage
    // before making any redirect decisions (Astro MPA = full page reload).
    if (!_hasHydrated) return;

    if (!isAuthenticated) {
      globalThis.location.href = '/';
      return;
    }

    // Force password change before accessing any other page
    if (mustChangePassword && !globalThis.location.pathname.startsWith('/profile')) {
      globalThis.location.href = '/profile?tab=security';
      return;
    }

    // Check RBAC
    if (allowedRoles && allowedRoles.length > 0 && !hasRole(...allowedRoles)) {
      globalThis.location.href = '/dashboard';
    }
  }, [_hasHydrated, isAuthenticated, mustChangePassword, allowedRoles, hasRole]);

  // Determine visibility — children are ALWAYS rendered in the DOM
  // so Astro can hydrate nested client:load islands.
  const shouldShow =
    _hasHydrated &&
    isAuthenticated &&
    !(mustChangePassword && !globalThis.location.pathname.startsWith('/profile')) &&
    !(allowedRoles && allowedRoles.length > 0 && !hasRole(...allowedRoles));

  return (
    <div style={{ display: shouldShow ? 'contents' : 'none' }}>
      {children}
    </div>
  );
};
