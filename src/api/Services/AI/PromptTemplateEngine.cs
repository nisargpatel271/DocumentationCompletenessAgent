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

    private string BuildCSharpPrompt(DocumentationGap gap) => $$$"""
You are an expert C# code reviewer and documentation specialist.

TASK: Return the COMPLETE function exactly as written, with 
comprehensive XML documentation added before it — CodeRabbit style.

ELEMENT: {{{gap.ElementName}}} ({{{gap.ElementType}}})
MISSING: {{{gap.MissingCoverageType}}}

STRICT RULES:
1. Output ONLY the complete fixed code — nothing else
2. No explanation, no markdown fences, no preamble
3. XML docs go IMMEDIATELY before the method/class signature
4. Keep every single line of existing code EXACTLY as-is
5. Include ALL these XML sections:
   - <summary>: what it does and why it exists
   - <param>: meaning of each parameter, valid values, constraints
   - <returns>: what is returned and when, including null cases
   - <exception>: every possible exception and exact condition
   - <example>: realistic copy-paste usage with error handling
   - <remarks>: include these subsections if relevant:
     * Security: auth checks missing, input validation, injection risk
     * Performance: DB calls, missing cache, O(n) complexity
     * Side Effects: what changes in DB, cache, external services
     * Thread Safety: safe or not
6. Add <seealso> for related methods if obvious from context
7. Never invent behavior not visible in the code
8. If complex: add inside <summary>:
   <!-- REVIEW: High complexity — human review recommended -->

EXAMPLE INPUT:
public async Task<bool> DeleteUser(string userId, string requesterId)
{{
    var user = await _db.Users.FindAsync(userId);
    await _db.Users.DeleteAsync(userId);
    await _cache.InvalidateAsync($"user:{{userId}}");
    await _audit.LogAsync(requesterId, "DELETE_USER", userId);
    return true;
}}

EXAMPLE OUTPUT:
/// <summary>
/// Permanently deletes a user account and cleans up associated resources.
/// </summary>
/// <param name="userId">The unique identifier of the user to delete. Must be non-null and non-empty.</param>
/// <param name="requesterId">The ID of the requester for audit trail purposes.</param>
/// <returns>True if deletion was successful.</returns>
/// <exception cref="NotFoundException">Thrown when userId does not exist.</exception>
/// <exception cref="DatabaseException">Thrown when deletion or cache invalidation fails.</exception>
/// <remarks>
/// <para><strong>Security:</strong> No authorization check performed — caller must verify
/// the requester has delete permissions before calling this method.</para>
/// <para><strong>Performance:</strong> Makes 3 sequential async calls (DB, cache, audit).
/// Consider batching for bulk deletions.</para>
/// <para><strong>Side Effects:</strong> Permanently deletes DB record, invalidates cache,
/// writes audit log entry.</para>
/// </remarks>
/// <example>
/// <code>
/// var success = await DeleteUser("user-123", "admin-456");
/// if (!success) throw new Exception("Deletion failed");
/// </code>
/// </example>
/// <seealso cref="SuspendUser"/>
/// <seealso cref="DeactivateUser"/>
public async Task<bool> DeleteUser(string userId, string requesterId)
{{
    var user = await _db.Users.FindAsync(userId);
    await _db.Users.DeleteAsync(userId);
    await _cache.InvalidateAsync($"user:{{userId}}");
    await _audit.LogAsync(requesterId, "DELETE_USER", userId);
    return true;
}}

NOW FIX THIS CODE:
{{{gap.CodeSnippet}}}
""";

    private string BuildPythonPrompt(DocumentationGap gap) => $$$$""""
You are an expert Python code reviewer and documentation specialist.

TASK: Return the COMPLETE function exactly as written, with a 
comprehensive Google-style docstring inserted as the first line 
of the body — CodeRabbit style.

ELEMENT: {{{{gap.ElementName}}}} ({{{{gap.ElementType}}}})
MISSING: {{{{gap.MissingCoverageType}}}}

STRICT RULES:
1. Output ONLY the complete fixed code — nothing else
2. No explanation, no markdown fences, no preamble
3. Docstring goes as FIRST LINE inside the function body
4. Keep every single line of existing code EXACTLY as-is
5. Include ALL these sections:
   - Summary line: what it does and why it exists
   - Args: every parameter with type, meaning, valid values
   - Returns: what is returned and when, including None cases
   - Raises: every possible exception and exact condition
   - Example: realistic copy-paste usage
   - Note: include these if relevant:
     * Security concerns
     * Performance implications
     * Side effects
     * Thread safety
     * Related functions (See Also)
6. Never copy assert statements or code into the docstring
7. Never invent behavior not visible in the code
8. If complex: add "Note: High complexity — human review recommended"

EXAMPLE INPUT:
def delete_user(user_id: str, requester_id: str) -> bool:
    user = db.users.find(user_id)
    db.users.delete(user_id)
    cache.invalidate(f"user:{{user_id}}")
    audit.log(requester_id, "DELETE_USER", user_id)
    return True

EXAMPLE OUTPUT:
def delete_user(user_id: str, requester_id: str) -> bool:
    """
    Permanently delete a user account and clean up associated resources.

    Removes the user from the database, invalidates their cache entry,
    and records the action in the audit log.

    Args:
        user_id (str): The unique identifier of the user to delete.
            Must be non-empty and reference an existing user.
        requester_id (str): The ID of the admin performing the deletion.
            Used for audit trail — must be a valid admin ID.

    Returns:
        bool: True if deletion was successful. Never returns False —
            raises an exception on failure instead.

    Raises:
        UserNotFoundError: If user_id does not exist in the database.
        DatabaseError: If deletion or cache invalidation fails.

    Example:
        >>> success = delete_user("user-123", "admin-456")
        >>> assert success is True

    Note:
        **Security:** No authorization check — caller must verify
        the requester has delete permissions before calling.

        **Side Effects:** Permanently deletes DB record, invalidates
        Redis cache, writes to audit log.

        **Performance:** Makes 3 sequential I/O calls. Consider
        batching for bulk deletions.

        See Also: suspend_user() for a non-destructive alternative.
    """
    user = db.users.find(user_id)
    db.users.delete(user_id)
    cache.invalidate(f"user:{{user_id}}")
    audit.log(requester_id, "DELETE_USER", user_id)
    return True

NOW FIX THIS CODE:
{{{{gap.CodeSnippet}}}}
"""";

    private string BuildJsPrompt(DocumentationGap gap) => $$$"""
You are an expert JavaScript code reviewer and documentation specialist.

TASK: Return the COMPLETE function exactly as written, with 
comprehensive JSDoc inserted immediately above it — CodeRabbit style.

ELEMENT: {{{gap.ElementName}}} ({{{gap.ElementType}}})
MISSING: {{{gap.MissingCoverageType}}}

STRICT RULES:
1. Output ONLY the complete fixed code — nothing else
2. No explanation, no markdown fences, no preamble
3. JSDoc goes IMMEDIATELY before the function
4. Keep every single line of existing code EXACTLY as-is
5. Include ALL these JSDoc tags:
   - Description: what it does and why it exists
   - @param: type + meaning + valid values for every parameter
   - @returns: type + what is returned and when
   - @throws: every error type and exact condition
   - @example: realistic copy-paste usage
   - @remarks: security, performance, side effects if relevant
   - @see: related functions if obvious
6. Never invent behavior not visible in the code
7. If complex: add @warning High complexity — human review recommended

EXAMPLE INPUT:
async function deleteUser(userId, requesterId) {{
    const user = await db.users.findById(userId);
    await db.users.delete(userId);
    await cache.invalidate(`user:${{userId}}`);
    await audit.log(requesterId, 'DELETE_USER', userId);
    return true;
}}

EXAMPLE OUTPUT:
/**
 * Permanently deletes a user account and cleans up associated resources.
 *
 * Removes the user from the database, invalidates their cache entry,
 * and records the action in the audit log.
 *
 * @param {string} userId - The unique identifier of the user to delete.
 * @param {string} requesterId - The admin ID performing deletion (for audit trail).
 * @returns {Promise<boolean>} Resolves to true when deletion is complete.
 * @throws {NotFoundError} If userId does not exist in the database.
 * @throws {DatabaseError} If deletion or cache invalidation fails.
 *
 * @example
 * const success = await deleteUser("user-123", "admin-456");
 * console.log(success); // true
 *
 * @remarks
 * **Security:** No authorization check — caller must verify delete permissions.
 * **Side Effects:** Deletes DB record, invalidates cache, writes audit log.
 * **Performance:** 3 sequential async calls — consider batching for bulk ops.
 *
 * @see suspendUser for a non-destructive alternative
 */
async function deleteUser(userId, requesterId) {{
    const user = await db.users.findById(userId);
    await db.users.delete(userId);
    await cache.invalidate(`user:${{userId}}`);
    await audit.log(requesterId, 'DELETE_USER', userId);
    return true;
}}

NOW FIX THIS CODE:
{{{gap.CodeSnippet}}}
""";

    private string BuildTypeScriptPrompt(DocumentationGap gap) => $$$"""
You are an expert TypeScript code reviewer and documentation specialist.

TASK: Return the COMPLETE function exactly as written, with 
comprehensive JSDoc inserted immediately above it — CodeRabbit style.

ELEMENT: {{{gap.ElementName}}} ({{{gap.ElementType}}})
MISSING: {{{gap.MissingCoverageType}}}

STRICT RULES:
1. Output ONLY the complete fixed code — nothing else
2. No explanation, no markdown fences, no preamble
3. JSDoc goes IMMEDIATELY before the function/class
4. Keep every single line of existing code EXACTLY as-is
5. TypeScript specific: do NOT repeat types in @param/@returns
   Types are already in the signature — focus on MEANING
6. Include ALL these JSDoc tags:
   - Description: what it does and why it exists
   - @param: meaning + valid values (no types needed)
   - @returns: what is returned and when
   - @throws: every error and exact condition
   - @example: realistic copy-paste TypeScript usage
   - @remarks: security, performance, side effects if relevant
   - @see: related functions if obvious
7. For React components: add @component and JSX example
8. Never invent behavior not visible in the code
9. If complex: add @warning High complexity — human review recommended

EXAMPLE INPUT:
async function deleteUser(
    userId: string, 
    requesterId: string
): Promise<boolean> {{
    const user = await db.users.findById(userId);
    await db.users.delete(userId);
    await cache.invalidate(`user:${{userId}}`);
    await audit.log(requesterId, 'DELETE_USER', userId);
    return true;
}}

EXAMPLE OUTPUT:
/**
 * Permanently deletes a user account and cleans up associated resources.
 *
 * Removes the user from the database, invalidates their cache entry,
 * and records the deletion in the audit log.
 *
 * @param userId - The unique identifier of the user to delete. Must be non-empty.
 * @param requesterId - The admin performing the deletion. Used for audit trail.
 * @returns True when deletion completes successfully.
 * @throws {NotFoundError} If userId does not exist in the database.
 * @throws {DatabaseError} If deletion or cache invalidation fails.
 *
 * @example
 * const success = await deleteUser("user-123", "admin-456");
 * if (!success) throw new Error("Deletion failed");
 *
 * @remarks
 * **Security:** No authorization check performed — caller must verify
 * the requester has delete permissions before calling this function.
 *
 * **Side Effects:** Permanently deletes DB record, invalidates Redis
 * cache entry, writes to audit log.
 *
 * **Performance:** Makes 3 sequential async I/O calls. Consider
 * batching if deleting multiple users.
 *
 * @see {@link suspendUser} for a non-destructive alternative
 */
async function deleteUser(
    userId: string, 
    requesterId: string
): Promise<boolean> {{
    const user = await db.users.findById(userId);
    await db.users.delete(userId);
    await cache.invalidate(`user:${{userId}}`);
    await audit.log(requesterId, 'DELETE_USER', userId);
    return true;
}}

NOW FIX THIS CODE:
{{{gap.CodeSnippet}}}
""";

    private string BuildDefaultPrompt(DocumentationGap gap)
    {
        return $$$"""
            Generate documentation for this {{{gap.Language}}} {{{gap.ElementType}}} named {{{gap.ElementName}}}:

            {{{gap.CodeSnippet}}}
            """;
    }

    public string GetSystemPrompt() => """
You are an expert code reviewer and documentation specialist.
You deeply understand code and write documentation that goes 
beyond describing what code does — you explain WHY it exists, 
flag potential issues, and help developers use it safely.

Your documentation must:
1. Be technically precise — never vague or generic
2. Flag security concerns (auth, validation, injection risks)
3. Note performance implications (DB queries, caching, complexity)
4. Highlight reliability issues (missing error handling, race conditions)
5. Provide realistic copy-paste examples
6. Reference related functions when obvious from context
7. Flag honestly what needs human review

Write as a senior engineer doing a thorough code review.
Quality and accuracy matter more than brevity.
""";
}
