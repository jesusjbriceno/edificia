import apiClient from '@/lib/api';
import type { TemplateResponse } from '@/lib/types';

export interface CreateTemplatePayload {
  name: string;
  templateType: string;
  description?: string;
  file: File;
}

export interface UpdateTemplateMetadataPayload {
  name: string;
  description?: string;
}

export interface ListTemplatesParams {
  templateType?: string;
  isAvailable?: boolean;
  isActive?: boolean;
}

export const templateService = {
  async list(params?: ListTemplatesParams): Promise<TemplateResponse[]> {
    const queryParams = {
      templateType: params?.templateType,
      isActive: params?.isAvailable ?? params?.isActive,
    };

    const { data } = await apiClient.get<TemplateResponse[]>('/templates', { params: queryParams });
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

  async updateMetadata(templateId: string, payload: UpdateTemplateMetadataPayload): Promise<void> {
    await apiClient.put(`/templates/${templateId}`, payload);
  },

  async setAvailability(templateId: string, isAvailable: boolean): Promise<void> {
    await apiClient.put(`/templates/${templateId}/availability`, { isAvailable });
  },

  async setDefault(templateId: string, isDefault: boolean): Promise<void> {
    await apiClient.put(`/templates/${templateId}/default`, { isDefault });
  },

  async delete(templateId: string): Promise<void> {
    await apiClient.delete(`/templates/${templateId}`);
  },
} as const;
