import apiClient from '@/lib/api';
import type { TemplateParamResponse } from '@/lib/types';

export interface ListTemplateParamsParams {
  isActive?: boolean;
}

export const templateParamService = {
  async list(params?: ListTemplateParamsParams): Promise<TemplateParamResponse[]> {
    const { data } = await apiClient.get<TemplateParamResponse[]>('/template-params', { params });
    return data;
  },

  async setActivation(templateParamId: string, isActive: boolean): Promise<void> {
    await apiClient.put(`/template-params/${templateParamId}/activation`, { isActive });
  },
} as const;
