import axios from 'axios';
import { Repository } from '../types/Repository';

const API_URL = 'http://localhost:5000/api/repositories';

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
    }
};
