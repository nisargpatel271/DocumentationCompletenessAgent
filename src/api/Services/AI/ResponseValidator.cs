using System.Text.RegularExpressions;

namespace DocumentationCompleteness.Api.Services.AI;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public double Score { get; set; }
    public bool NeedsHumanReview { get; set; }
}

public class ResponseValidator
{
    public bool IsValid(string response)
    {
        return !string.IsNullOrWhiteSpace(response) && response.Length >= 50;
    }

    public ValidationResult Validate(string response, string language)
    {
        var result = new ValidationResult
        {
            IsValid = true,
            Score = 0.7,
            NeedsHumanReview = false
        };

        if (string.IsNullOrWhiteSpace(response) || response.Length < 50)
        {
            result.IsValid = false;
            result.Score = 0;
            return result;
        }

        // 1. Check for placeholders
        string[] placeholders = { "TODO", "Description here", "Add description", "Insert description", "Your description" };
        bool hasPlaceholders = false;
        foreach (var p in placeholders)
        {
            if (response.Contains(p, StringComparison.OrdinalIgnoreCase))
            {
                hasPlaceholders = true;
                break;
            }
        }

        if (hasPlaceholders)
        {
            result.NeedsHumanReview = true;
            result.Score -= 0.3;
        }

        // 2. Check for examples
        if (response.Contains("example", StringComparison.OrdinalIgnoreCase) || 
            response.Contains(">>>") || 
            response.Contains("```"))
        {
            result.Score += 0.1;
        }

        // 3. Length checks
        if (response.Length < 100)
        {
            result.NeedsHumanReview = true;
        }

        // 4. Complexity note
        if (response.Contains("human review recommended", StringComparison.OrdinalIgnoreCase))
        {
            result.NeedsHumanReview = true;
        }

        // 5. Language specific
        if (language.Equals("csharp", StringComparison.OrdinalIgnoreCase))
        {
            if (response.Contains("<example>")) result.Score += 0.1;
            if (response.Contains("<remarks>")) result.Score += 0.05;
            
            // Param count check (approximated by counting tags)
            // Note: In a real scenario we'd pass the expected param count
        }

        // Final NeedsHumanReview check
        if (result.Score < 0.6)
        {
            result.NeedsHumanReview = true;
        }

        result.Score = Math.Clamp(result.Score, 0, 1.0);
        return result;
    }
}
