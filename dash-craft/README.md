# DashCraft

DashCraft is an open-source, self-hostable dashboard builder for SQL Server. You connect a
database, pick tables, joins and columns visually, and build interactive dashboards without
writing SQL. DashCraft only ever reads your data (see the
[constitution](.specify/memory/constitution.md)).

> **Status: early-stage.** This is the project foundation: a running but empty web app, an API
> with a health check, and an application database. Connections, queries, dashboards and sign-in
> come in later features.

## Prerequisites

| Tool | Version | Needed for |
|------|---------|------------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) or another engine with Compose v2+ | current | Running the system; integration tests |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0.401 or later 10.0 feature band (pinned in `global.json`) | Backend build and tests |
| [Node.js](https://nodejs.org/) | 24 LTS (24.15 or later) | Frontend build and tests |
| Git | any | Cloning |

## Quick start

```bash
git clone <repo-url>
cd <repo>/dash-craft
cp .env.example .env          # optional: only needed to change the defaults
docker compose up -d --build
```

When the containers are up (usually under 2 minutes):

| What | Address |
|------|---------|
| Web app | <http://localhost:8080> |
| API health check | <http://localhost:5080/health> (also via the web app at <http://localhost:8080/health>) |
| OpenAPI document | `/openapi/v1.json` on the API, in the Development environment only |

The home page shows **API: Healthy** once the API can reach its database. `/health` returns
`200` when everything is healthy and `503` while the database is unreachable or still being set
up. The database schema is created and migrated automatically on startup.

## Stopping and resetting

```bash
docker compose down        # stop everything; the database volume is kept
docker compose up -d       # start again; your data is still there
docker compose down -v     # stop and DELETE all local data (the dashcraft-sqldata volume)
```

## Configuration

Compose reads `.env` (copy it from `.env.example`). Every value has a working default.

| Variable | Default | Meaning |
|----------|---------|---------|
| `MSSQL_SA_PASSWORD` | `DashCraft_Dev_Passw0rd!` | SQL Server `sa` password. 8+ characters with upper, lower, digit and symbol. |
| `SQL_PORT` | `14330` | Host port for SQL Server |
| `API_PORT` | `5080` | Host port for the API |
| `WEB_PORT` | `8080` | Host port for the web app |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` to expose the OpenAPI document |

> **Warning:** the default password is for local development only. Change `MSSQL_SA_PASSWORD`
> before running DashCraft anywhere shared or reachable from a network. Never commit `.env`
> (it is gitignored).

The API itself is configured with standard ASP.NET Core settings, for example
`ConnectionStrings__DashCraft` and `Database__ApplyMigrationsOnStartup` as environment variables.

## Running locally without containers

Useful for debugging with hot reload:

```bash
docker compose up -d sqlserver                 # just the database, on localhost:14330
dotnet run --project src/api/DashCraft.Api     # API on http://localhost:5080 (Development)
cd src/web && npm ci && npm run dev            # web app on http://localhost:5173
```

The Vite dev server forwards `/health` and `/api` to the API on port 5080 (override with
`VITE_API_PROXY_TARGET`). In Development, the API uses the connection string in
`src/api/DashCraft.Api/appsettings.Development.json`, which matches the compose defaults.

## Running tests

| Suite | Command (from `dash-craft/`) | Needs Docker? |
|-------|------------------------------|---------------|
| Backend unit tests (4 projects) | `dotnet test --solution src/api/DashCraft.Unit.slnf` | No |
| Backend integration tests | `dotnet test --project tests/api/DashCraft.IntegrationTests` | Yes |
| Frontend | `cd src/web && npm ci && npm run lint && npm run typecheck && npm test` | No |

- Integration tests start a disposable SQL Server container with Testcontainers and remove it
  afterwards. If Docker isn't running, the SQL Server tests fail quickly with "Integration tests
  need a running Docker engine".
- Every command exits non-zero if any test fails.
- Backend tests use xUnit v3 with Shouldly on Microsoft.Testing.Platform (enabled in
  `global.json`).
- DashCraft itself sends no telemetry. The .NET tooling does by default; to opt out, set
  `DOTNET_CLI_TELEMETRY_OPTOUT=1` and `TESTINGPLATFORM_TELEMETRY_OPTOUT=1` in your environment
  (CI already does).

## Database migrations

The application database is managed with EF Core Code First migrations in
`src/api/DashCraft.Infrastructure/Persistence/Migrations`.

```bash
dotnet tool restore
dotnet ef migrations add <Name> \
  --project src/api/DashCraft.Infrastructure \
  --startup-project src/api/DashCraft.Api \
  --output-dir Persistence/Migrations
```

Migrations are applied automatically on startup when `Database__ApplyMigrationsOnStartup=true`
(the compose and Development default).

## Repository layout

```text
dash-craft/
├── docker-compose.yml          # sqlserver + api + web
├── .env.example                # compose settings (copy to .env)
├── global.json                 # .NET SDK pin and test runner
├── Directory.Build.props       # shared .NET build settings (warnings are errors)
├── Directory.Packages.props    # every NuGet version, pinned centrally
├── src/
│   ├── api/                    # .NET 10 backend (Clean Architecture)
│   │   ├── DashCraft.slnx      # full solution; DashCraft.Unit.slnf = unit tests only
│   │   ├── DashCraft.Domain/          # entities and rules; no dependencies
│   │   ├── DashCraft.Application/     # features, handlers, abstractions, Mediator behaviors
│   │   ├── DashCraft.Infrastructure/  # EF Core, migrations, SQL Server, health check
│   │   └── DashCraft.Api/             # Minimal API endpoints, DI, error handling
│   └── web/                    # React + TypeScript (Vite)
│       └── src/
│           ├── app/            # root component and providers
│           ├── routes/         # central route table and thin route components
│           ├── features/       # one folder per feature (components, api, schemas)
│           ├── components/     # shared UI primitives
│           └── lib/            # shared utilities (API client)
├── tests/
│   └── api/                    # backend unit test projects + DashCraft.IntegrationTests
└── specs/                      # Spec Kit feature specs, plans and tasks
```

## Troubleshooting

- **"port is already allocated"**: another program uses 8080, 5080 or 14330. Set `WEB_PORT`,
  `API_PORT` or `SQL_PORT` in `.env` and run `docker compose up -d` again.
- **Docker isn't running**: start Docker Desktop and wait until it reports that the engine is
  running. On Windows, if `docker` isn't found in an already-open terminal after installing,
  open a new terminal.
- **SQL Server container exits immediately**: the `MSSQL_SA_PASSWORD` doesn't meet SQL Server's
  complexity rules. Check `docker compose logs sqlserver`.
- **`/health` returns 503**: the API can't reach its database, or migrations are still running
  just after startup. Check `docker compose ps` and `docker compose logs api`.

## Contributing and CI

The `CI` GitHub Actions workflow (`.github/workflows/ci.yml` at the repository root) runs on
every push to `master` and every pull request:

- **backend**: build, unit tests, integration tests;
- **frontend**: lint, type-check, tests, build;
- **docker**: builds both images.

The constitution requires CI to pass before merge. GitHub can't enforce that from repository
files, so a maintainer must do this once: **Settings → Branches → Add branch protection rule**
for `master`, enable **Require status checks to pass before merging**, and select `backend`,
`frontend` and `docker`.

## License

DashCraft is released under the [MIT License](LICENSE). Third-party package licenses were
verified in [`specs/001-project-foundation/research.md`](specs/001-project-foundation/research.md).
The SQL Server container image is not part of DashCraft: it runs under Microsoft's license terms,
and the Developer edition used by `docker-compose.yml` is for development and testing only.
