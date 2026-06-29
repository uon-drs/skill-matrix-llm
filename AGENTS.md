# skill-matrix-llm — Agent Instructions

## Code Style

Follow `.editorconfig` at the repo root for all formatting decisions — indent size, line endings, charset, C# naming conventions, and C# language feature preferences. Do not override or duplicate its rules.

---

## Documentation

### Backend (C#)

All public classes, methods, and properties must have XML doc comments:

```csharp
/// <summary>Brief description of what this does.</summary>
/// <param name="paramName">What the parameter is.</param>
/// <returns>What is returned.</returns>
```

Internal and private members: add a comment only when the intent is not obvious from the name and types alone.

### Frontend (TypeScript)

All exported functions, components, and hooks must have JSDoc comments:

```ts
/**
 * Brief description of what this does.
 * @param paramName - what the parameter is
 * @returns what is returned
 */
```

Inline comments for non-obvious logic only — do not comment self-evident code.

---

## Testing

### Backend

- **Framework:** xUnit
- **Location:** `backend/tests/SkillMatrixLlm.Api.IntegrationTests/` and `backend/tests/SkillMatrixLlm.Api.UnitTests/`
- **Attributes:** `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterised cases
- **Naming:** `MethodName_ExpectedBehaviour_Condition`
- **Pattern:** use `WebApplicationFactory` integration tests for controllers and middleware; plain unit tests for pure logic classes

### Frontend

- **Framework:** Vitest + React Testing Library (set up if not already present)
- **Location:** colocate tests alongside source files as `ComponentName.test.tsx`, or group in a `__tests__/` folder in the same directory
- **Naming:** `describe` block per component or function; `it('does x when y')`

---

## Commits

After making code changes, create commits that group changes in functionally meaningful ways. Each commit should represent one coherent unit of work (e.g. a new feature, a bug fix, a refactor) — not one commit per file, and not all changes lumped into one commit.

---

## Development Workflow

When implementing a feature or bug fix:

0. **Create a GitHub issue** if one describing the work doesn't already exist, using the body format from `.github/ISSUE_TEMPLATE.md`:
   ```bash
   gh issue create --title "<title>" --body "<description>"
   ```
1. **Ensure `main` is up to date** before branching:
   ```bash
   git checkout main && git pull
   ```
2. **Create a branch** with a descriptive name:
   ```bash
   git checkout -b feature/<name>   # new functionality
   git checkout -b fix/<name>       # bug fix
   ```
3. **Develop** the requested change.
4. **Commit** to the branch. Group commits by type and file:
   - App code per file
   - Tests per file, separate from app code
   - `CLAUDE.md` updates last
5. **Open a PR** using the `gh` CLI, with a body matching `.github/pull_request_template.md`:
   ```bash
   gh pr create --title "<title>" --body "..."
   ```
