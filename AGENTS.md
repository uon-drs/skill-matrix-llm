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
