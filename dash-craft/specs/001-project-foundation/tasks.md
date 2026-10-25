/spe---

description: "Task list for 001-project-foundation"
---

# Tasks: Project Foundation

**Input**: Design documents from `/specs/001-project-foundation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/health.openapi.yaml, quickstart.md

**Tests**: REQUIRED. The spec (US2, FR-010 to FR-013) and constitution Principle VI make tests
mandatory: xUnit v3 + Shouldly for the backend, Vitest + React Testing Library for the frontend,
and Testcontainers for integration. Within each story, write the test tasks first and confirm they
fail before implementing.

**Organization**: Tasks are grouped by user story so each story can be implemented and verified
on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: The user story this task belongs to (US1, US2, US3)

## Path Conventions

- Backend projects: `src/api/DashCraft.<Layer>/`; solution `src/api/DashCraft.slnx`
- Backend tests: `tests/api/DashCraft.<Layer>.Tests/`, `tests/api/DashCraft.IntegrationTests/`
- Frontend: `src/web/` (feature folders under `src/web/src/features/`)
- Root: `docker-compose.yml`, `Directory.*.props`, `global.json`, `README.md`

## Version pins (from research.md, verified 2026-10-25; use exactly these)

- **NuGet**:
  - Mediator.Abstractions / Mediator.SourceGenerator 3.0.2
  - Riok.Mapperly 4.3.1
  - Microsoft.EntityFrameworkCore.SqlServer / .Design 10.0.13
  - Microsoft.Extensions.Diagnostics.HealthChecks 10.0.13 (MIT; used by the custom database health check)
  - Dapper 2.1.89
  - Microsoft.Data.SqlClient 6.1.7 (NOT 7.x)
  - Microsoft.SqlServer.TransactSql.ScriptDom 180.140.0
  - FluentValidation / FluentValidation.DependencyInjectionExtensions 12.2.0
  - Microsoft.AspNetCore.OpenApi 10.0.13
  - xunit.v3 4.0.2, xunit.runner.visualstudio 4.0.1, Microsoft.NET.Test.Sdk 18.11.0
  - Shouldly 4.3.0
  - Microsoft.AspNetCore.Mvc.Testing 10.0.13, Testcontainers.MsSql 4.16.0
  - Local tool: dotnet-ef 10.0.13
- **npm** (exact versions, no `^`):
  - Runtime libraries:
    - react / react-dom 19.3.1
    - react-router 8.5.0
    - @tanstack/react-query 5.104.5, @tanstack/react-table 9.3.1
    - react-hook-form 7.91.0, zod 4.6.7, @hookform/resolvers 6.0.0
  - Build and styling:
    - vite 8.3.5, @vitejs/plugin-react 6.1.2
    - tailwindcss / @tailwindcss/vite 4.3.3
    - typescript 6.0.3 (NOT 7.x)
  - Testing:
    - vitest 5.0.5, jsdom 30.1.3
    - @testing-library/react 16.3.3, @testing-library/dom 10.4.2
    - @testing-library/jest-dom 7.0.1, @testing-library/user-event 14.6.8
  - Types:
    - @types/react / @types/react-dom 19.3.0
    - @types/node 24.19.2
  - Linting:
    - eslint 10.13.0, @eslint/js 10.0.1, typescript-eslint 8.72.0
    - eslint-plugin-react-hooks 7.1.1, eslint-plugin-react-refresh 0.5.7
    - globals 17.13.0
- **Images**:
  - `mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-24.04`
  - `mcr.microsoft.com/dotnet/sdk:10.0`, `mcr.microsoft.com/dotnet/aspnet:10.0`
  - `node:24.22.1-alpine`, `nginx:1.30.6-alpine`
- **Actions**: `actions/checkout@v7`, `actions/setup-dotnet@v6`, `actions/setup-node@v7`

Do NOT add packages that research.md §2 rejected (Serilog, AspNetCore.HealthChecks.SqlServer,
Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore, Scalar, EF Core Tools, coverlet,
msw, xunit v2), nor MediatR or AutoMapper.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Repository scaffolding, solution, projects, and toolchains

- [ ] T001 [P] Create `LICENSE` at the repo root with the standard MIT License text and `Copyright (c) 2026 The DashCraft Authors`
- [ ] T002 Create `global.json` at the repo root pinning `"sdk": { "version": "10.0.401", "rollForward": "latestFeature" }`
- [ ] T003 [P] Create `Directory.Build.props` at the repo root:
  - `TargetFramework` net10.0, `Nullable` enable, `ImplicitUsings` enable
  - `TreatWarningsAsErrors` true, `LangVersion` latest
  - `ManagePackageVersionsCentrally` true
- [ ] T004 [P] Create `Directory.Packages.props` at the repo root with a `PackageVersion` entry for every NuGet pin listed in "Version pins" above (Central Package Management; no versions in `.csproj` files)
- [ ] T005 [P] Create `.config/dotnet-tools.json` with the local tool `dotnet-ef` 10.0.13 (`dotnet new tool-manifest` + `dotnet tool install dotnet-ef --version 10.0.13`)
- [ ] T006 [P] Create `.editorconfig` at the repo root:
  - `root = true`, UTF-8, final newline, trim trailing whitespace
  - C# 4-space indent with file-scoped namespaces preferred; TS/JSON/YAML/CSS 2-space indent
  - `dotnet_diagnostic.RMG012.severity = error` and `dotnet_diagnostic.RMG020.severity = error` (Mapperly unmapped members fail the build)
- [ ] T007 [P] Create `.gitignore` covering:
  - .NET: `bin/`, `obj/`, `*.user`, `.vs/`, `TestResults/`
  - Node: `node_modules/`, `dist/`, `coverage/`
  - Secrets: `.env` (but NOT `.env.example`)
  - IDE folders, OS files

  Also create `.gitattributes` with `* text=auto eol=lf`, plus `*.ps1 eol=crlf` and `*.cmd eol=crlf`.
- [ ] T008 Create the four backend projects in `src/api/`:
  - `dotnet new classlib`: `DashCraft.Domain`, `DashCraft.Application`, `DashCraft.Infrastructure`
  - `dotnet new web`: `DashCraft.Api`
  - Delete template `Class1.cs` files and remove any `TargetFramework`/`Nullable`/`ImplicitUsings` already set by `Directory.Build.props`
  - Add assembly marker classes:
    - `src/api/DashCraft.Domain/DomainAssembly.cs` (`public static class DomainAssembly`)
    - `src/api/DashCraft.Application/ApplicationAssembly.cs`
    - `src/api/DashCraft.Infrastructure/InfrastructureAssembly.cs`
- [ ] T009 Add project references (dependencies point inward):
  - Application → Domain
  - Infrastructure → Application
  - Api → Application and Infrastructure

  Create `src/api/DashCraft.slnx` (`dotnet new sln --format slnx -n DashCraft`) and add all four projects.
- [ ] T010 Create five xunit.v3 test projects under `tests/api/`:
  - `DashCraft.Domain.Tests`, `DashCraft.Application.Tests`, `DashCraft.Infrastructure.Tests`, `DashCraft.Api.Tests`, `DashCraft.IntegrationTests`
  - Each references `xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `Shouldly` without versions, and `OutputType` Exe as xunit.v3 requires
  - Each unit test project references its matching layer project; `DashCraft.IntegrationTests` references `DashCraft.Api` and `DashCraft.Infrastructure`
  - Add all five to `src/api/DashCraft.slnx` under a `tests` solution folder
  - Add `<Using Include="Shouldly" />` and `<Using Include="Xunit" />` in each test project
- [ ] T011 Add NuGet package references per research.md §4 (no versions):
  - `DashCraft.Application`: Mediator.Abstractions, FluentValidation, FluentValidation.DependencyInjectionExtensions, Riok.Mapperly
  - `DashCraft.Infrastructure`: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.Extensions.Diagnostics.HealthChecks, Dapper, Microsoft.Data.SqlClient, Microsoft.SqlServer.TransactSql.ScriptDom
  - `DashCraft.Api`: Mediator.SourceGenerator (`PrivateAssets=all`, `IncludeAssets` runtime;build;native;contentfiles;analyzers), Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.Design (`PrivateAssets=all`)
  - `DashCraft.IntegrationTests`: Microsoft.AspNetCore.Mvc.Testing, Testcontainers.MsSql
  - `DashCraft.Domain`: nothing

  Run `dotnet build src/api/DashCraft.slnx` and confirm 0 warnings, 0 errors.
- [ ] T012 Create `src/web/package.json`:
  - `name` "dashcraft-web", `private` true, `license` "MIT", `type` "module", `engines.node` ">=24.15"
  - Exact npm pins from "Version pins": runtime libraries in `dependencies`, the rest in `devDependencies`
  - Scripts:
    - `dev`: `vite`
    - `build`: `tsc -b && vite build`
    - `preview`: `vite preview`
    - `lint`: `eslint . --max-warnings 0`
    - `typecheck`: `tsc -b --noEmit`
    - `test`: `vitest run`
    - `test:watch`: `vitest`

  Run `npm install` in `src/web` to produce `src/web/package-lock.json`.
- [ ] T013 [P] Create the TypeScript configs in `src/web/`:
  - `tsconfig.json` with project references to `tsconfig.app.json` and `tsconfig.node.json`
  - `tsconfig.app.json` for `src/` and `tests/`:
    - `strict`, `noUncheckedIndexedAccess`, `noUnusedLocals`, `noUnusedParameters`, `noFallthroughCasesInSwitch`
    - `jsx` react-jsx, `moduleResolution` bundler, `target` ES2022
    - `types`: `["vite/client", "vitest/globals", "@testing-library/jest-dom"]`
  - `tsconfig.node.json` for `vite.config.ts` and `eslint.config.js`
- [ ] T014 [P] Create `src/web/eslint.config.js` (flat config):
  - `@eslint/js` recommended, `typescript-eslint` `strictTypeChecked` with `parserOptions.projectService`
  - `eslint-plugin-react-hooks` recommended, `eslint-plugin-react-refresh` (`only-export-components` warn)
  - `@typescript-eslint/no-explicit-any: "error"`; `globals.browser`
  - Ignore `dist`, `coverage`
- [ ] T015 [P] Create `src/web/vite.config.ts`:
  - Plugins: `@vitejs/plugin-react` and `@tailwindcss/vite`
  - `server.proxy` forwards `/health` and `/api` to `process.env.VITE_API_PROXY_TARGET ?? "http://localhost:5080"`
  - Vitest `test` block: `environment: "jsdom"`, `globals: true`, `setupFiles: ["./tests/setup.ts"]`, `include: ["src/**/*.test.{ts,tsx}", "tests/**/*.test.{ts,tsx}"]`
- [ ] T016 [P] Create the frontend entry files:
  - `src/web/index.html`: title "DashCraft", `<div id="root">`, module script `/src/main.tsx`
  - `src/web/src/index.css`: `@import "tailwindcss";`
  - `src/web/tests/setup.ts`: `import "@testing-library/jest-dom/vitest";` plus `afterEach(cleanup)`
  - `src/web/src/vite-env.d.ts`: declares `VITE_API_BASE_URL?: string`
- [ ] T017 [P] Create the frontend folder skeleton per the constitution, each with a short `README.md` stating what belongs there:
  - `src/web/src/app/`, `src/web/src/routes/`, `src/web/src/features/`, `src/web/src/components/`, `src/web/src/lib/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: App database plumbing, DI composition, error handling, frontend shell, and the
integration test harness. Every user story depends on these.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T018 [P] Create `src/api/DashCraft.Infrastructure/Persistence/DatabaseOptions.cs`:
  - Options class bound to section `"Database"`, with `bool ApplyMigrationsOnStartup` (default false), `int StartupMaxRetries` (default 12) and `TimeSpan StartupRetryDelay` (default 5 s)
  - Connection string name constant `public const string ConnectionStringName = "DashCraft";`
- [ ] T019 [P] Create `src/api/DashCraft.Infrastructure/Persistence/DashCraftDbContext.cs`:
  - `public sealed class DashCraftDbContext(DbContextOptions<DashCraftDbContext> options) : DbContext(options)`
  - No `DbSet`s
  - `OnModelCreating` sets `HasDefaultSchema("dbo")` and applies configurations from `InfrastructureAssembly`
- [ ] T020 [P] Create `src/api/DashCraft.Infrastructure/Persistence/DashCraftDbContextFactory.cs`:
  - Implements `IDesignTimeDbContextFactory<DashCraftDbContext>` for `dotnet ef`
  - Uses `UseSqlServer` with a non-secret placeholder connection string (`Server=localhost;Database=DashCraft;Trusted_Connection=False;TrustServerCertificate=True`); generating a migration never connects
- [ ] T021 Create `src/api/DashCraft.Infrastructure/DependencyInjection.cs` with `public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)`:
  - Binds `DatabaseOptions` (`ValidateOnStart`)
  - Reads `ConnectionStrings:DashCraft`; throws a clear `InvalidOperationException` ("Connection string 'DashCraft' is not configured.") if missing, without logging it
  - Registers `DashCraftDbContext` with `UseSqlServer(cs, sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null))`

  Depends on T018, T019.
- [ ] T022 Generate the empty baseline migration:
  - Run `dotnet tool restore`, then `dotnet ef migrations add InitialCreate --project src/api/DashCraft.Infrastructure --startup-project src/api/DashCraft.Api --output-dir Persistence/Migrations`
  - Output lands in `src/api/DashCraft.Infrastructure/Persistence/Migrations/`
  - Verify `Up`/`Down` are empty and a model snapshot exists

  Depends on T019, T020.
- [ ] T023 Create the background migration runner so the API serves `/health` (as 503) while it waits for the database (spec edge case "Database not yet ready at startup"):
  - **`src/api/DashCraft.Infrastructure/Persistence/DatabaseReadiness.cs`**: singleton `public sealed class DatabaseReadiness` with `bool IsReady` (thread-safe via `Volatile`), `MarkReady()`, and constructor logic setting it ready immediately when `ApplyMigrationsOnStartup` is false
  - **`src/api/DashCraft.Infrastructure/Persistence/MigrationHostedService.cs`**: `internal sealed class MigrationHostedService : BackgroundService`
    - `ExecuteAsync` starts with `await Task.Yield()` so it never blocks host startup or Kestrel
    - Does nothing unless `DatabaseOptions.ApplyMigrationsOnStartup` is true
    - Creates a scope and calls `db.Database.MigrateAsync(stoppingToken)`, then `readiness.MarkReady()`
    - On `SqlException` or `InvalidOperationException`, retries up to `StartupMaxRetries` times with `StartupRetryDelay`, logging `"Waiting for application database (attempt {Attempt}/{Max})"` at Warning without the connection string
    - After the last failed attempt, logs at Critical and calls `IHostApplicationLifetime.StopApplication()` so the container restart policy takes over
    - Logs `"Application database is up to date"` at Information on success
  - Register both in `AddInfrastructure` (`AddSingleton<DatabaseReadiness>()`, `AddHostedService<MigrationHostedService>()`)

  Depends on T018, T021.
- [ ] T024 [P] Create `tests/api/DashCraft.Infrastructure.Tests/Persistence/DatabaseReadinessTests.cs`:
  - With `ApplyMigrationsOnStartup=false`, `IsReady` is true immediately
  - With it true, `IsReady` is false until `MarkReady()` is called
- [ ] T025 [P] Create `tests/api/DashCraft.Application.Tests/Common/Behaviors/ValidationBehaviorTests.cs` (write before T026 and confirm it fails). Define a test-only `sealed record PingCommand(string Name) : ICommand<string>` and an inline `AbstractValidator<PingCommand>` requiring `Name` non-empty, then call `ValidationBehavior.Handle` with a `next` delegate that records invocation:
  - Invalid input throws `ValidationException` containing the `Name` failure, and `next` is not called
  - Valid input returns `next`'s result
  - No validators means `next` is called
- [ ] T026 [P] Create `src/api/DashCraft.Application/Common/Behaviors/ValidationBehavior.cs`:
  - `public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators) : IPipelineBehavior<TMessage, TResponse> where TMessage : notnull, IMessage`
  - Runs all validators with the cancellation token; if any failures, throws `FluentValidation.ValidationException` with all failures; otherwise calls `next`
- [ ] T027 Create `src/api/DashCraft.Application/DependencyInjection.cs` with `public static IServiceCollection AddApplication(this IServiceCollection services)`:
  - Registers validators via `AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly, includeInternalTypes: true)`
  - Does NOT call `AddMediator` (the source generator lives in Api)

  Depends on T026.
- [ ] T028 [P] Create `tests/api/DashCraft.Api.Tests/ErrorHandling/ValidationExceptionHandlerTests.cs` (write before T029 and confirm it fails):
  - Build a minimal service provider with `AddProblemDetails()` and a `DefaultHttpContext`
  - Assert that handling a `ValidationException` with two failures on `Name` writes status 400 and a ProblemDetails body whose `errors.Name` has both messages
  - Assert that a non-validation exception returns `false` (not handled)
- [ ] T029 [P] Create `src/api/DashCraft.Api/ErrorHandling/ValidationExceptionHandler.cs`:
  - `IExceptionHandler` that handles `FluentValidation.ValidationException`
  - Writes a 400 ProblemDetails (`title` "One or more validation errors occurred.") with an `errors` dictionary grouped by property name, via `IProblemDetailsService`
- [ ] T030 [P] Create `src/api/DashCraft.Api/ErrorHandling/UnhandledExceptionHandler.cs`:
  - `IExceptionHandler` that logs the exception at Error and writes a 500 ProblemDetails with `title` "Something went wrong on our side. Please try again."
  - Includes `detail` (exception message) only when `IHostEnvironment.IsDevelopment()`; always includes `traceId`
- [ ] T031 Create `src/api/DashCraft.Api/Program.cs`:
  - **Services**:
    - `AddProblemDetails()`; register the exception handlers (validation first)
    - `AddOpenApi()`
    - `AddApplication()`, `AddInfrastructure(builder.Configuration)`
    - `AddMediator(o => { o.ServiceLifetime = ServiceLifetime.Scoped; o.Assemblies = [typeof(ApplicationAssembly)]; o.PipelineBehaviors = [typeof(ValidationBehavior<,>)]; })`
  - **Middleware**: `UseExceptionHandler()`, `UseStatusCodePages()`; `MapOpenApi()` only in Development
  - **Startup**: no blocking database work before `RunAsync`; migrations run in `MigrationHostedService` (T023)
  - End with `public partial class Program;` for `WebApplicationFactory`
  - Keep the template's `MapGet("/")` out

  Depends on T021, T023, T027, T029, T030.
- [ ] T032 [P] Create `src/api/DashCraft.Api/appsettings.json` and `src/api/DashCraft.Api/appsettings.Development.json`:
  - **`appsettings.json`**:
    - Logging levels (Default Information, Microsoft.AspNetCore Warning, Microsoft.EntityFrameworkCore.Database.Command Warning)
    - `"Database": { "ApplyMigrationsOnStartup": false }`
    - `AllowedHosts` "*"
    - No connection string
  - **`appsettings.Development.json`**:
    - `ConnectionStrings:DashCraft` = `Server=localhost,14330;Database=DashCraft;User Id=sa;Password=DashCraft_Dev_Passw0rd!;TrustServerCertificate=True;Encrypt=True`
    - `Database:ApplyMigrationsOnStartup` true

  The password is the documented development-only default, the same as compose. Also update `Properties/launchSettings.json` to use `http://localhost:5080` only.
- [ ] T033 [P] Create `tests/api/DashCraft.IntegrationTests/Infrastructure/SqlServerFixture.cs`:
  - xunit.v3 assembly fixture (`[assembly: AssemblyFixture(typeof(SqlServerFixture))]`) implementing `IAsyncLifetime`
  - Starts `new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-24.04").Build()` and exposes `ConnectionString` (with `Database=DashCraft` set via `SqlConnectionStringBuilder`)
  - If Docker is unavailable, catch the Testcontainers exception and throw `InvalidOperationException("Integration tests need a running Docker engine. Start Docker Desktop and try again.", inner)`
- [ ] T034 Create `tests/api/DashCraft.IntegrationTests/Infrastructure/DashCraftApiFactory.cs`:
  - `WebApplicationFactory<Program>` taking a connection string plus optional `applyMigrations` (default true) and `startupMaxRetries` (default 1)
  - Sets environment "Testing"; uses `ConfigureAppConfiguration` to add in-memory `ConnectionStrings:DashCraft`, `Database:ApplyMigrationsOnStartup`, `Database:StartupMaxRetries`, `Database:StartupRetryDelay=00:00:01`
  - Exposes `CreateClient()` and `Task<HttpResponseMessage> WaitUntilHealthyAsync(TimeSpan timeout)`, which polls `GET /health` every 500 ms until 200 and fails with the last body if the timeout (default 60 s) passes. Migrations now run in the background, so tests must not assume readiness immediately after startup.

  Depends on T031, T033.
- [ ] T035 [P] Create `src/web/src/lib/apiClient.ts`:
  - `export class ApiError extends Error` with `status: number` and an optional `problem` (parsed ProblemDetails)
  - `export async function apiGet<T>(path: string, schema: z.ZodType<T>, options?: { signal?: AbortSignal; acceptStatuses?: number[] }): Promise<T>`:
    - Prefixes `import.meta.env.VITE_API_BASE_URL ?? ""`, sends `Accept: application/json`
    - Treats `response.ok` or any status in `acceptStatuses` as success and parses the JSON with `schema.parse`
    - Otherwise throws `ApiError`
  - Network errors propagate as `TypeError`; no `any`
- [ ] T036 [P] Create `src/web/src/app/providers.tsx`:
  - `AppProviders({ children })` creating one `QueryClient` (defaults: `retry: 1`, `refetchOnWindowFocus: false`) inside `useState`, wrapping children in `QueryClientProvider`
  - Also export `createTestQueryClient()` (`retry: false`) from `src/web/src/app/queryClient.ts` for tests
- [ ] T037 Create `src/web/src/routes/router.tsx`, `src/web/src/app/App.tsx`, and `src/web/src/main.tsx`:
  - `router.tsx` exports `routes: RouteObject[]` (central route table, initially `[]`) and `createAppRouter()` using `createBrowserRouter(routes)`
  - `App.tsx` renders `<AppProviders><RouterProvider router={...} /></AppProviders>`
  - `main.tsx` imports `./index.css` and renders `<StrictMode><App /></StrictMode>` into `#root`

  Depends on T036.

**Checkpoint**: `dotnet build src/api/DashCraft.slnx` succeeds with no warnings, the Phase 2 unit tests (T024 to T028) pass, `npm run typecheck` passes in `src/web`, and the `InitialCreate` migration exists. User stories can begin.

---

## Phase 3: User Story 1 - Start the whole system with one command (Priority: P1) 🎯 MVP

**Goal**: `docker compose up` starts SQL Server, the API, and the web app. `/health` reports the
database status per [contracts/health.openapi.yaml](contracts/health.openapi.yaml), and the home
page shows the API status.

**Independent Test**:
1. From a fresh clone, run `docker compose up -d --build`.
2. Within 2 minutes `curl -i http://localhost:5080/health` returns 200 with a `database` check that is `Healthy`.
3. `http://localhost:8080` shows the DashCraft page with a healthy API status.
4. Stopping `sqlserver` makes `/health` return 503.

### Tests for User Story 1 (write first, confirm they fail) ⚠️

- [ ] T038 [P] [US1] Create `tests/api/DashCraft.Api.Tests/Health/HealthResponseWriterTests.cs`. Using a `DefaultHttpContext` with a `MemoryStream` body and hand-built `HealthReport`s, assert:
  - Content type is `application/json; charset=utf-8`
  - JSON has exactly `status`, `totalDurationMs`, `checks`, with camelCase names and string enum values `Healthy`/`Degraded`/`Unhealthy`
  - Each check has `name`, `status`, `durationMs`, and `description` only when present
  - An Unhealthy `database` entry whose exception message contains "Login failed for user 'sa'" and "Server=secret-host" is rendered with `description` = `"The application database is not reachable."`, and the body contains neither "Login failed" nor "secret-host"

  This is the no-leak test required by constitution Principle II, Health endpoint exception.
- [ ] T039 [P] [US1] Create `tests/api/DashCraft.Infrastructure.Tests/Health/DatabaseHealthCheckTests.cs`. With a `DatabaseReadiness` that is not ready and a `DashCraftDbContext` built with `UseSqlServer("Server=unused;Database=DashCraft")`, `CheckHealthAsync` returns `Unhealthy` without attempting a connection (it completes in under 1 s)
- [ ] T040 [P] [US1] Create `tests/api/DashCraft.Infrastructure.Tests/Persistence/DashCraftDbContextTests.cs`. Build `DashCraftDbContext` with `UseSqlServer("Server=unused;Database=DashCraft")` (no connection is opened) and assert:
  - `context.Model` builds with default schema `dbo`
  - `context.Database.GetMigrations()` contains exactly one migration ending in `_InitialCreate`
- [ ] T041 [P] [US1] Create `tests/api/DashCraft.IntegrationTests/Health/HealthEndpointTests.cs`, using `SqlServerFixture` + `DashCraftApiFactory`:
  - `factory.WaitUntilHealthyAsync(...)` succeeds, and the final `GET /health` returns 200 with content type `application/json`
  - Body deserializes to the contract shape with `status == "Healthy"` and a single check `name == "database"`, `status == "Healthy"`
  - `totalDurationMs >= 0`
- [ ] T042 [P] [US1] Create `tests/api/DashCraft.IntegrationTests/Persistence/MigrationTests.cs`:
  - After `WaitUntilHealthyAsync`, `SELECT MigrationId FROM dbo.__EFMigrationsHistory` (via `Microsoft.Data.SqlClient`) returns one row ending in `_InitialCreate`
  - Starting a second factory against the same database succeeds and still yields exactly one row (idempotent, FR-006)
- [ ] T043 [P] [US1] Create `tests/api/DashCraft.IntegrationTests/Health/DatabaseUnavailableTests.cs`. Use a `DashCraftApiFactory` with connection string `Server=127.0.0.1,1;Database=DashCraft;User Id=sa;Password=x;Connect Timeout=2;TrustServerCertificate=True` and `ApplyMigrationsOnStartup=false`. No container is needed. Assert:
  - `GET /health` returns 503 with `status == "Unhealthy"` and a `database` check that is `Unhealthy` with description `"The application database is not reachable."`
  - The body does not contain "127.0.0.1", "Password", or "Exception" (FR-005, no leaked details)
- [ ] T044 [P] [US1] Create `tests/api/DashCraft.IntegrationTests/Health/StartupAvailabilityTests.cs` (spec edge case "Database not yet ready at startup"). Use a `DashCraftApiFactory` with the same unreachable connection string as T043, `applyMigrations: true`, and `startupMaxRetries: 30`. No container is needed. Assert:
  - While migrations are still retrying, `GET /health` responds (no connection error) with 503 and a `database` check that is `Unhealthy`
  - The host has not stopped
- [ ] T045 [P] [US1] Create `src/web/src/features/home/components/HomePage.test.tsx`. Render `HomePage` inside `QueryClientProvider` (from `createTestQueryClient()`) and `MemoryRouter`, stubbing `fetch` with `vi.stubGlobal`:
  - **Healthy**: a 200 healthy body shows heading "DashCraft" and text "API: Healthy"
  - **Unhealthy**: a 503 body with database Unhealthy shows "API: Unhealthy" and "The application database is not reachable."
  - **Unreachable**: `fetch` rejects with `TypeError`, so the page shows "We can't reach the DashCraft API right now. Check that it is running." and the page still renders its heading
  - **Malformed**: a 200 body `{ "foo": 1 }` is treated as unreachable

### Implementation for User Story 1

- [ ] T046 [US1] Create `src/api/DashCraft.Infrastructure/Health/DatabaseHealthCheck.cs` (`internal sealed class DatabaseHealthCheck(DashCraftDbContext db, DatabaseReadiness readiness) : IHealthCheck`):
  - Returns `Unhealthy` without touching the database when `!readiness.IsReady`
  - Otherwise returns `Healthy` if `await db.Database.CanConnectAsync(ct)`, else `Unhealthy`
  - Catches exceptions and returns `Unhealthy` (the writer never exposes the exception)

  Register it in `AddInfrastructure` in `src/api/DashCraft.Infrastructure/DependencyInjection.cs` with `services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database", failureStatus: HealthStatus.Unhealthy, tags: ["db"], timeout: TimeSpan.FromSeconds(5))`. Makes T039 pass.
- [ ] T047 [US1] Create `src/api/DashCraft.Api/Health/HealthResponseWriter.cs`: `public static class HealthResponseWriter` with `public static Task WriteAsync(HttpContext context, HealthReport report)` writing the JSON contract with `System.Text.Json` (camelCase, string enums):
  - `durationMs` and `totalDurationMs` rounded to 1 decimal
  - `description` for a non-Healthy entry comes from a fixed map (`"database"` → `"The application database is not reachable."`, otherwise `"This dependency is not available."`); never use `entry.Description` or `entry.Exception`

  Makes T038 pass.
- [ ] T048 [US1] Create `src/api/DashCraft.Api/Endpoints/HealthEndpoints.cs` with `public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)`:
  - Maps `app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthResponseWriter.WriteAsync, ResultStatusCodes = { [Healthy] = 200, [Degraded] = 200, [Unhealthy] = 503 } })`
  - Adds `.AllowAnonymous()` with the comment `// Anonymous per constitution Principle II, Health endpoint exception (no-leak test: HealthResponseWriterTests).`

  Call `app.MapHealthEndpoints()` from `src/api/DashCraft.Api/Program.cs`. Makes T041 to T043 and T044 pass.
- [ ] T049 [P] [US1] Create `src/web/src/features/health/schemas/health.ts` with Zod schemas mirroring data-model.md and export inferred types via `z.infer`:
  - `healthStatusSchema = z.enum(["Healthy", "Degraded", "Unhealthy"])`
  - `healthCheckResultSchema` (`name: z.string()`, `status`, `description: z.string().optional()`, `durationMs: z.number().nonnegative()`)
  - `healthReportSchema` (`status`, `totalDurationMs: z.number().nonnegative()`, `checks: z.array(healthCheckResultSchema)`)
- [ ] T050 [US1] Create the health API layer:
  - `src/web/src/features/health/api/getHealth.ts`: `getHealth(signal)` returns `apiGet("/health", healthReportSchema, { signal, acceptStatuses: [503] })`
  - `src/web/src/features/health/api/useHealth.ts`: `useHealth()` returns `useQuery({ queryKey: ["health"], queryFn: ({ signal }) => getHealth(signal), refetchInterval: 10_000 })`

  Depends on T035, T049.
- [ ] T051 [P] [US1] Create `src/web/src/components/StatusBadge.tsx`:
  - Shared primitive `StatusBadge({ tone: "success" | "warning" | "danger" | "neutral", children })`
  - Tailwind pill with text plus color (never color alone), and `aria-hidden` dot
- [ ] T052 [US1] Create `src/web/src/features/health/components/HealthStatus.tsx`:
  - Uses `useHealth()` inside a `role="status" aria-live="polite"` region
  - Loading: "Checking API status…"
  - Success: `StatusBadge` "API: {status}" (Healthy→success, Degraded→warning, Unhealthy→danger) and a list of non-Healthy checks with their `description`
  - Error (network, `ApiError`, or `ZodError`): "We can't reach the DashCraft API right now. Check that it is running."

  Depends on T050, T051.
- [ ] T053 [US1] Create `src/web/src/features/home/components/HomePage.tsx`: responsive centered Tailwind layout with `<h1>DashCraft</h1>`, the one-line description "Build dashboards from your SQL Server data without writing SQL.", and `<HealthStatus />`. Makes T045 pass.
- [ ] T054 [US1] Create the routes and register them in `routes` in `src/web/src/routes/router.tsx`:
  - `src/web/src/routes/HomeRoute.tsx` (thin, renders `HomePage`)
  - `src/web/src/routes/NotFoundRoute.tsx` ("Page not found" + link home)
  - Routes: `{ path: "/", element: <HomeRoute /> }` and `{ path: "*", element: <NotFoundRoute /> }`
- [ ] T055 [P] [US1] Create `src/api/Dockerfile` (build context = repo root), multi-stage:
  - **Build stage**: `mcr.microsoft.com/dotnet/sdk:10.0` with `ENV DOTNET_CLI_TELEMETRY_OPTOUT=1` (FR-015); copy `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, then the 4 `src/api/DashCraft.*/*.csproj`; `dotnet restore src/api/DashCraft.Api/DashCraft.Api.csproj`; copy `src/api/`; `dotnet publish src/api/DashCraft.Api -c Release -o /app --no-restore`
  - **Runtime stage**: `mcr.microsoft.com/dotnet/aspnet:10.0` with `ENV ASPNETCORE_HTTP_PORTS=8080`, `EXPOSE 8080`, `USER app`, `ENTRYPOINT ["dotnet","DashCraft.Api.dll"]`

  Add a root `.dockerignore` excluding `**/bin`, `**/obj`, `**/node_modules`, `**/dist`, `.git`, `.env`, `tests/`, `specs/`.
- [ ] T056 [P] [US1] Create `src/web/Dockerfile`, `src/web/nginx.conf`, and `src/web/.dockerignore` (build context = `src/web`):
  - **Dockerfile**:
    - Build stage `node:24.22.1-alpine`: `npm ci`, `npm run build`
    - Runtime stage `nginx:1.30.6-alpine`: copy `dist` to `/usr/share/nginx/html` and `nginx.conf` to `/etc/nginx/conf.d/default.conf`; `EXPOSE 8080`
  - **nginx.conf**:
    - `listen 8080`, gzip on
    - `location /health` and `location /api/` → `proxy_pass http://api:8080` with `proxy_set_header Host`/`X-Forwarded-*`
    - `location /` → `try_files $uri /index.html`
  - **.dockerignore**: `node_modules`, `dist`, `coverage`
- [ ] T057 [US1] Create `docker-compose.yml` at the repo root (project `name: dashcraft`):
  - **`sqlserver`**:
    - Image `mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-24.04`
    - Env: `ACCEPT_EULA: "Y"`, `MSSQL_PID: Developer`, `MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:-DashCraft_Dev_Passw0rd!}`
    - Ports `${SQL_PORT:-14330}:1433`; volume `dashcraft-sqldata:/var/opt/mssql`
    - Healthcheck `["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q \"SELECT 1\" -b -o /dev/null"]`, interval 10s, timeout 5s, retries 12, start_period 30s
  - **`api`**:
    - Build context `.`, dockerfile `src/api/Dockerfile`
    - Env:
      - `ConnectionStrings__DashCraft: "Server=sqlserver,1433;Database=DashCraft;User Id=sa;Password=${MSSQL_SA_PASSWORD:-DashCraft_Dev_Passw0rd!};TrustServerCertificate=True;Encrypt=True"`
      - `Database__ApplyMigrationsOnStartup: "true"`
      - `Logging__Console__FormatterName: json`
      - `ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}`
    - Ports `${API_PORT:-5080}:8080`; `depends_on: sqlserver: condition: service_healthy`; `restart: unless-stopped`
  - **`web`**: build `src/web`; ports `${WEB_PORT:-8080}:8080`; `depends_on: [api]`; `restart: unless-stopped`
  - Top-level `volumes: dashcraft-sqldata: {}`
- [ ] T058 [P] [US1] Create `.env.example` at the repo root, with a header comment "Copy to .env. Development defaults only; change MSSQL_SA_PASSWORD for any shared or production use. SQL Server requires 8+ chars with upper, lower, digit and symbol.":
  - `MSSQL_SA_PASSWORD=DashCraft_Dev_Passw0rd!`
  - `SQL_PORT=14330`, `API_PORT=5080`, `WEB_PORT=8080`
  - `ASPNETCORE_ENVIRONMENT=Production`
- [ ] T059 [US1] Validate US1 end-to-end per quickstart.md Scenarios 1 to 3:
  - `docker compose up -d --build`, then confirm `/health` 200 on ports 5080 and 8080, the page renders "API: Healthy", and `__EFMigrationsHistory` has `InitialCreate`
  - Stop and restart `sqlserver` and observe 503 then 200
  - Run `docker compose down` + `up -d` and confirm no data loss
  - Fix any defects found

**Checkpoint**: US1 is fully functional. `dotnet test tests/api/DashCraft.IntegrationTests`, `dotnet test tests/api/DashCraft.Api.Tests`, `dotnet test tests/api/DashCraft.Infrastructure.Tests`, and `npm test` all pass.

---

## Phase 4: User Story 2 - Run the automated tests (Priority: P2)

**Goal**: Every backend project has a passing sample test. Unit, integration, and frontend suites
each run with one documented command and fail non-zero on any failure, and CI runs all of it.

**Independent Test**: From a fresh clone, run the three commands in quickstart.md Scenario 4.
Each exits 0, and each backend test project reports at least one passing test. Break one
assertion and confirm a non-zero exit.

### Tests for User Story 2

- [ ] T060 [P] [US2] Create `tests/api/DashCraft.Domain.Tests/ArchitectureTests.cs`: `typeof(DomainAssembly).Assembly.GetReferencedAssemblies()` contains no name starting with `"DashCraft."` (constitution: Domain has no dependencies on other layers)
- [ ] T061 [P] [US2] Create `tests/api/DashCraft.Application.Tests/ArchitectureTests.cs`: the Application assembly references `DashCraft.Domain` but not `DashCraft.Infrastructure` or `DashCraft.Api`, and does not reference `Microsoft.EntityFrameworkCore` or `Dapper`
- [ ] T062 [P] [US2] Create `tests/api/DashCraft.Infrastructure.Tests/ArchitectureTests.cs`: the Infrastructure assembly does not reference `DashCraft.Api` or `Microsoft.AspNetCore.Mvc*`

### Implementation for User Story 2

- [ ] T063 [US2] Make the unit test command exclude integration tests: verify `dotnet test src/api/DashCraft.slnx --filter "FullyQualifiedName!~DashCraft.IntegrationTests"` runs all four unit test projects with Docker stopped. If xunit.v3 + VSTest filtering misbehaves, add `src/api/DashCraft.Unit.slnf` (solution filter listing the 4 layer projects and 4 unit test projects) and use `dotnet test src/api/DashCraft.Unit.slnf` instead; update quickstart.md Scenario 4 to match.
- [ ] T064 [US2] Confirm failure propagation (FR-013):
  - Temporarily break one assertion in each suite (backend unit, integration, frontend) and confirm `dotnet test` / `npm test` exit non-zero, then revert
  - Confirm that with Docker stopped the integration run fails fast with the T033 message rather than hanging
- [ ] T065 [US2] Create `.github/workflows/ci.yml` (name `CI`, on `push` to `master` and `pull_request`; `permissions: contents: read`; `concurrency` cancels superseded runs; workflow-level `env: DOTNET_CLI_TELEMETRY_OPTOUT: "1"`, `DOTNET_NOLOGO: "1"`):
  - **`backend`** (ubuntu-latest):
    - `actions/checkout@v7`, `actions/setup-dotnet@v6` with `global-json-file: global.json`
    - `dotnet restore src/api/DashCraft.slnx`
    - `dotnet build src/api/DashCraft.slnx -c Release --no-restore`
    - Unit tests with the T063 command (`-c Release --no-build`)
    - `dotnet test tests/api/DashCraft.IntegrationTests -c Release --no-build`
  - **`frontend`** (ubuntu-latest, `working-directory: src/web`):
    - `actions/setup-node@v7` with `node-version: 24`, `cache: npm`, `cache-dependency-path: src/web/package-lock.json`
    - `npm ci`, `npm run lint`, `npm run typecheck`, `npm test`, `npm run build`
  - **`docker`** (ubuntu-latest, needs nothing): `docker compose build`

  GitHub branch protection can't be configured from repo files. The README (T066, "Contributing and CI") MUST tell maintainers to enable a branch protection rule on `master` requiring the `backend`, `frontend`, and `docker` checks (constitution: "CI MUST pass before merge").

**Checkpoint**: All 5 backend test projects and the frontend suite pass locally, failures exit non-zero, and the CI workflow is valid YAML that mirrors the local commands.

---

## Phase 5: User Story 3 - Understand the project from the README (Priority: P3)

**Goal**: A new contributor can start the system, run every test suite, and find where code
belongs using only the README.

**Independent Test**: Follow only `README.md` from a fresh clone (quickstart.md Scenario 5). The
system starts, all three test suites pass, and the reader can say where backend, frontend, and
test code live.

### Implementation for User Story 3

- [ ] T066 [US3] Write `README.md` at the repo root with these sections (FR-014):
  1. **What is DashCraft**: one paragraph, plus a note that the project is early-stage.
  2. **Prerequisites**: Docker Desktop or another engine with Compose v2+; .NET SDK 10.0.401+; Node.js 24 LTS (>= 24.15); Git.
  3. **Quick start**: clone, optional `cp .env.example .env`, `docker compose up -d --build`, and the URLs: web <http://localhost:8080>, health <http://localhost:5080/health>, OpenAPI `/openapi/v1.json` (Development only).
  4. **Stopping and resetting**: `docker compose down`; `docker compose down -v` wipes local data.
  5. **Configuration**: the `.env` variables, how to change ports, and the development-only SA password warning.
  6. **Running locally without containers**: SQL Server via `docker compose up -d sqlserver`, `dotnet run --project src/api/DashCraft.Api`, `cd src/web && npm ci && npm run dev`.
  7. **Running tests**: the exact unit, integration (needs Docker), and frontend commands from T063 and quickstart.md.
  8. **Database migrations**: `dotnet tool restore`, then `dotnet ef migrations add <Name> --project src/api/DashCraft.Infrastructure --startup-project src/api/DashCraft.Api --output-dir Persistence/Migrations`.
  9. **Repository layout**: a tree matching plan.md, with one line per folder.
  10. **Troubleshooting**: port in use; Docker not running; SQL Server password complexity; health 503 means the database is unreachable or migrations are still running at startup.
  11. **Contributing and CI**: what the `CI` workflow runs, and the one-time maintainer step of enabling branch protection on `master` requiring the `backend`, `frontend`, and `docker` checks.
  12. **License**: MIT (see `LICENSE`); third-party licenses verified in `specs/001-project-foundation/research.md`; the SQL Server container image runs under Microsoft's EULA (Developer edition is for development and testing only).
- [ ] T067 [P] [US3] Cross-check documentation consistency:
  - Ports, commands, and paths in `README.md` match `docker-compose.yml`, `.env.example`, `src/web/vite.config.ts`, `launchSettings.json`, and `specs/001-project-foundation/quickstart.md`
  - Fix any mismatch in the file that is wrong
- [ ] T068 [US3] Do a README walkthrough (quickstart.md Scenario 5) from a clean clone in a temp directory: `git clone` the local repo, follow `README.md` verbatim, and note and fix every step that needs knowledge not written in the README

**Checkpoint**: All three user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification against the spec, plan, and constitution

- [ ] T069 [P] Security and secrets review:
  - `git status --ignored` shows `.env` ignored
  - `git grep -n "Password="` finds only the documented development-only default in `appsettings.Development.json`, `docker-compose.yml`, `.env.example`, and test fixtures
  - API logs from `docker compose logs api` contain no connection string or password
  - `/health` bodies contain no host names or exception text
- [ ] T070 [P] Constitution compliance pass:
  - No MVC controllers (`git grep -n "ControllerBase"` is empty), no `any` in `src/web/src` (lint passes)
  - Domain has no package references, Mapperly diagnostics are errors in `.editorconfig`, and every package version comes from `Directory.Packages.props` (no `Version=` attributes in `.csproj`)
  - `/health` is the only anonymous endpoint (`git grep -n "AllowAnonymous"` returns only `HealthEndpoints.cs`, with the constitution comment)
  - FR-015 no telemetry or cloud dependency:
    - `npm ls --all` in `src/web` and `dotnet list src/api/DashCraft.slnx package --include-transitive` show no analytics or telemetry SDKs (e.g., Application Insights, OpenTelemetry exporters, Sentry, Segment)
    - `git grep -nE "https?://"` in `src/` finds no external endpoints other than documentation links
    - With the system running and the host offline (Wi-Fi off), `/health` still returns 200 and the page loads
  - `LICENSE` exists and is MIT
- [ ] T071 Full clean verification from repo root:
  - `dotnet build src/api/DashCraft.slnx -c Release` (0 warnings)
  - The unit test command, then `dotnet test tests/api/DashCraft.IntegrationTests`
  - In `src/web`: `npm ci && npm run lint && npm run typecheck && npm test && npm run build`
  - `docker compose build`
  - Record the results
- [ ] T072 Run every scenario in `specs/001-project-foundation/quickstart.md` (1 to 6; for 6, validate the workflow syntax locally, e.g. `actionlint` if available, or note that it must be confirmed on first push), and confirm SC-001 to SC-006
- [ ] T073 If any pinned version or design decision changed during implementation, update `specs/001-project-foundation/research.md` and `specs/001-project-foundation/plan.md` to match the delivered state

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; BLOCKS all user stories
- **US1 (Phase 3)**: depends on Foundational
- **US2 (Phase 4)**: depends on Foundational. T065 (CI) and T063/T064 are most meaningful after US1, because the integration and frontend suites get their real tests in US1, but US2's own tests (T060, T061, T062) do not depend on US1
- **US3 (Phase 5)**: depends on US1 and US2 being in place to document accurately (it documents their commands)
- **Polish (Phase 6)**: depends on all stories

### Key task dependencies

- T008 → T009 → T010 → T011 (projects before references and packages)
- T012 → T013 to T017 (package.json and `npm install` first)
- T018, T019 → T021; T019, T020 → T022; T021 → T023 → T024; T025 → T026 → T027; T028 → T029
- T021, T023, T027, T029, T030 → T031
- T031, T033 → T034 → T041 to T043, T044
- T036 → T037; T035, T049 → T050 → T052 → T053 → T054
- T055, T056, T058 → T057 → T059
- T046 makes T039 pass; T047 makes T038 pass; T048 makes T041 to T043 and T044 pass; T049 to T054 make T045 pass

### Within Each User Story

- Tests are written first and must fail before implementation
- Backend: infrastructure registration → API writer/endpoints
- Frontend: schemas → API layer → components → routes
- Container files → compose → end-to-end validation

## Parallel Example: Phase 1

```text
After T012: T013 tsconfig | T014 eslint | T015 vite.config | T016 entry files | T017 folders
Independent of backend: T002 global.json | T003 Build.props | T004 Packages.props | T005 tools | T006 editorconfig | T007 gitignore
```

## Parallel Example: User Story 1

```text
Tests together:  T038 HealthResponseWriterTests | T039 DatabaseHealthCheckTests | T040 DbContextTests
                 T041 HealthEndpointTests | T042 MigrationTests | T043 DatabaseUnavailableTests
                 T044 StartupAvailabilityTests | T045 HomePage.test.tsx
Then in parallel: backend T046→T047→T048  ||  frontend T049, T051 → T050 → T052 → T053 → T054
                  ||  containers T055 | T056 | T058 → T057
Finally:         T059 end-to-end validation
```

## Parallel Example: User Story 2

```text
T060 Domain arch test | T061 Application arch test | T062 Infrastructure arch test
then T063 → T064 → T065
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup
2. Phase 2: Foundational
3. Phase 3: US1. **Stop and validate** with T059: one command brings up a healthy system.

### Incremental Delivery

1. Setup + Foundational: a compiling skeleton
2. US1: runnable system with health check and its tests (MVP)
3. US2: complete per-project sample tests, documented test commands, CI
4. US3: README that lets a newcomer do all of the above unaided
5. Polish: security, constitution, and full quickstart validation

---

## Notes

- [P] = different files and no dependency on an incomplete task
- Commit after each task or logical group. CI must pass before merge (constitution).
- Docker in this Claude Code session: until the session is restarted, prefix PowerShell commands with `$env:Path = "$env:LOCALAPPDATA\Programs\DockerDesktop\resources\bin;$env:Path";` so `docker` and its credential helper resolve
- No product features (connections, dashboards, queries, auth) may be added (FR-017)
