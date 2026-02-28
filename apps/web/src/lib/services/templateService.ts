import apiClient from '@/lib/api';
import type { TemplateResponse } from '@/lib/types';

export interface CreateTemplatePayload {
  name: string;
  templateType: string;
  description?: string;
  file: File;
}

export const templateService = {
  async list(params?: { templateType?: string; isActive?: boolean }): Promise<TemplateResponse[]> {
    const { data } = await apiClient.get<TemplateResponse[]>('/templates', { params });
    return data;
  },

  async create(payload: CreateTemplatePayload): Promise<string> {
    const formData = new FormData();
    formData.append('name', payload.name);
    formData.append('templateType', payload.templateType);
    if (payload.description) {
      formData.append('description', payload.description);
    }
    formData.append('templateFile', payload.file);

    const { data } = await apiClient.post<string>('/templates', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });

    return data;
  },

  async toggleStatus(templateId: string, isActive: boolean): Promise<void> {
    await apiClient.put(`/templates/${templateId}/toggle-status`, { isActive });
  },
} as const;
