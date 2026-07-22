# Copilot Instructions for TNTU Internship 2026 Task Board

## Project structure
- `src/Projects.Api/` — Projects microservice (ASP.NET Core Web API, EF Core Cosmos).
- `src/Tasck.Api/` — Tasks microservice project folder (contains `Tasks.Api.csproj`; folder name is intentionally `Tasck.Api`).
- `src/Projects.Api.Tests/` — unit tests for Projects API.
- `src/Tasks.Api.Tests/` — unit tests for Tasks API.
- `docs/` — architecture, prerequisites, domain notes, internship plan, and user stories.
- `TaskBoard.slnx` — solution that includes both APIs and both test projects.
- `.github/workflows/` — CI/CD workflows for Projects and Tasks APIs.

## Coding conventions
- Use .NET 8 and ASP.NET Core Web API patterns already present in the repo.
- Keep API routes versioned under `/api/v1/...`.
- Use DTOs in `Models/`, entities in `Entities/`, business logic in `Services/`, and persistence in `Data/`.
- Use dependency injection via `Program.cs` (`AddScoped`, `AddHttpClient`, `AddDbContext`).
- Prefer async service/controller methods and return standard HTTP status codes.
- Use JSON and `ProblemDetails`-style error responses for API validation/business errors.
- Follow existing C# style in files being edited (naming, brace style, nullable enabled).

## Test framework
- Primary framework: **xUnit** (`[Fact]`, `Assert.*`).
- Supporting test libraries in use:
  - `Microsoft.EntityFrameworkCore.InMemory` for in-memory DB tests.
  - `Moq` in `Tasks.Api.Tests` for mocking cross-service dependencies.
  - `FluentAssertions` in `Projects.Api.Tests` for expressive assertions.
- Test projects are:
  - `src/Projects.Api.Tests/Projects.Api.Tests.csproj`
  - `src/Tasks.Api.Tests/Tasks.Api.Tests.csproj`

## Build, test, and run
Run commands from repository root (`/home/runner/work/internship2026/internship2026`).

### Restore and build
- `dotnet restore TaskBoard.slnx`
- `dotnet build TaskBoard.slnx --configuration Release`

### Run tests
- `dotnet test TaskBoard.slnx --configuration Release --no-build`

### Run services locally
- Projects API: `dotnet run --project src/Projects.Api/Projects.Api.csproj`
- Tasks API: `dotnet run --project src/Tasck.Api/Tasks.Api.csproj`

### Optional publish commands (used by CI)
- `dotnet publish src/Projects.Api/Projects.Api.csproj -c Release`
- `dotnet publish src/Tasck.Api/Tasks.Api.csproj -c Release`

## CI/CD notes
- Workflows run on pushes/PRs and build/test each service.
- Projects workflow uses `src/Projects.Api/**` and `src/Projects.Api.Tests/**` paths.
- Tasks workflow uses `src/Tasks.Api/**` and `src/Tasks.Api.Tests/**` paths.
- Deploy steps target Azure Web Apps from `main` branch pushes.
