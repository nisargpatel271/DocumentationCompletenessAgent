export interface Repository {
  id: string;
  name: string;
  repositoryUrl: string;
  source: string;
  defaultBranch: string;
  isActive: boolean;
  lastScannedAt: string | null;
}

export interface AnalysisJob {
  id: string;
  repositoryId: string;
  status: 'Queued' | 'Running' | 'Completed' | 'Failed';
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  errorMessage: string | null;
}

export interface DocumentationGap {
  id: string;
  filePath: string;
  lineNumber: number;
  elementName: string;
  elementType: string;
  gapType: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  message: string;
  missingCoverageType: string;
  language: string;
  status: string;
  codeSnippet?: string;
}

export interface AnalysisResult {
  jobId: string;
  overallCoverage: number;
  totalGaps: number;
  criticalGaps: number;
  highPriorityGaps: number;
  mediumPriorityGaps: number;
  lowPriorityGaps: number;
  totalFiles: number;
  analyzedFiles: number;
  gaps: DocumentationGap[];
}

export interface AISuggestion {
  id: string;
  gapId: string;
  elementName: string;
  elementType: string;
  language: string;
  generatedDocumentation: string;
  confidenceScore: number;
  needsHumanReview: boolean;
  status: 'Pending' | 'Accepted' | 'Rejected';
  generatedAt: string;
}
