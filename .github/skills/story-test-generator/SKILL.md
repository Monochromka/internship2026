---
name: story-test-generator
description: Generates test cases and writes unit tests based on a user story number. Use this when asked to write tests for a specific user story or feature.
---

When asked to write tests for a user story, you must strictly follow this exact sequence of 4 steps:

1. **Read the story:** 
   - Use your search tools to find and read the relevant User Story markdown file (usually located in `docs/` or a similar directory). 
   - Carefully analyze the Acceptance Criteria and Business Rules.

2. **Find the relevant code:**
   - Search the repository to locate the existing implementation related to this story.
   - Look for the relevant Controllers, Services, Entities, and DTOs specifically in `src/Projects.Api/` or `src/Tasck.Api/`.

3. **Write test cases:**
   - Based strictly on the Acceptance Criteria, generate a comprehensive list of test cases (covering both happy paths and edge cases/error responses like 404, 409).
   - Output this list to the user first to provide context.

4. **Write the tests:**
   - Write the actual test code in the appropriate test project (`src/Projects.Api.Tests/` or `src/Tasks.Api.Tests/`).
   - Use **xUnit** (`[Fact]`, `[Theory]`) as the primary test framework.
   - Use **Moq** for mocking cross-service dependencies (especially in Tasks.Api).
   - Use **FluentAssertions** for expressive assertions (especially in Projects.Api).
   - Use `Microsoft.EntityFrameworkCore.InMemory` if database interactions need to be tested.
   - Ensure the generated code follows the project's C# style (nullable enabled, async/await).