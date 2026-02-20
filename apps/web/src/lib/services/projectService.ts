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

  /** PATCH /projects/:id */
  async update(id: string, payload: Partial<CreateProjectRequest>): Promise<ProjectResponse> {
    const { data } = await apiClient.patch<ProjectResponse>(`/projects/${id}`, payload);
    return data;
  },

  /** DELETE /projects/:id */
  async delete(id: string): Promise<void> {
    await apiClient.delete(`/projects/${id}`);
  },

  /** GET /projects/:id/tree */
  async getContentTree(projectId: string): Promise<ContentTreeResponse> {
    const { data } = await apiClient.get<ContentTreeResponse>(
      `/projects/${projectId}/tree`,
    );
    return data;
  },

  /** PUT /projects/:id/tree */
  async updateContentTree(
    projectId: string,
    payload: UpdateProjectTreeRequest,
  ): Promise<void> {
    await apiClient.put(`/projects/${projectId}/tree`, payload);
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

  /** GET /projects/:id/export — downloads a .docx binary blob */
  async exportDocx(projectId: string): Promise<{ blob: Blob; fileName: string }> {
    const response = await apiClient.get(`/projects/${projectId}/export`, {
      responseType: 'blob',
    });

    // Extract filename from Content-Disposition header, fallback to default
    const disposition = response.headers['content-disposition'] as string | undefined;
    let fileName = 'memoria-proyecto.docx';
    if (disposition) {
      const match = /filename\*?=(?:UTF-8''|")?([^";]+)"?/i.exec(disposition);
      if (match?.[1]) {
        fileName = decodeURIComponent(match[1]);
      }
    }

    return { blob: response.data as Blob, fileName };
  },
} as const;
