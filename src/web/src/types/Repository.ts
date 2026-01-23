export interface Repository {
    id: string;
    name: string;
    source: string;
    repositoryUrl: string;
    defaultBranch: string;
    isActive: boolean;
    lastScannedAt?: string;
    scanFrequency?: string;
    settings?: string;
    createdAt: string;
    updatedAt: string;
    createdBy?: string;
}
