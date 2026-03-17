using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services.AI;

public class PromptTemplateEngine
{
    public string BuildPrompt(DocumentationGap gap)
    {
        return gap.Language.ToLower() switch
        {
            "csharp" => BuildCSharpPrompt(gap),
            "python" => BuildPythonPrompt(gap),
            "javascript" => BuildJsPrompt(gap),
            "typescript" => BuildTypeScriptPrompt(gap),
            _ => BuildDefaultPrompt(gap)
        };
    }

    private string BuildCSharpPrompt(DocumentationGap gap) => $$"""
        You are a senior C# developer fixing missing documentation.

        TASK: Return the COMPLETE function exactly as written, but with 
        proper XML documentation comments added before it.

        ELEMENT: {{gap.ElementName}} ({{gap.ElementType}})
        MISSING: {{gap.MissingCoverageType}}

        STRICT RULES:
        1. Output ONLY the complete fixed code — nothing else
        2. No explanation, no markdown fences, no preamble
        3. XML docs go IMMEDIATELY before the method/class signature
        4. Keep every single line of existing code EXACTLY as-is
        5. Add <summary>, <param> for every param, <returns> if non-void,
           <exception> for every throw, <example> for public elements
        6. Never invent behavior not visible in the code
        7. If complex: add inside <summary>:
           <!-- REVIEW: High complexity — verify this documentation -->

        EXAMPLE:
        INPUT CODE:
        public async Task<User> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid ID");
            return await _repo.FindAsync(id);
        }

        YOUR OUTPUT:
        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user. Must not be empty.</param>
        /// <returns>The user matching the specified ID.</returns>
        /// <exception cref="ArgumentException">Thrown when id is Guid.Empty.</exception>
        /// <example>
        /// <code>
        /// var user = await GetUserById(userId);
        /// Console.WriteLine(user.Name);
        /// </code>
        /// </example>
        public async Task<User> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid ID");
            return await _repo.FindAsync(id);
        }

        NOW FIX THIS CODE:
        {{gap.CodeSnippet}}
        """;

    private string BuildPythonPrompt(DocumentationGap gap) => $$$""""
        You are a senior Python developer fixing missing documentation.

        TASK: Return the COMPLETE function exactly as written, but with 
        a proper Google-style docstring inserted as the first line of the body.

        ELEMENT: {{{gap.ElementName}}} ({{{gap.ElementType}}})
        MISSING: {{{gap.MissingCoverageType}}}

        STRICT RULES:
        1. Output ONLY the complete fixed code — nothing else
        2. No explanation, no markdown fences, no preamble
        3. Docstring goes as FIRST LINE inside the function body
        4. Keep every single line of existing code EXACTLY as-is
        5. Add summary line, Args, Returns, Raises, Example sections
        6. Never copy assert statements or code into the docstring
        7. Never invent behavior not visible in the code
        8. If complex: add "Note: High complexity — verify this documentation"

        EXAMPLE:
        INPUT CODE:
        def get_user_by_id(user_id: str) -> dict:
            if not user_id:
                raise ValueError("Invalid ID")
            return db.find_user(user_id)

        YOUR OUTPUT:
        def get_user_by_id(user_id: str) -> dict:
            """
            Retrieve a user by their unique identifier.

            Args:
                user_id (str): The unique identifier of the user. Must be non-empty.

            Returns:
                dict: User data including id, name, and email.

            Raises:
                ValueError: If user_id is empty or None.

            Example:
                >>> user = get_user_by_id("abc-123")
                >>> print(user["name"])
            """
            if not user_id:
                raise ValueError("Invalid ID")
            return db.find_user(user_id)

        NOW FIX THIS CODE:
        {{{gap.CodeSnippet}}}
        """";

    private string BuildJsPrompt(DocumentationGap gap) => $$"""
        You are a senior JavaScript developer fixing missing documentation.

        TASK: Return the COMPLETE function exactly as written, but with 
        proper JSDoc inserted immediately above it.

        ELEMENT: {{gap.ElementName}} ({{gap.ElementType}})
        MISSING: {{gap.MissingCoverageType}}

        STRICT RULES:
        1. Output ONLY the complete fixed code — nothing else
        2. No explanation, no markdown fences, no preamble
        3. JSDoc goes IMMEDIATELY before the function
        4. Keep every single line of existing code EXACTLY as-is
        5. Add description, @param for every param, @returns, @throws
        6. Never invent behavior not visible in the code
        7. If complex: add @warning High complexity — verify this documentation

        EXAMPLE:
        INPUT CODE:
        async function getUserById(userId) {
            if (!userId) throw new Error("Invalid ID");
            return await db.findUser(userId);
        }

        YOUR OUTPUT:
        /**
         * Retrieves a user by their unique identifier.
         * @param {string} userId - The unique identifier of the user.
         * @returns {Promise<Object>} The user object with id, name and email.
         * @throws {Error} If userId is empty or null.
         * @example
         * const user = await getUserById("abc-123");
         * console.log(user.name);
         */
        async function getUserById(userId) {
            if (!userId) throw new Error("Invalid ID");
            return await db.findUser(userId);
        }

        NOW FIX THIS CODE:
        {{gap.CodeSnippet}}
        """;

    private string BuildTypeScriptPrompt(DocumentationGap gap) => $$"""
        You are a senior TypeScript developer fixing missing documentation.

        TASK: Return the COMPLETE function exactly as written, but with 
        proper JSDoc inserted immediately above it.

        ELEMENT: {{gap.ElementName}} ({{gap.ElementType}})
        MISSING: {{gap.MissingCoverageType}}

        STRICT RULES:
        1. Output ONLY the complete fixed code — nothing else
        2. No explanation, no markdown fences, no preamble
        3. JSDoc goes IMMEDIATELY before the function/class
        4. Keep every single line of existing code EXACTLY as-is
        5. Since this is TypeScript — do NOT repeat types in @param/@returns
           Focus on MEANING not type declarations
        6. Add description, @param for every param, @returns, @throws
        7. For React components add @component and @example with JSX
        8. Never invent behavior not visible in the code
        9. If complex: add @warning High complexity — verify this documentation

        EXAMPLE:
        INPUT CODE:
        async function getUserById(userId: string): Promise<User> {
            if (!userId) throw new Error("Invalid ID");
            return await db.findUser(userId);
        }

        YOUR OUTPUT:
        /**
         * Retrieves a user by their unique identifier.
         * @param userId - The unique identifier of the user. Must be non-empty.
         * @returns The matching user object.
         * @throws {Error} If userId is empty or null.
         * @example
         * const user = await getUserById("abc-123");
         * console.log(user.name);
         */
        async function getUserById(userId: string): Promise<User> {
            if (!userId) throw new Error("Invalid ID");
            return await db.findUser(userId);
        }

        NOW FIX THIS CODE:
        {{gap.CodeSnippet}}
        """;

    private string BuildDefaultPrompt(DocumentationGap gap)
    {
        return $$"""
            Generate documentation for this {{gap.Language}} {{gap.ElementType}} named {{gap.ElementName}}:

            {{gap.CodeSnippet}}
            """;
    }

    public string GetSystemPrompt() => """
        You are a senior software engineer with 10+ years of experience 
        writing technical documentation. Your job is to generate accurate,
        professional documentation for code.

        STRICT RULES:
        1. Output ONLY the documentation comment — no explanation, no markdown fences
        2. Never invent behavior that isn't visible in the code
        3. Never use placeholder text like "TODO" or "Description here"
        4. Always document EVERY parameter — never skip one
        5. If the method returns a value, always document it
        6. If you see throw/raise/except statements, document those exceptions
        7. Include at least one practical usage example for public methods
        8. If code logic is unclear or highly complex, add this exact line:
           "Note: This method has high complexity — human review recommended."
        9. Use precise technical language — avoid vague words like "handles" or "deals with"
        10. Write as if explaining to a capable junior developer joining the team

        Your documentation will be shown directly to developers and must be 
        production-ready. Quality matters more than speed.
        """;
}
