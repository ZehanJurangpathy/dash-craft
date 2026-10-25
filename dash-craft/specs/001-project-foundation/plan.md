# Implementation Plan: Project Foundation

**Branch**: `001-project-foundation` | **Date**: 2026-10-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-project-foundation/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Create a runnable, empty DashCraft skeleton:

- **Backend**: a .NET 10 Clean Architecture backend (Api, Application, Domain, Infrastructure).
  It exposes `GET /health`, which reports the status of an EF Core-managed SQL Server application
  database initialized by an empty `InitialCreate` migration.
- **Frontend**: a Vite + React + TypeScript web app whose home route calls the health endpoint
  through a feature-scoped API layer.
- **Repository root**: docker-compose that starts SQL Server, the API, and the web app with one
  command.

Every backend project gets a unit test project. An integration test project runs against a real
SQL Server via Testcontainers, and the web app has Vitest component tests. A GitHub Actions
workflow runs all of it. Every package version and license was verified on 2026-10-25
([research.md](research.md)); none violates the licensing rule.

## Technical Context

**Language/Version**: C# 14 on .NET 10 (SDK 10.0.401, pinned in `global.json`); TypeScript 6.0.3
(strict) on Node.js 24 LTS

**Primary Dependencies**:
- **Backend**:
  - ASP.NET Core Minimal API 10.0.13, Microsoft.AspNetCore.OpenApi 10.0.13
  - Mediator 3.0.2 (abstractions and source generator)
  - Riok.Mapperly 4.3.1, FluentValidation 12.2.0
  - EF Core SqlServer 10.0.13, Microsoft.Extensions.Diagnostics.HealthChecks 10.0.13
  - Dapper 2.1.89, Microsoft.Data.SqlClient 6.1.7
  - Microsoft.SqlServer.TransactSql.ScriptDom 180.140.0
- **Frontend**:
  - React 19.3.1, Vite 8.3.5, React Router 8.5.0
  - TanStack Query 5.104.5, TanStack Table 9.3.1
  - Tailwind CSS 4.3.3
  - React Hook Form 7.91.0, Zod 4.6.7, @hookform/resolvers 6.0.0

**Storage**: SQL Server 2025 (`mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-24.04`) for the
DashCraft application database via EF Core Code First; persisted in a named Docker volume

**Testing**:
- **Backend**: xunit.v3 4.0.2 + Shouldly 4.3.0; Microsoft.AspNetCore.Mvc.Testing 10.0.13 and
  Testcontainers.MsSql 4.16.0 for integration tests
- **Frontend**: Vitest 5.0.5 + React Testing Library 16.3.3 + jsdom 30.1.3

**Target Platform**: Linux containers (Docker Compose v2+); developer machines on Windows, macOS,
Linux; CI on GitHub Actions `ubuntu-latest`

**Project Type**: Web application (Minimal API backend + SPA frontend) in a monorepo

**Performance Goals**: System healthy within 2 minutes of `docker compose up` (SC-002); placeholder
page loads in under 3 seconds once healthy (SC-004)

**Constraints**:
- No cloud services or telemetry.
- No secrets committed; configuration via environment variables and `.env`.
- Warnings are treated as errors; Mapperly unmapped members are errors.
- No product features: no connections, dashboards, queries, or authentication.
- Project license: MIT (`LICENSE` at the repo root).

**Scale/Scope**: 4 backend projects, 5 test projects, 1 SPA with 1 route, 1 endpoint, 3 containers,
1 CI workflow

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Gate (from constitution v1.1.0) | Status | Evidence |
|---|---------------------------------|--------|----------|
| I | Read-only data access: only validated SELECTs reach user databases; Dapper only for that path; EF Core never against user DBs | ✅ N/A-safe | No query execution in this feature. Dapper/ScriptDom are referenced but unused. EF Core only targets the application DB ([data-model.md](data-model.md)). |
| II | Server-side authorization on every endpoint, except a health endpoint meeting the Health endpoint exception | ✅ | `/health` is the only anonymous endpoint and meets every condition of the exception: it returns only status names, dependency names, timings and fixed messages; it is read-only; it is marked anonymous in code with a reference to the rule (T048); and no-leak tests exist (T038, T043). No other endpoints exist in this feature. Every future endpoint group MUST require authorization. |
| III | Visual-first UX; non-technical error messages | ✅ | The placeholder page shows "API not reachable" style messages, never raw errors. |
| IV | Self-hostable via Docker/compose; no telemetry; secrets out of repo/logs; licenses verified against the project license | ✅ | One-command compose. `.env` is gitignored. Health output has no secrets. DashCraft is MIT-licensed, and every package is MIT/Apache-2.0/BSD-3-Clause, all compatible ([research.md §1.4](research.md)). No MediatR or AutoMapper. Telemetry checks in T070. |
| V | SQL Server only; SQL Server logic in Infrastructure behind Application interfaces | ✅ | DbContext, migrations, and future Dapper/ScriptDom code live in Infrastructure. No multi-database abstraction. |
| VI | Unit tests for every project + integration tests vs real SQL Server; xUnit + Shouldly; frontend component tests; tests before or alongside code; CI must pass before merge | ✅ | Four unit test projects, one integration project with Testcontainers, and Vitest/RTL tests. Each test task sits next to (and before) the code it covers, including Phase 2. CI gates all suites, and the README documents the branch protection setting that enforces it. |
| Tech | Minimal API + endpoint groups; Mediator; Mapperly; EF Core migrations; FluentValidation + ProblemDetails; structured logging; async + cancellation | ✅ | See Project Structure. Mediator pipeline has a validation behavior. ProblemDetails for all errors. JSON console logging. |
| Front | Strict TS, no `any`, React Router, TanStack Query, Tailwind, RHF + Zod, feature folders, API hooks in `features/*/api` | ✅ | ESLint `no-explicit-any: error`; health API hook in `features/health/api`. |
| Gov | New dependencies justified | ✅ | Each dependency was requested or is justified in research; rejected extras are listed in [research.md §2](research.md). |

**Result**: PASS (pre-research). **Post-design re-check**: PASS against constitution v1.1.0. The
anonymous `/health` endpoint is covered by Principle II's Health endpoint exception and is no
longer a violation.

## Project Structure

### Documentation (this feature)

```text
specs/001-project-foundation/
├── plan.md              # This file
├── research.md          # Phase 0: versions, licenses, design decisions
├── data-model.md        # Phase 1: application DB baseline + health shape
├── quickstart.md        # Phase 1: end-to-end validation guide
├── contracts/
│   └── health.openapi.yaml
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks; not created here)
```

### Source Code (repository root)

```text
.
├── .github/workflows/ci.yml          # backend, frontend, docker jobs
├── .config/dotnet-tools.json         # dotnet-ef 10.0.13 (local tool)
├── .editorconfig                     # C#/TS style; Mapperly RMG012/RMG020 = error
├── .gitignore  .gitattributes
├── .env.example                      # MSSQL_SA_PASSWORD, ports
├── docker-compose.yml                # sqlserver, api, web; volume dashcraft-sqldata
├── global.json                       # SDK 10.0.401
├── Directory.Build.props             # net10.0, nullable, warnings as errors
├── Directory.Packages.props          # central package versions (all pins)
├── .dockerignore                     # api build context (repo root)
├── LICENSE                           # MIT
├── README.md
├── src/
│   ├── api/
│   │   ├── DashCraft.slnx
│   │   ├── Dockerfile                # sdk:10.0 build -> aspnet:10.0 runtime (non-root)
│   │   ├── DashCraft.Domain/         # no package references; DomainAssembly marker
│   │   ├── DashCraft.Application/
│   │   │   ├── DependencyInjection.cs        # AddApplication(): Mediator, validators
│   │   │   └── Common/Behaviors/ValidationBehavior.cs
│   │   ├── DashCraft.Infrastructure/
│   │   │   ├── DependencyInjection.cs        # AddInfrastructure(): DbContext, health check, hosted service
│   │   │   ├── Health/DatabaseHealthCheck.cs  # readiness-aware "database" check
│   │   │   ├── Persistence/DashCraftDbContext.cs
│   │   │   ├── Persistence/DashCraftDbContextFactory.cs  # design-time factory for dotnet ef
│   │   │   ├── Persistence/DatabaseOptions.cs
│   │   │   ├── Persistence/DatabaseReadiness.cs
│   │   │   ├── Persistence/MigrationHostedService.cs  # background migrations with retry
│   │   │   └── Persistence/Migrations/<ts>_InitialCreate.cs
│   │   └── DashCraft.Api/
│   │       ├── Program.cs
│   │       ├── Endpoints/HealthEndpoints.cs     # MapHealthEndpoints() extension
│   │       ├── Health/HealthResponseWriter.cs   # JSON per contract
│   │       ├── ErrorHandling/                   # IExceptionHandler -> ProblemDetails
│   │       └── appsettings*.json
│   └── web/
│       ├── Dockerfile                # node:24 build -> nginx:1.30 runtime
│       ├── nginx.conf                # SPA fallback; proxy /health, /api -> api:8080
│       ├── .dockerignore
│       ├── package.json  package-lock.json
│       ├── vite.config.ts            # react + tailwind plugins; dev proxy; vitest config
│       ├── tsconfig*.json            # strict
│       ├── eslint.config.js
│       ├── index.html
│       ├── src/
│       │   ├── main.tsx
│       │   ├── vite-env.d.ts         # typed VITE_API_BASE_URL
│       │   ├── index.css             # @import "tailwindcss"
│       │   ├── app/                  # App.tsx, providers.tsx, queryClient.ts (test client)
│       │   ├── routes/               # router.tsx (central), HomeRoute.tsx, NotFoundRoute.tsx
│       │   ├── features/
│       │   │   ├── home/components/HomePage.tsx (+ HomePage.test.tsx)
│       │   │   └── health/
│       │   │       ├── api/getHealth.ts, api/useHealth.ts
│       │   │       ├── schemas/health.ts          # Zod + z.infer types
│       │   │       └── components/HealthStatus.tsx
│       │   ├── components/           # shared UI primitives (StatusBadge)
│       │   └── lib/apiClient.ts      # fetch wrapper, ProblemDetails parsing
│       └── tests/setup.ts            # jest-dom matchers
└── tests/
    └── api/
        ├── DashCraft.Domain.Tests/
        ├── DashCraft.Application.Tests/
        ├── DashCraft.Infrastructure.Tests/
        ├── DashCraft.Api.Tests/
        └── DashCraft.IntegrationTests/   # WebApplicationFactory + Testcontainers fixture
```

**Structure Decision**: This is the monorepo layout from the constitution: `/src/api` holds the
.NET solution, `/src/web` the SPA, `/tests/api` all backend tests, and Docker files sit at the
root. The API Dockerfile lives next to the solution, but its build context is the repo root
(compose sets `context: .`) so it can copy `global.json` and the `Directory.*.props` files. The
web image's build context is `src/web`. Project dependencies point inward:

- Api → Application, Infrastructure
- Infrastructure → Application → Domain

### Key design notes

- **Endpoints**: features register through `IEndpointRouteBuilder` extension methods. Future
  feature groups go under `/api/...`, and `/health` sits at the root.
  `Program.cs` only composes services and middleware.
- **Errors**:
  - `AddProblemDetails()`, `UseExceptionHandler()`, and `UseStatusCodePages()` turn all non-2xx
    responses into ProblemDetails.
  - A `ValidationException` (from the Mediator validation behavior) maps to 400 with `errors`.
  - Stack traces appear only in Development.
- **Logging**: built-in `ILogger`. The JSON console formatter is used in containers
  (`Logging__Console__FormatterName=json`), and the simple formatter locally. Connection strings
  are never logged.
- **OpenAPI**: `AddOpenApi()` + `MapOpenApi()` at `/openapi/v1.json` in Development.
- **Health**: a custom `DatabaseHealthCheck` registered as `"database"`, mapped to `/health` with
  `HealthResponseWriter`. It reports Unhealthy until `DatabaseReadiness` says migrations are done,
  then checks `CanConnectAsync`. Returns 200, or 503 when Unhealthy. It is anonymous under
  constitution Principle II's Health endpoint exception.
- **Startup**: when `Database:ApplyMigrationsOnStartup` is true, `MigrationHostedService` applies
  migrations in the background with retries, so Kestrel starts immediately and `/health` answers
  503 while SQL Server is still booting (spec edge case). If retries run out, it logs at Critical
  and stops the host so the container restart policy takes over.
- **Frontend data flow**:
  - `lib/apiClient` performs the fetch.
  - `features/health/api/getHealth` parses the response with a Zod schema.
  - `useHealth` wraps it in TanStack Query with `refetchInterval` of 10 s.
  - `HomePage` renders `HealthStatus`.
  - The API base URL is same-origin: nginx proxies in containers, and Vite's dev server proxies
    in local development.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Packages referenced before first use (Dapper, ScriptDom, Mapperly, TanStack Table, React Hook Form) | The user explicitly requested the full baseline so licenses and .NET 10 compatibility are verified once, up front | Adding them per feature is leaner, but deferring the license/compatibility review risks late surprises. Pins are centralized, so the cost is low. |
