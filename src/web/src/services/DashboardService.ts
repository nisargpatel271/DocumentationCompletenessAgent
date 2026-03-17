import { api } from './api';

export interface DashboardSummary {
    overallCoverage: number;
    totalGaps: number;
    criticalGaps: number;
    totalRepositories: number;
    topGapsRepos: Array<{
        id: string;
        name: string;
        coverage: number;
        criticalGaps: number;
        totalGaps: number;
    }>;
    recentJobs: Array<{
        id: string;
        repoName: string;
        status: string;
        createdAt: string;
        completedAt?: string;
    }>;
}

export interface DashboardTrends {
    overall: { date: string; coverage: number }[];
    byRepository: {
        repositoryId: string;
        repositoryName: string;
        data: { date: string; coverage: number }[];
    }[];
}

export const DashboardService = {
    getSummary: async (): Promise<DashboardSummary> => {
        const res = await api.get('/dashboard/summary');
        return res.data;
    },
    getTrends: async (days: number = 30): Promise<DashboardTrends> => {
        const res = await api.get(`/dashboard/trends?days=${days}`);
        return res.data;
    }
};
