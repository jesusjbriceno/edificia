import React, { useEffect, useRef } from 'react';
import { useAuthStore } from '@/store/useAuthStore';

/**
 * SessionProvider simplificado para Opción B (tokens en cliente).
 * 
 * Zustand persist se encarga de rehidratar desde localStorage.
 * Este componente solo asegura que isHydrated se marque como true
 * una vez el componente se monta en el cliente.
 */
interface SessionProviderProps {
  children: React.ReactNode;
}

export const SessionProvider: React.FC<SessionProviderProps> = ({ children }) => {
  const initialized = useRef(false);

  useEffect(() => {
    if (!initialized.current) {
      // Zustand persist ya habrá rehidratado desde localStorage a estas alturas.
      // Solo necesitamos asegurar que isHydrated sea true.
      const state = useAuthStore.getState();
      if (!state.isHydrated) {
        useAuthStore.setState({ isHydrated: true });
      }
      initialized.current = true;
    }
  }, []);

  return <>{children}</>;
};
