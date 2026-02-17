import apiClient from '@/lib/api';
import type {
  ProjectResponse,
  CreateProjectRequest,
  ContentTreeResponse,
  UpdateProjectTreeRequest,
  UpdateSectionRequest,
  PagedResponse,
} from '@/lib/types';

/**
 * Project service – thin wrappers around the /projects endpoints.
 * Every function returns the typed DTO directly or throws `ApiError`.
 */
export const projectService = {
  /** GET /projects?page=&pageSize=&status= */
  async list(params?: {
    page?: number;
    pageSize?: number;
    status?: string;
  }): Promise<PagedResponse<ProjectResponse>> {
    const { data } = await apiClient.get<PagedResponse<ProjectResponse>>('/projects', {
      params,
    });
    return data;
  },

  /** GET /projects/:id */
  async getById(id: string): Promise<ProjectResponse> {
    const { data } = await apiClient.get<ProjectResponse>(`/projects/${id}`);
    return data;
  },

  /** POST /projects */
  async create(payload: CreateProjectRequest): Promise<ProjectResponse> {
    const { data } = await apiClient.post<ProjectResponse>('/projects', payload);
    return data;
  },

  /** GET /projects/:id/content-tree */
  async getContentTree(projectId: string): Promise<ContentTreeResponse> {
    const { data } = await apiClient.get<ContentTreeResponse>(
      `/projects/${projectId}/content-tree`,
    );
    return data;
  },

  /** PUT /projects/:id/content-tree */
  async updateContentTree(
    projectId: string,
    payload: UpdateProjectTreeRequest,
  ): Promise<void> {
    await apiClient.put(`/projects/${projectId}/content-tree`, payload);
  },

  /** PATCH /projects/:projectId/sections/:sectionId */
  async updateSection(
    projectId: string,
    sectionId: string,
    payload: UpdateSectionRequest,
  ): Promise<void> {
    await apiClient.patch(
      `/projects/${projectId}/sections/${sectionId}`,
      payload,
    );
  },
} as const;
