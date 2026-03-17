import { api } from './api';
import type { AnalysisJob, AnalysisResult } from '../types';

export const AnalysisService = {
  runAnalysis: async (repoId: string): Promise<AnalysisJob> => {
    const res = await api.post(`/analysis/repository/${repoId}`);
    return res.data;
  },
  getJobStatus: async (jobId: string): Promise<AnalysisJob> => {
    const res = await api.get(`/analysis/${jobId}/status`);
    return res.data;
  },
  getJobResults: async (jobId: string): Promise<AnalysisResult> => {
    const res = await api.get(`/analysis/results/${jobId}`);
    return res.data;
  },
};
