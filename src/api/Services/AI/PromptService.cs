using System;
using System.Text;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services.AI
{
    public class PromptService : IPromptService
    {
        public string GenerateDocumentationPrompt(DocumentationGap gap)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("You are a Senior Software Engineer and Documentation Expert.");
            sb.AppendLine($"Task: Generate high-quality documentation for a {gap.Language} {gap.ElementType} named '{gap.ElementName}'.");
            
            sb.AppendLine("\n### SYSTEM INSTRUCTIONS ###");
            sb.AppendLine("1. Be concise, technical, and accurate.");
            sb.AppendLine("2. Follow the specific documentation standard for the language:");
            sb.AppendLine("   - C#: Use XML documentation comments (/// <summary>...).");
            sb.AppendLine("   - Python: Use Google-style docstrings (\"\"\"Summary. Args: Returns: \"\"\").");
            sb.AppendLine("   - JS/TS: Use JSDoc (/** ... */).");
            sb.AppendLine("   - C++: Use Doxygen style (/** ... */ or ///).");
            sb.AppendLine("3. DO NOT include the source code in your response, ONLY the documentation block.");
            sb.AppendLine("4. Document all parameters and return types accurately based on the code provided.");
            sb.AppendLine("5. Mention any edge cases or exceptions if they are visible in the snippet.");

            sb.AppendLine("\n### CODE CONTEXT ###");
            sb.AppendLine($"File: {gap.FilePath}");
            sb.AppendLine($"Language: {gap.Language}");
            sb.AppendLine($"Element Name: {gap.ElementName}");
            sb.AppendLine($"Element Type: {gap.ElementType}");
            sb.AppendLine($"Current Issue: {gap.Message}");
            
            sb.AppendLine("\n### SOURCE CODE SNIPPET ###");
            sb.AppendLine("```" + gap.Language);
            sb.AppendLine(gap.CodeSnippet);
            sb.AppendLine("```");

            sb.AppendLine("\n### RESPONSE FORMAT ###");
            sb.AppendLine("Return ONLY the documentation block. No markdown backticks, no chat preamble.");
            
            return sb.ToString();
        }
    }
}
