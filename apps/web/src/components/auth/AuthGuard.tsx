import React, { useEffect } from 'react';
import { useAuthStore } from '@/store/useAuthStore'; 

interface AuthGuardProps {
  children: React.ReactNode;
}

export const AuthGuard: React.FC<AuthGuardProps> = ({ children }) => {
  const { isAuthenticated, isHydrated } = useAuthStore();

  useEffect(() => {
    // Si ya hidrató y no hay usuario, mandamos a la raíz (Login)
    if (isHydrated && !isAuthenticated) {
      window.location.href = '/';
    }
  }, [isHydrated, isAuthenticated]);

  // Si no está hidratado o no está autenticado, no renderizamos nada
  // para evitar FOUC (Flash of Unauthenticated Content) o tests fallidos
  if (!isHydrated || !isAuthenticated) {
    return null;
  }

  return <>{children}</>;
};

// CRÍTICO: Exportación por defecto para compatibilidad con los tests existentes
export default AuthGuard;