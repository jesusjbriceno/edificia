import apiClient from '@/lib/api';
import type {
  GenerateTextRequest,
  GeneratedTextResponse,
} from '@/lib/types';

/**
 * AI service – thin wrappers around the /projects/:id/ai endpoints.
 * Every function returns the typed DTO directly or throws `ApiError`.
 */
export const aiService = {
  /** POST /projects/:projectId/ai/generate */
  async generate(
    projectId: string,
    payload: GenerateTextRequest,
  ): Promise<GeneratedTextResponse> {
    const { data } = await apiClient.post<GeneratedTextResponse>(
      `/projects/${projectId}/ai/generate`,
      payload,
    );
    return data;
  },
} as const;
