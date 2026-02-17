using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DocumentationCompleteness.Api.Models;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Services.Analysis
{
    /// <summary>
    /// ENTERPRISE DOCUMENTATION INTELLIGENCE ENGINE
    /// 
    /// Purpose: Analyze source code to produce accurate, standards-compliant documentation insights.
    /// Principles: Accuracy over verbosity. Code truth is authoritative.
    /// </summary>
    public class UniversalCodeAnalyzer : ICodeAnalyzer
    {
        private readonly ILogger<UniversalCodeAnalyzer> _logger;

        // ----------------------------------------------------------------
        // LANGUAGE SPECIALIST REGEX PATTERNS
        // ----------------------------------------------------------------
        
        // Python Specialist
        private static readonly Regex PyDefRegex = new Regex(@"^\s*def\s+(\w+)\s*\(", RegexOptions.Compiled);
        private static readonly Regex PyClassRegex = new Regex(@"^\s*class\s+(\w+)", RegexOptions.Compiled);
        private static readonly Regex PyDocstringArgsRegex = new Regex(@"Args:", RegexOptions.Compiled);
        private static readonly Regex PyDocstringReturnsRegex = new Regex(@"Returns:", RegexOptions.Compiled);

        // TypeScript/JavaScript Specialist
        private static readonly Regex TsClassRegex = new Regex(@"^\s*(?:export\s+)?class\s+(\w+)", RegexOptions.Compiled);
        private static readonly Regex TsFuncRegex = new Regex(@"^\s*(?:export\s+)?function\s+(\w+)\s*\(", RegexOptions.Compiled);
        private static readonly Regex TsMethodRegex = new Regex(@"^\s*(?:public|private|protected)?\s*(\w+)\s*\(", RegexOptions.Compiled);
        private static readonly Regex TsExportRegex = new Regex(@"^\s*export\s+", RegexOptions.Compiled);

        // C++ Specialist
        private static readonly Regex CppClassRegex = new Regex(@"^\s*class\s+(\w+)", RegexOptions.Compiled);
        private static readonly Regex CppFuncRegex = new Regex(@"^\s*(?:[\w\:\*&<>]+\s+)+(\w+)\s*\(", RegexOptions.Compiled);

        public UniversalCodeAnalyzer(ILogger<UniversalCodeAnalyzer> logger)
        {
            _logger = logger;
        }

        public bool SupportsFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return IsSupportedExtension(ext) || Path.GetFileName(filePath).Equals("README.md", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSupportedExtension(string ext)
        {
            return ext.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".py", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".h", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".hpp", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".cc", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var fileName = Path.GetFileName(filePath);
            var ext = Path.GetExtension(filePath);

            try 
            {
                if (fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                {
                    return await AnalyzeReadmeAsync(filePath, repositoryId, jobId);
                }
                else if (ext.Equals(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    return await AnalyzeCSharpAsync(filePath, repositoryId, jobId);
                }
                else if (ext.Equals(".py", StringComparison.OrdinalIgnoreCase))
                {
                    return await AnalyzePythonAsync(filePath, repositoryId, jobId);
                }
                else if (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".js", StringComparison.OrdinalIgnoreCase))
                {
                     return await AnalyzeTypeScriptAsync(filePath, repositoryId, jobId);
                }
                else if (ext.Equals(".cpp", StringComparison.OrdinalIgnoreCase) || ext.Equals(".h", StringComparison.OrdinalIgnoreCase) || 
                         ext.Equals(".hpp", StringComparison.OrdinalIgnoreCase) || ext.Equals(".cc", StringComparison.OrdinalIgnoreCase))
                {
                     return await AnalyzeCppAsync(filePath, repositoryId, jobId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze file: {Path}", filePath);
                // Fail-safe: Return empty result rather than crashing
            }

            return new FileAnalysisResult();
        }

        // ----------------------------------------------------------------
        // C# SPECIALIST (Senior .NET Architect)
        // ----------------------------------------------------------------
        private async Task<FileAnalysisResult> AnalyzeCSharpAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var result = new FileAnalysisResult();
            var gaps = new List<DocumentationGap>();

            var code = await File.ReadAllTextAsync(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = await tree.GetRootAsync();
            
            // Note: We skip SemanticModel for speed unless strictly needed, 
            // but for "Public vs Internal" confidence, we rely on syntax modifiers.

            // 1. File Header Analysis
            result.TotalElements++;
            if (!HasFileHeader(root))
            {
                gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "File", Path.GetFileName(filePath), "Medium", "Missing standard copyright/file header.", "Missing", "Header"));
            }
            else
            {
                 result.DocumentedElements++;
                 result.ActualWeightedScore += 1;
            }
            result.TotalWeightedScore += 1;

            // 2. Class & Method Analysis
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var cls in classes)
            {
                var isPublicClass = IsPublic(cls.Modifiers);
                var className = cls.Identifier.Text;
                
                // Class Documentation
                result.TotalElements++;
                result.TotalWeightedScore += 3;
                
                if (!HasXmlDocumentation(cls))
                {
                    var severity = isPublicClass ? "Critical" : "Low";
                    gaps.Add(CreateGapWithNode(repositoryId, jobId, filePath, cls, "Class", className, severity, "Missing XML summary for class."));
                }
                else
                {
                    result.DocumentedElements++;
                    result.ActualWeightedScore += 3;
                }

                // Method Analysis
                foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
                {
                    var methodName = method.Identifier.Text;
                    var isPublicMethod = IsPublic(method.Modifiers) && isPublicClass; // Public method in internal class is effectively internal
                    
                    result.TotalElements++;
                    result.TotalWeightedScore += 2; // Baseline weight

                    var xmlDoc = GetXmlDoc(method);
                    if (string.IsNullOrEmpty(xmlDoc))
                    {
                        var severity = isPublicMethod ? "Critical" : "Low";
                        gaps.Add(CreateGapWithNode(repositoryId, jobId, filePath, method, "Method", methodName, severity, $"Missing XML summary for {(isPublicMethod ? "public" : "internal")} method."));
                    }
                    else
                    {
                        result.DocumentedElements++;
                        result.ActualWeightedScore += 2;

                        // QUALITY GATE: Parameter Coverage
                        // Rule: "Parameter descriptions must explain meaning" (We check presence first)
                        foreach (var param in method.ParameterList.Parameters)
                        {
                            var paramName = param.Identifier.Text;
                            if (!xmlDoc.Contains($"name=\"{paramName}\""))
                            {
                                gaps.Add(CreateGapWithNode(repositoryId, jobId, filePath, method, "Parameter", $"{methodName}({paramName})", "Medium", $"Missing <param> tag for '{paramName}'.", "Incomplete", "Parameters"));
                            }
                        }

                        // QUALITY GATE: Return Type
                        if (method.ReturnType.ToString() != "void" && !xmlDoc.Contains("<returns>"))
                        {
                            gaps.Add(CreateGapWithNode(repositoryId, jobId, filePath, method, "Return", methodName, "Low", "Missing <returns> tag.", "Incomplete", "Returns"));
                        }
                    }
                }
            }

            result.Gaps = gaps;
            return result;
        }

        // ----------------------------------------------------------------
        // PYTHON SPECIALIST (Google-Style Expert)
        // ----------------------------------------------------------------
        private async Task<FileAnalysisResult> AnalyzePythonAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var result = new FileAnalysisResult();
            var gaps = new List<DocumentationGap>();
            var lines = await File.ReadAllLinesAsync(filePath);

            int totalElements = 0;
            int documentedElements = 0;

            // 1. File Header
            if (!HasFileHeaderGeneric(lines, "#"))
            {
                 // Py files often lack headers, keeping it Medium
                 gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "File", Path.GetFileName(filePath), "Medium", "Missing standard file header/copyright.", "Missing", "Header"));
            }

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                Match match = null;
                string type = "";

                if ((match = PyDefRegex.Match(line)).Success) type = "Function";
                else if ((match = PyClassRegex.Match(line)).Success) type = "Class";

                if (match != null && match.Success)
                {
                    totalElements++;
                    var name = match.Groups[1].Value;
                    var isPublic = !name.StartsWith("_"); // Python convention
                    
                    var (hasDoc, docContent) = GetPythonDocstring(lines, i);

                    if (hasDoc)
                    {
                        documentedElements++;
                        
                        // QUALITY GATE: Google-Style Check
                        // Public APIs require "Args:" and "Returns:"
                        if (isPublic && type == "Function")
                        {
                            if (!PyDocstringArgsRegex.IsMatch(docContent) && line.Contains(",")) // Heuristic: has params
                            {
                                gaps.Add(CreateGap(repositoryId, jobId, filePath, i + 1, "Parameter", name, "Medium", "Public API docstring missing 'Args:' section.", "Incomplete", "Parameters"));
                            }
                            if (!PyDocstringReturnsRegex.IsMatch(docContent) && !line.Contains("-> None")) // Heuristic: might return
                            {
                                // We don't strictly enforce Returns for void, but if unknown, suggests review
                            }
                        }
                    }
                    else
                    {
                        var severity = isPublic ? "Critical" : "Low";
                        gaps.Add(CreateGap(repositoryId, jobId, filePath, i + 1, type, name, severity, $"Missing docstring for {(isPublic ? "public" : "internal")} {type.ToLower()}.", "Missing", "Summary"));
                    }
                }
            }

            result.TotalElements = totalElements;
            result.DocumentedElements = documentedElements;
            result.TotalWeightedScore = totalElements * 2;
            result.ActualWeightedScore = documentedElements * 2;
            result.Gaps = gaps;
            return result;
        }

        // ----------------------------------------------------------------
        // TYPESCRIPT SPECIALIST (JSDoc Expert)
        // ----------------------------------------------------------------
        private async Task<FileAnalysisResult> AnalyzeTypeScriptAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var result = new FileAnalysisResult();
            var gaps = new List<DocumentationGap>();
            var lines = await File.ReadAllLinesAsync(filePath);

            int totalElements = 0;
            int documentedElements = 0;

            // 1. File Header
            if (!HasFileHeaderGeneric(lines, "//", "/*"))
            {
                gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "File", Path.GetFileName(filePath), "Medium", "Missing standard copyright/file header.", "Missing", "Header"));
            }

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                Match match = null;
                string type = "";

                if ((match = TsClassRegex.Match(line)).Success) type = "Class";
                else if ((match = TsFuncRegex.Match(line)).Success) type = "Function";
                else if ((match = TsMethodRegex.Match(line)).Success) type = "Method";

                if (match != null && match.Success)
                {
                    totalElements++;
                    var name = match.Groups[1].Value;
                    
                    // Visibility Check
                    var isPublic = line.Contains("public") || line.Contains("export") || (!line.Contains("private") && !line.Contains("protected"));
                    
                    var (hasDoc, docContent) = GetPrecedingJSDoc(lines, i);

                    if (hasDoc)
                    {
                        documentedElements++;
                        // QUALITY GATE: JSDoc Tags
                        if (isPublic && line.Contains("(") && line.IndexOf(")") > line.IndexOf("(") + 1) // Has params
                        {
                            if (!docContent.Contains("@param"))
                            {
                                gaps.Add(CreateGap(repositoryId, jobId, filePath, i + 1, "Parameter", name, "Medium", "Public API JSDoc missing @param tags.", "Incomplete", "Parameters"));
                            }
                        }
                    }
                    else
                    {
                        var severity = isPublic ? "Critical" : "Low";
                        gaps.Add(CreateGap(repositoryId, jobId, filePath, i + 1, type, name, severity, $"Missing JSDoc for {(isPublic ? "public" : "internal")} {type.ToLower()}.", "Missing", "Summary"));
                    }
                }
            }

            result.TotalElements = totalElements;
            result.DocumentedElements = documentedElements;
            result.TotalWeightedScore = totalElements * 2;
            result.ActualWeightedScore = documentedElements * 2;
            result.Gaps = gaps;
            return result;
        }

        // ----------------------------------------------------------------
        // C++ SPECIALIST (Doxygen Expert)
        // ----------------------------------------------------------------
        private async Task<FileAnalysisResult> AnalyzeCppAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var result = new FileAnalysisResult();
            var gaps = new List<DocumentationGap>();
            var lines = await File.ReadAllLinesAsync(filePath);
            int totalElements = 0;
            int documentedElements = 0;

            if (!HasFileHeaderGeneric(lines, "//", "/*"))
                gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "File", Path.GetFileName(filePath), "Medium", "Missing file header.", "Missing", "Header"));

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                Match match = null;
                string type = "";

                if ((match = CppClassRegex.Match(line)).Success) type = "Class";
                else if ((match = CppFuncRegex.Match(line)).Success) type = "Function";

                if (match != null && match.Success)
                {
                    totalElements++;
                    var name = match.Groups[1].Value;
                    if (name.StartsWith("~")) continue; // Destructor skip

                    var (hasDoc, _) = GetPrecedingJSDoc(lines, i); // Reuse C-style comment check

                    if (hasDoc)
                    {
                        documentedElements++;
                    }
                    else
                    {
                        // Harder to determine public/private in C++ regex, default to High for safety
                        gaps.Add(CreateGap(repositoryId, jobId, filePath, i + 1, type, name, "High", "Missing Doxygen documentation.", "Missing", "Summary"));
                    }
                }
            }
            result.TotalElements = totalElements;
            result.DocumentedElements = documentedElements;
            result.TotalWeightedScore = totalElements * 2;
            result.ActualWeightedScore = documentedElements * 2;
            result.Gaps = gaps;
            return result;
        }

        // ----------------------------------------------------------------
        // README SPECIALIST
        // ----------------------------------------------------------------
        private async Task<FileAnalysisResult> AnalyzeReadmeAsync(string filePath, Guid repositoryId, Guid jobId)
        {
            var result = new FileAnalysisResult(); // README itself is 1 element
            var content = await File.ReadAllTextAsync(filePath);
            var gaps = new List<DocumentationGap>();
            
            result.TotalElements = 1;

            if (string.IsNullOrWhiteSpace(content))
            {
                gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "File", "README.md", "Critical", "README is empty.", "Missing", "Content"));
            }
            else
            {
                bool hasInstall = content.Contains("Installation", StringComparison.OrdinalIgnoreCase) || content.Contains("Getting Started", StringComparison.OrdinalIgnoreCase);
                bool hasUsage = content.Contains("Usage", StringComparison.OrdinalIgnoreCase) || content.Contains("How to run", StringComparison.OrdinalIgnoreCase);

                if (!hasInstall || !hasUsage)
                {
                    result.DocumentedElements = 0; // Partial doesn't count for full score
                    gaps.Add(CreateGap(repositoryId, jobId, filePath, 1, "Section", "README.md", "High", "README missing 'Installation' or 'Usage' sections.", "Incomplete", "Content"));
                }
                else
                {
                    result.DocumentedElements = 1;
                }
            }
            
            result.Gaps = gaps;
            return result;
        }

        // ----------------------------------------------------------------
        // HELPERS & UTILITIES
        // ----------------------------------------------------------------

        private bool IsPublic(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        private bool HasXmlDocumentation(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia();
            return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                                   t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
        }

        private string GetXmlDoc(SyntaxNode node)
        {
             var trivia = node.GetLeadingTrivia();
             return trivia.FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                                               t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)).ToString();
        }

        private bool HasFileHeader(SyntaxNode root)
        {
             if (!root.HasLeadingTrivia) return false;
             var trivia = root.GetLeadingTrivia();
             // Check if START of file has comment
             return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }

        private bool HasFileHeaderGeneric(string[] lines, string single, string? multi = null)
        {
            if (lines.Length == 0) return false;
            for (int i = 0; i < Math.Min(lines.Length, 5); i++)
            {
                var l = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(l)) continue;
                if (l.StartsWith(single)) return true;
                if (!string.IsNullOrEmpty(multi) && l.StartsWith(multi)) return true;
                return false; // Code hit
            }
            return false;
        }

        private (bool exists, string content) GetPythonDocstring(string[] lines, int defLineIndex)
        {
            // Look forward for docstring (""" or ''')
            for (int j = defLineIndex + 1; j < lines.Length; j++)
            {
                var line = lines[j].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                if (line.StartsWith("\"\"\"") || line.StartsWith("'''"))
                {
                    // Collect content until closing
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine(line);
                    // Simple logic: if it doesn't end on same line, we assume it exists. 
                    // Not fully parsing for Validation extraction yet, but checking existence.
                    return (true, sb.ToString());
                }
                return (false, ""); // Code hit
            }
            return (false, "");
        }

        private (bool exists, string content) GetPrecedingJSDoc(string[] lines, int defLineIndex)
        {
            // Look backward for /** or */
            var sb = new System.Text.StringBuilder();
            bool foundEnd = false;
            for (int j = defLineIndex - 1; j >= 0; j--)
            {
                var line = lines[j].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("@")) continue; // Skip decorators
                
                if (line.EndsWith("*/")) 
                {
                    foundEnd = true;
                    sb.Insert(0, line + "\n");
                    if (line.StartsWith("/**")) return (true, sb.ToString()); // Single line JSDoc
                    continue;
                }
                if (foundEnd)
                {
                    sb.Insert(0, line + "\n");
                    if (line.StartsWith("/**")) return (true, sb.ToString());
                    if (line.StartsWith("/*")) return (true, sb.ToString());
                }
                else
                {
                    return (false, ""); // Code matched before doc
                }
            }
            return (false, "");
        }

        private DocumentationGap CreateGapWithNode(Guid repoId, Guid jobId, string filePath, SyntaxNode node, string type, string name, string severity, string message, string gapType = "Missing", string coverageType = "Summary")
        {
            var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            return CreateGap(repoId, jobId, filePath, line, type, name, severity, message, gapType, coverageType);
        }

        private DocumentationGap CreateGap(Guid repoId, Guid jobId, string filePath, int line, string type, string name, string severity, string message, string gapType = "Missing", string coverageType = "Summary")
        {
            return new DocumentationGap
            {
                Id = Guid.NewGuid(),
                RepositoryId = repoId,
                JobId = jobId,
                FilePath = filePath,
                LineNumber = line,
                ElementName = name,
                ElementType = type,
                Severity = severity,
                Message = message,
                GapType = gapType,
                MissingCoverageType = coverageType,
                Status = "Open"
            };
        }
    }
}
