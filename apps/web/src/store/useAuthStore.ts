import { create } from 'zustand';
import { devtools } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  role: string;
  name: string;
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isHydrated: boolean;
  
  login: (user: User) => void;
  logout: () => Promise<void>;
  syncFromServer: (serverUser: User | null) => void;
}

export const useAuthStore = create<AuthState>()(
  devtools(
    (set) => ({
      user: null,
      isAuthenticated: false,
      isHydrated: false,

      login: (user) => {
        set({ user, isAuthenticated: true });
      },

      logout: async () => {
        set({ user: null, isAuthenticated: false });
        await fetch('/api/auth/logout', { method: 'POST' });
        // Redirigir a raíz
        window.location.href = '/';
      },

      syncFromServer: (serverUser) => {
        set({ 
          user: serverUser, 
          isAuthenticated: !!serverUser,
          isHydrated: true 
        });
      }
    }),
    { name: 'AuthStore' }
  )
);