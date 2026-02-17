import apiClient, { ApiError } from '@/lib/api';
import type {
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  RevokeTokenRequest,
  ChangePasswordRequest,
  CurrentUserResponse,
  UpdateProfileRequest,
  UpdateProfileResponse,
} from '@/lib/types';

/**
 * Auth service – thin wrappers around the /auth endpoints.
 * Every function returns the typed DTO directly or throws `ApiError`.
 */
export const authService = {
  /** POST /auth/login */
  async login(payload: LoginRequest): Promise<LoginResponse> {
    const { data } = await apiClient.post<LoginResponse>('/auth/login', payload);
    return data;
  },

  /** POST /auth/refresh */
  async refresh(payload: RefreshTokenRequest): Promise<LoginResponse> {
    const { data } = await apiClient.post<LoginResponse>('/auth/refresh', payload);
    return data;
  },

  /** POST /auth/revoke */
  async revoke(payload: RevokeTokenRequest): Promise<void> {
    await apiClient.post('/auth/revoke', payload);
  },

  /** POST /auth/change-password */
  async changePassword(payload: ChangePasswordRequest): Promise<void> {
    await apiClient.post('/auth/change-password', payload);
  },

  /** GET /auth/me */
  async me(): Promise<CurrentUserResponse> {
    const { data } = await apiClient.get<CurrentUserResponse>('/auth/me');
    return data;
  },

  /** PUT /auth/profile */
  async updateProfile(payload: UpdateProfileRequest): Promise<UpdateProfileResponse> {
    const { data } = await apiClient.put<UpdateProfileResponse>('/auth/profile', payload);
    return data;
  },
} as const;
