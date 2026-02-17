import axios from 'axios';
import type { Repository } from '../types/Repository';

const API_URL = 'http://localhost:5022/api/repositories';

export const RepositoryService = {
    getAll: async (): Promise<Repository[]> => {
        const response = await axios.get<Repository[]>(API_URL);
        return response.data;
    },

    getById: async (id: string): Promise<Repository> => {
        const response = await axios.get<Repository>(`${API_URL}/${id}`);
        return response.data;
    },

    create: async (repository: Omit<Repository, 'id' | 'createdAt' | 'updatedAt'>): Promise<Repository> => {
        const response = await axios.post<Repository>(API_URL, repository);
        return response.data;
    },

    getGitHubRepositories: async (): Promise<Repository[]> => {
        const response = await axios.get<Repository[]>('http://localhost:5022/api/integrations/github/repos');
        return response.data;
    },

    getAzureDevOpsRepositories: async (): Promise<Repository[]> => {
        const response = await axios.get<Repository[]>('http://localhost:5022/api/integrations/ado/repos');
        return response.data;
    }
};
