import React, { useEffect } from 'react';
import { useAuthStore } from '@/store/useAuthStore';

interface AuthGuardProps {
  children: React.ReactNode;
  allowedRoles?: string[];
}

export const AuthGuard: React.FC<AuthGuardProps> = ({ children, allowedRoles }) => {
  const { isAuthenticated, isHydrated, user } = useAuthStore();

  useEffect(() => {
    // Si ya hidrató y no hay usuario, mandamos a la raíz (Login)
    if (isHydrated && !isAuthenticated) {
      window.location.href = '/';
    }
  }, [isHydrated, isAuthenticated]);

  // Si no está hidratado o no está autenticado, no renderizamos nada
  // para evitar FOUC (Flash of Unauthenticated Content)
  if (!isHydrated || !isAuthenticated) {
    return null;
  }

  // Verificación de roles si se especifican
  if (allowedRoles && allowedRoles.length > 0 && user) {
    const userRoles = user.roles ?? [];
    const hasRole = allowedRoles.some(role => userRoles.includes(role));
    if (!hasRole) {
      return (
        <div className="flex items-center justify-center min-h-[50vh]">
          <p className="text-gray-400">No tienes permisos para acceder a esta sección.</p>
        </div>
      );
    }
  }

  return <>{children}</>;
};

// Exportación por defecto para compatibilidad
export default AuthGuard;