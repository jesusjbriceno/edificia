import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserInfo, Role, LoginResponse } from '@/lib/types';

// ─── State shape ─────────────────────────────────────────

interface AuthState {
  // Data
  user: UserInfo | null;
  accessToken: string | null;
  refreshToken: string | null;
  mustChangePassword: boolean;

  // Computed-like getters (derived from data)
  isAuthenticated: boolean;

  // Actions
  /** Hydrate from a LoginResponse (after login or refresh). */
  setAuth: (response: LoginResponse) => void;
  /** Update only the token pair (used by the refresh interceptor). */
  setTokens: (accessToken: string, refreshToken: string | null) => void;
  /** Partially update the user profile (after PUT /auth/profile). */
  updateUser: (patch: Partial<UserInfo>) => void;
  /** Clear session completely. */
  logout: () => void;

  // Helpers
  hasRole: (...roles: Role[]) => boolean;
}

// ─── Store ───────────────────────────────────────────────

const INITIAL_STATE = {
  user: null,
  accessToken: null,
  refreshToken: null,
  mustChangePassword: false,
  isAuthenticated: false,
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      ...INITIAL_STATE,

      setAuth: (response: LoginResponse) =>
        set({
          user: response.user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          mustChangePassword: response.mustChangePassword,
          isAuthenticated: true,
        }),

      setTokens: (accessToken, refreshToken) =>
        set({ accessToken, refreshToken }),

      updateUser: (patch) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...patch } : null,
        })),

      logout: () => set({ ...INITIAL_STATE }),

      hasRole: (...roles: Role[]) => {
        const user = get().user;
        if (!user) return false;
        return user.roles.some((r) => roles.includes(r));
      },
    }),
    {
      name: 'edificia-auth-storage',
      // Only persist the essential serialisable fields
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        mustChangePassword: state.mustChangePassword,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
