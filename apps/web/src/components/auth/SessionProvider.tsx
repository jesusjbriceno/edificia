import React, { useRef } from 'react';
// IMPORTANTE: Asegurar que la ruta relativa sea correcta
import { useAuthStore } from '@/store/useAuthStore';

interface SessionProviderProps {
  initialSession: any | null;
  children: React.ReactNode;
}

export const SessionProvider: React.FC<SessionProviderProps> = ({ initialSession, children }) => {
  const initialized = useRef(false);

  if (!initialized.current) {
    useAuthStore.setState({
      user: initialSession,
      isAuthenticated: !!initialSession,
      isHydrated: true,
    });
    initialized.current = true;
  }

  return <>{children}</>;
};

