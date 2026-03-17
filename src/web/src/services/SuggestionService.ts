import { api } from './api';
import type { AISuggestion } from '../types';

export const SuggestionService = {
  generate: async (gapId: string): Promise<AISuggestion> => {
    const res = await api.post('/documentation/generate', { gapId });
    return res.data;
  },
  accept: async (id: string): Promise<void> => {
    await api.post(`/documentation/suggestions/${id}/accept`);
  },
  reject: async (id: string): Promise<void> => {
    await api.post(`/documentation/suggestions/${id}/reject`);
  },
};
