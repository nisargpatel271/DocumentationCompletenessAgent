using System;
using System.Collections.Generic;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services.Analysis
{
    public class FileAnalysisResult
    {
        public List<DocumentationGap> Gaps { get; set; } = new List<DocumentationGap>();

        // Phase 3 Intelligence: Weighted Coverage
        public int TotalElements { get; set; }
        public int DocumentedElements { get; set; }
        
        // This is the weighted score (Class=3, Method=2, etc.)
        public int TotalWeightedScore { get; set; }
        public int ActualWeightedScore { get; set; }

        public double CoveragePercentage => TotalWeightedScore == 0 ? 100 : Math.Round((double)ActualWeightedScore / TotalWeightedScore * 100, 2);
    }
}
