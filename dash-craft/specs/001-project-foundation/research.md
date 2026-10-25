# Research: Project Foundation

**Feature**: `001-project-foundation` | **Date**: 2026-10-25

All versions below were checked against the live NuGet, npm, MCR, Docker Hub, and GitHub
registries on 2026-10-25. "Latest stable" means the newest version without a prerelease suffix.

## 1. Package versions, licenses, and .NET 10 compatibility

### 1.1 Backend (NuGet)

| Package | Pinned version | License | .NET 10 compatibility | Constitution |
|---------|----------------|---------|-----------------------|--------------|
| Mediator.Abstractions | 3.0.2 | MIT (license file checked) | net8.0 / netstandard2.0 assets, runs on net10.0 | ✅ |
| Mediator.SourceGenerator | 3.0.2 | MIT (license file checked) | Roslyn analyzer (netstandard2.0) | ✅ |
| Riok.Mapperly | 4.3.1 | Apache-2.0 | Source generator (netstandard2.0) | ✅ |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.13 | MIT | net10.0 | ✅ |
| Microsoft.EntityFrameworkCore.Design | 10.0.13 | MIT | net10.0 (design-time only) | ✅ |
| dotnet-ef (local tool) | 10.0.13 | MIT | net10.0 | ✅ |
| Microsoft.Extensions.Diagnostics.HealthChecks | 10.0.13 | MIT | net10.0 | ✅ |
| Dapper | 2.1.89 | Apache-2.0 | net10.0 asset | ✅ |
| Microsoft.Data.SqlClient | **6.1.7** (latest is 7.1.1, see 1.3) | MIT | net8.0/net9.0 assets, runs on net10.0 | ✅ |
| Microsoft.SqlServer.TransactSql.ScriptDom | 180.140.0 | MIT | net8.0 / netstandard2.1 assets | ✅ |
| FluentValidation | 12.2.0 | Apache-2.0 | net8.0 asset | ✅ |
| FluentValidation.DependencyInjectionExtensions | 12.2.0 | Apache-2.0 | net8.0 asset | ✅ |
| Microsoft.AspNetCore.OpenApi | 10.0.13 | MIT | net10.0 | ✅ |
| xunit.v3 | 4.0.2 | Apache-2.0 | net8.0 asset | ✅ |
| xunit.runner.visualstudio | 4.0.1 | Apache-2.0 | test adapter | ✅ |
| Microsoft.NET.Test.Sdk | 18.11.0 | MIT | test host | ✅ |
| Shouldly | 4.3.0 | BSD-3-Clause | net9.0 asset | ✅ |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.13 | MIT | net10.0 | ✅ |
| Testcontainers.MsSql | 4.16.0 | MIT | net10.0 asset | ✅ |

Local SDK found: .NET SDK **10.0.401**.

### 1.2 Frontend (npm)

| Package | Pinned version | License | Notes | Constitution |
|---------|----------------|---------|-------|--------------|
| react / react-dom | 19.3.1 | MIT | | ✅ |
| vite | 8.3.5 | MIT | Node ^20.19 or >=22.12 | ✅ |
| @vitejs/plugin-react | 6.1.2 | MIT | peer vite ^8 | ✅ |
| typescript | **6.0.3** (latest is 7.0.2, see 1.3) | Apache-2.0 | | ✅ |
| react-router | 8.5.0 | MIT | Node >=22.22 | ✅ |
| @tanstack/react-query | 5.104.5 | MIT | | ✅ |
| @tanstack/react-table | 9.3.1 | MIT | Installed, first used by a later feature | ✅ |
| tailwindcss / @tailwindcss/vite | 4.3.3 | MIT | peer vite ^8 supported | ✅ |
| react-hook-form | 7.91.0 | MIT | Installed, first used by a later feature | ✅ |
| zod | 4.6.7 | MIT | | ✅ |
| @hookform/resolvers | 6.0.0 | MIT | peer zod ^4, react-hook-form ^7.55 | ✅ |
| vitest | 5.0.5 | MIT | Node ^22.12 or ^24 | ✅ |
| jsdom | 30.1.3 | MIT | Node ^22.22.2 or ^24.15 | ✅ |
| @testing-library/react | 16.3.3 | MIT | | ✅ |
| @testing-library/dom | 10.4.2 | MIT | peer of RTL | ✅ |
| @testing-library/jest-dom | 7.0.1 | MIT | | ✅ |
| @testing-library/user-event | 14.6.8 | MIT | | ✅ |
| @types/react / @types/react-dom | 19.3.0 | MIT | | ✅ |
| @types/node | 24.19.2 (latest is 26.6.5; pinned to match the Node 24 runtime) | MIT | | ✅ |
| eslint / @eslint/js | 10.13.0 / 10.0.1 | MIT | see 6 | ✅ |
| typescript-eslint | 8.72.0 | MIT | peer typescript <6.1.0 | ✅ |
| eslint-plugin-react-hooks | 7.1.1 | MIT | | ✅ |
| eslint-plugin-react-refresh | 0.5.7 | MIT | | ✅ |
| globals | 17.13.0 | MIT | | ✅ |

### 1.3 Pinned below "latest", and why

- **Microsoft.Data.SqlClient 6.1.7 rather than 7.1.1**: EF Core SqlServer 10.0.13 depends on
  SqlClient 6.1.7. Referencing 7.x directly would force a major-version upgrade underneath EF
  Core that the EF team has not shipped against, and Dapper and EF Core would then share a driver
  version EF was not tested with. We pin 6.1.7 so EF Core and Dapper share one tested driver.
  Revisit when an EF Core 10.x patch moves to SqlClient 7.
- **TypeScript 6.0.3 rather than 7.0.2**: `typescript-eslint` 8.72.0 declares a peer dependency
  of `typescript >=4.8.4 <6.1.0`. TypeScript 7 (the native compiler) would break linting, which we
  rely on to enforce the constitution's "no `any`" rule. Revisit when typescript-eslint supports 7.

### 1.4 Licensing verdict

**No package violates the constitution's licensing rule.** DashCraft is licensed under **MIT**
(chosen 2026-10-25). Every dependency is MIT, Apache-2.0, or BSD-3-Clause, all of which are
permissive and compatible with distributing DashCraft under MIT. Specific checks:

- Mediator ships its license as a file rather than an SPDX expression; the file in the 3.0.2
  package was inspected and is the MIT License (Copyright 2022 Martin Othamar).
- MediatR and AutoMapper (commercially licensed in recent versions) are **not** used.
- FluentValidation 12 remains Apache-2.0.
- ScriptDom is MIT, as the constitution expected.

## 2. Packages considered and NOT added

| Package | Reason not added |
|---------|------------------|
| Serilog.AspNetCore 10.0.0 (Apache-2.0) | Built-in `ILogger` with the JSON console formatter gives structured logs with no new dependency (constitution: "new dependencies need a clear justification"). |
| AspNetCore.HealthChecks.SqlServer 9.0.0 (Apache-2.0) | Last release targets .NET 8/9 only; a small custom check over the existing DbContext covers the need. |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 10.0.13 (MIT) | Its `AddDbContextCheck` can't report "not ready" while background migrations are still running (§7). The custom `DatabaseHealthCheck` needs only the base health checks package. |
| Scalar.AspNetCore 2.18.3 (MIT) | An API docs UI isn't needed for one endpoint; the OpenAPI document alone meets the requirement. Easy to add later. |
| Microsoft.EntityFrameworkCore.Tools | Only needed for Visual Studio's Package Manager Console; the `dotnet-ef` local tool covers the CLI. |
| coverlet.collector 10.1.0 (MIT) | Coverage reporting wasn't requested. |
| msw 3.0.3 (MIT) | One component test can stub `fetch` directly; add MSW when API mocking grows. |
| xunit (v2) 2.9.3 | Superseded by xunit.v3. |

## 3. Solution layout and build configuration

- **Decision**: `src/api/DashCraft.slnx` (the .NET 10 default XML solution format) includes the
  four layer projects and all test projects under `tests/api`. A repo-root `global.json` pins
  SDK `10.0.401` (`rollForward: latestFeature`). Repo-root `Directory.Build.props` sets
  `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`.
  `Directory.Packages.props` uses Central Package Management so every version is pinned in one
  place.
- **Mapperly "unmapped members fail the build"**: `.editorconfig` raises Mapperly diagnostics
  RMG012 (a target member has no matching source member) and RMG020 (a source member is not
  mapped to any target member) to `error`.
- **Rationale**: Central pinning makes the license review auditable and keeps versions in sync.
- **Alternatives**: Classic `.sln` works but is noisier to merge; per-project versions scatter
  pins.

## 4. Where each package is referenced

- **Domain**: no packages (constitution: Domain has no dependencies).
- **Application**: Mediator.Abstractions, FluentValidation (+ DI extensions), Riok.Mapperly.
- **Infrastructure**: EF Core SqlServer, Microsoft.Extensions.Diagnostics.HealthChecks, Dapper,
  Microsoft.Data.SqlClient, ScriptDom. The query builder and SELECT-only validator will be added
  as a self-contained `QuerySafety` area in Infrastructure, which has no web-layer dependency,
  satisfying both Principle V and the "dedicated, independently testable module" rule. Whether
  it becomes its own project is decided by the first query feature, not here.
- **Api**: Mediator.SourceGenerator (the generator must run in the composing host project),
  Microsoft.AspNetCore.OpenApi, EF Core Design (`PrivateAssets=all`, for migrations tooling).
- **Tests**: xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Shouldly; the
  integration project adds Microsoft.AspNetCore.Mvc.Testing and Testcontainers.MsSql.

## 5. Health check design

- **Decision**: ASP.NET Core health checks with a custom `DatabaseHealthCheck` registered as
  `"database"`, exposed at `GET /health` with a custom JSON writer (see
  `contracts/health.openapi.yaml`). The check reports Unhealthy until `DatabaseReadiness` says
  migrations have finished, then uses `Database.CanConnectAsync`. Returns 200 when Healthy, 503
  when Unhealthy. The response names the failing dependency but never includes exception details
  or connection strings.
- **Authorization**: anonymous under constitution v1.1.0 Principle II's Health endpoint exception.
  The no-leak tests the exception requires are `HealthResponseWriterTests` and
  `DatabaseUnavailableTests`.
- **Rationale**: The framework's health check pipeline returns the right status codes for Docker
  and load balancers. Health is infrastructure, not a business query, so it is not routed through
  Mediator.
- **Alternatives**: A hand-written Minimal API endpoint (duplicates framework behavior);
  liveness/readiness split (unnecessary until there is an orchestrator that uses it).

## 6. Linting

- **Decision**: ESLint 10 flat config with `typescript-eslint` strict type-checked rules,
  `@typescript-eslint/no-explicit-any: error`, and the React hooks / refresh plugins.
- **Rationale**: The constitution forbids unjustified `any`. Strict TypeScript alone doesn't
  catch explicit `any`; a lint rule makes it a CI failure, and a justified exception requires an
  `eslint-disable` comment, which is the "justified in a comment" mechanism.

## 7. Database startup and migrations

- **Decision**: A `MigrationHostedService` (BackgroundService) applies EF Core migrations when
  `Database__ApplyMigrationsOnStartup=true` (set in docker-compose and Development, default false
  otherwise). It runs in the background, so Kestrel starts immediately and `/health` answers 503
  until migrations finish; that is the spec's edge case "health check reports unhealthy until the
  database is ready".
  - It retries up to `Database:StartupMaxRetries` times with `Database:StartupRetryDelay`.
  - If retries run out, it logs at Critical and stops the host, so the container restart policy
    takes over.
  - `Migrate()` creates the database if it is missing and is idempotent.
  - `EnableRetryOnFailure` handles transient failures.
  - Compose still waits for SQL Server's health check (`sqlcmd SELECT 1`) before starting the API.
- **Initial migration**: `InitialCreate`, generated from an empty `DashCraftDbContext`. It only
  produces the `__EFMigrationsHistory` table, which is the "empty baseline schema with version
  tracking" from the spec.
- **Rationale**: Meets FR-006 (automatic, repeatable) and the edge case "database not yet ready".
- **Alternatives**:
  - Migrating synchronously before `RunAsync`: simpler, but the API isn't listening while it
    waits, so `/health` gives connection-refused instead of 503. Rejected after
    `/speckit-analyze` finding I1.
  - A separate migration container or SQL bundle: more moving parts for an empty schema.
  - `EnsureCreated`: bypasses migrations. Rejected.

## 8. Container images and runtime

| Image | Tag | Use |
|-------|-----|-----|
| mcr.microsoft.com/mssql/server | `2025-CU10-ubuntu-24.04` | Application database (compose) and Testcontainers |
| mcr.microsoft.com/dotnet/sdk | `10.0` | API build stage |
| mcr.microsoft.com/dotnet/aspnet | `10.0` | API runtime (non-root `app` user) |
| node | `24.22.1-alpine` | Web build stage; Node 24 LTS satisfies react-router (>=22.22) and vitest/jsdom |
| nginx | `1.30.6-alpine` (stable line) | Serves the built SPA and reverse-proxies `/health` and `/api` to the API |

- **SQL Server licensing note**: the SQL Server image is not bundled with DashCraft; it runs
  under Microsoft's EULA with `MSSQL_PID=Developer`, which is free for development and testing.
  Self-hosters bring their own licensed SQL Server for production. This doesn't affect
  DashCraft's own license, but the README must say so.
- **Same-origin proxy**: nginx (container) and the Vite dev server (local) both proxy `/health`
  and `/api` to the API, so the browser never needs CORS and the API URL is not baked into the
  bundle.
- **Secrets**: `MSSQL_SA_PASSWORD` comes from a gitignored `.env` file. Compose uses a clearly
  labeled development-only default so `docker compose up` works on a fresh clone (FR-001). The
  README warns to change it outside local development.

## 9. Tests

- **Backend unit tests**: one test project per layer (`DashCraft.{Domain,Application,Infrastructure,Api}.Tests`),
  each with at least one meaningful sample test (for example, the health response writer maps
  statuses correctly, or the DbContext model builds).
- **Integration tests**: `DashCraft.IntegrationTests` uses `WebApplicationFactory<Program>` plus a
  shared Testcontainers SQL Server fixture (pinned image above). Tests cover:
  - `/health` returns 200 Healthy after migrations;
  - migrations are recorded in `__EFMigrationsHistory`;
  - `/health` returns 503 with `database: Unhealthy` when the database is unreachable (FR-005).
  If Docker is unavailable, Testcontainers fails fast with a clear error.
- **Frontend**: Vitest + jsdom + React Testing Library. A sample test renders the home page with
  `fetch` stubbed and asserts both healthy and unreachable states.
- **Runner**: classic VSTest via `dotnet test` (Microsoft.NET.Test.Sdk + xunit.runner.visualstudio)
  for the broadest IDE and CI support. Unit and integration tests are split with
  `dotnet test --filter` by project, so unit tests run without Docker.

## 10. Continuous integration

- **Decision**: GitHub Actions workflow `.github/workflows/ci.yml` on push and pull request with
  three jobs:
  - **backend**: `actions/setup-dotnet@v6` (from `global.json`), restore, build with warnings as
    errors, unit tests, integration tests (GitHub `ubuntu-latest` runners include Docker).
  - **frontend**: `actions/setup-node@v7` (Node 24, npm cache), `npm ci`, lint, type-check,
    test, build.
  - **docker**: `docker compose build` to prove both images build.
- Uses `actions/checkout@v7`. No secrets are required.

## 11. Environment note

Docker Desktop is installed on the development machine (Docker 29.9.2, Compose v5.6.1). The
start command and integration tests need the Docker engine running; unit and frontend tests do
not. Compose files use only the Compose Specification, so any Compose v2+ engine works.
