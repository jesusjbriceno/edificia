import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

// Alineado con UserInfo del backend (types.ts)
export interface User {
  id: string;
  email: string;
  fullName: string;
  collegiateNumber?: string | null;
  roles: string[];
}

interface AuthState {
  // Datos de sesión
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isHydrated: boolean;

  // Acciones
  login: (payload: { user: User; accessToken: string; refreshToken: string | null }) => void;
  setTokens: (accessToken: string, refreshToken: string | null) => void;
  logout: () => void;
  syncFromServer: (serverUser: User | null) => void;
}

export const useAuthStore = create<AuthState>()(
  devtools(
    persist(
      (set) => ({
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        isHydrated: false,

        login: ({ user, accessToken, refreshToken }) => {
          set({
            user,
            accessToken,
            refreshToken,
            isAuthenticated: true,
          });
        },

        setTokens: (accessToken, refreshToken) => {
          set({ accessToken, refreshToken });
        },

        logout: () => {
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
          });
        },

        syncFromServer: (serverUser) => {
          set({
            user: serverUser,
            isAuthenticated: !!serverUser,
            isHydrated: true,
          });
        },
      }),
      {
        name: 'edificia-auth',
        // Solo persistimos lo esencial en localStorage
        partialize: (state) => ({
          user: state.user,
          accessToken: state.accessToken,
          refreshToken: state.refreshToken,
          isAuthenticated: state.isAuthenticated,
        }),
        onRehydrateStorage: () => (state) => {
          // Cuando se rehidrata desde localStorage, marcamos isHydrated
          if (state) {
            state.isHydrated = true;
          }
        },
      }
    ),
    { name: 'AuthStore' }
  )
);