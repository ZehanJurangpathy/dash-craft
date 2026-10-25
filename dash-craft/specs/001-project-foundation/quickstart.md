# Quickstart & Validation: Project Foundation

**Feature**: `001-project-foundation`

Use this guide to prove the feature works end-to-end. Each scenario maps to a user story or
requirement in [spec.md](spec.md).

## Prerequisites

| Tool | Version | Needed for |
|------|---------|------------|
| Docker Desktop (or another engine with Compose v2+) | current (verified with 29.9.2, Compose v5.6.1) | Running the system, integration tests |
| .NET SDK | 10.0.401+ (see `global.json`) | Backend build and tests |
| Node.js | 24 LTS (>= 24.15) | Frontend build and tests |
| Git | any | Cloning |

Default ports: web `8080`, API `5080`, SQL Server `14330`. Override them in `.env` (copy
`.env.example`).

## Scenario 1: Start the system with one command (US1, FR-001 to FR-007)

```bash
git clone <repo-url> && cd <repo>/dash-craft
docker compose up -d --build
```

Expected:

1. `docker compose ps` shows `sqlserver` (healthy), `api`, and `web` running.
2. `curl -i http://localhost:5080/health` returns `200` with `"status":"Healthy"` and a
   `database` check that is `Healthy` (shape: [contracts/health.openapi.yaml](contracts/health.openapi.yaml)).
   Within about 2 minutes of start (SC-002).
3. Opening <http://localhost:8080> shows the DashCraft placeholder page with "API: Healthy".
4. `curl -i http://localhost:8080/health` returns the same report through the web proxy.
5. The `DashCraft` database contains `__EFMigrationsHistory` with the `InitialCreate` row:
   `docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d DashCraft -Q "SELECT MigrationId FROM __EFMigrationsHistory"`

## Scenario 2: Database outage is reported (FR-005, edge cases)

```bash
docker compose stop sqlserver
curl -i http://localhost:5080/health      # expect 503, database: Unhealthy, no exception text
# the web page now shows that the API reports a problem
docker compose start sqlserver
curl -i http://localhost:5080/health      # returns to 200 Healthy without restarting the API
```

## Scenario 3: Stop, restart, data persists (FR-002, FR-007, SC-005)

```bash
docker compose down        # stops everything, keeps the data volume
docker compose up -d
curl -i http://localhost:5080/health      # 200 Healthy; migrations not re-applied
```

Repeat 3 times. `docker compose down -v` is the documented way to wipe local data.

## Scenario 4: Run the tests (US2, FR-010 to FR-013)

```bash
# Backend unit tests (no Docker needed)
dotnet test --solution src/api/DashCraft.Unit.slnf

# Backend integration tests (Docker required; starts and removes a SQL Server container)
dotnet test --project tests/api/DashCraft.IntegrationTests

# Frontend
cd src/web && npm ci && npm run lint && npm run typecheck && npm test
```

Expected: every command exits `0`, and each backend test project and the frontend report at
least one passing test. Then break one assertion on purpose and confirm the command exits
non-zero (FR-013); revert it afterwards.

## Scenario 5: README walkthrough (US3, FR-014, SC-001, SC-006)

A developer unfamiliar with the project follows only `README.md` from a fresh clone. They should
start the system, open the web app and health check, run all three test suites, and find where
backend, frontend, and test code live, all without outside help.

## Scenario 6: CI

Push a branch. The `CI` workflow runs the backend (build, unit, integration), frontend (lint,
type-check, test, build), and docker (compose build) jobs; all pass.
