import axios from 'axios';

const API_URL = 'http://localhost:5022/api';

export interface AnalysisJob {
    id: string;
    repositoryId: string;
    status: string;
    log: string;
    createdAt: string;
    completedAt?: string;
}

export interface DocumentationGap {
    id: string;
    filePath: string;
    lineNumber: number;
    elementName: string;
    elementType: string;
    gapType: string;
    severity: string;
    message: string;
    missingCoverageType: string;
    status: string;
}

export interface AnalysisResult {
    id: string;
    jobId: string;
    totalFiles: number;
    analyzedFiles: number;
    overallCoverage: number;
    totalGaps: number;
    criticalGaps: number;
    highPriorityGaps: number;
    mediumPriorityGaps: number;
    lowPriorityGaps: number;
    createdAt: string;
    gaps: DocumentationGap[];
}

export const AnalysisService = {
    runAnalysis: async (repositoryId: string): Promise<AnalysisJob> => {
        const response = await axios.post(`${API_URL}/analysis/repository/${repositoryId}`);
        return response.data;
    },

    getJobStatus: async (jobId: string): Promise<AnalysisJob> => {
        const response = await axios.get(`${API_URL}/analysis/${jobId}`);
        return response.data;
    },

    getJobResults: async (jobId: string): Promise<AnalysisResult> => {
        const response = await axios.get(`${API_URL}/analysis/results/${jobId}`);
        return response.data;
    }
};
