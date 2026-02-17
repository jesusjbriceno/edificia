import React, { useEffect } from 'react';
import { useAuthStore } from '@/store/useAuthStore';
import type { Role } from '@/lib/types';

interface AuthGuardProps {
  children: React.ReactNode;
  /** If set, at least one of these roles is required. */
  allowedRoles?: Role[];
}

export const AuthGuard: React.FC<AuthGuardProps> = ({ children, allowedRoles }) => {
  const { isAuthenticated, mustChangePassword, hasRole } = useAuthStore();

  useEffect(() => {
    if (!isAuthenticated) {
      window.location.href = '/';
      return;
    }

    // Force password change before accessing any other page
    if (mustChangePassword && !window.location.pathname.startsWith('/profile')) {
      window.location.href = '/profile?tab=security';
      return;
    }

    // Check RBAC
    if (allowedRoles && allowedRoles.length > 0 && !hasRole(...allowedRoles)) {
      window.location.href = '/dashboard';
    }
  }, [isAuthenticated, mustChangePassword, allowedRoles, hasRole]);

  if (!isAuthenticated) return null;

  if (mustChangePassword && !window.location.pathname.startsWith('/profile')) return null;

  if (allowedRoles && allowedRoles.length > 0 && !hasRole(...allowedRoles)) return null;

  return <>{children}</>;
};
