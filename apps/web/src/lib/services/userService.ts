import apiClient from '@/lib/api';
import type {
  UserResponse,
  CreateUserRequest,
  UpdateUserRequest,
  PagedResponse,
} from '@/lib/types';

/**
 * User (admin) service – thin wrappers around the /users endpoints.
 * Every function returns the typed DTO directly or throws `ApiError`.
 */
export const userService = {
  /** GET /users?page=&pageSize=&role=&isActive= */
  async list(params?: {
    page?: number;
    pageSize?: number;
    role?: string;
    isActive?: boolean;
  }): Promise<PagedResponse<UserResponse>> {
    const { data } = await apiClient.get<PagedResponse<UserResponse>>('/users', { params });
    return data;
  },

  /** GET /users/:id */
  async getById(id: string): Promise<UserResponse> {
    const { data } = await apiClient.get<UserResponse>(`/users/${id}`);
    return data;
  },

  /** POST /users */
  async create(payload: CreateUserRequest): Promise<UserResponse> {
    const { data } = await apiClient.post<UserResponse>('/users', payload);
    return data;
  },

  /** PUT /users/:id */
  async update(id: string, payload: UpdateUserRequest): Promise<UserResponse> {
    const { data } = await apiClient.put<UserResponse>(`/users/${id}`, payload);
    return data;
  },

  /** POST /users/:id/activate */
  async activate(id: string): Promise<void> {
    await apiClient.post(`/users/${id}/activate`);
  },

  /** POST /users/:id/deactivate */
  async deactivate(id: string): Promise<void> {
    await apiClient.post(`/users/${id}/deactivate`);
  },

  /** POST /users/:id/reset-password */
  async resetPassword(id: string): Promise<void> {
    await apiClient.post(`/users/${id}/reset-password`);
  },

  /** DELETE /users/:id */
  async remove(id: string): Promise<void> {
    await apiClient.delete(`/users/${id}`);
  },
} as const;
