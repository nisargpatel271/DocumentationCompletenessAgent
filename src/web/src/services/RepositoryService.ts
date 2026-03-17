import { api } from './api';
import type { Repository } from '../types';

export const RepositoryService = {
  getAll: async (): Promise<Repository[]> => {
    const res = await api.get('/repositories');
    return res.data;
  },
  create: async (data: {
    name: string;
    repositoryUrl: string;
    source: string;
    defaultBranch: string;
    personalAccessToken: string;
  }): Promise<Repository> => {
    const res = await api.post('/repositories', data);
    return res.data;
  },
  delete: async (id: string): Promise<void> => {
    await api.delete(`/repositories/${id}`);
  },
};
