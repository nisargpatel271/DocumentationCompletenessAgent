import { api } from './api';
import type { Repository } from '../types';

export const IntegrationService = {
    getGitHubRepos: async (): Promise<Repository[]> => {
        const res = await api.get('/Integrations/github/repos');
        return res.data;
    },
    getAdoRepos: async (): Promise<Repository[]> => {
        const res = await api.get('/Integrations/ado/repos');
        return res.data;
    },
};
