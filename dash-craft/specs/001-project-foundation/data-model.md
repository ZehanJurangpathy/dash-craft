# Data Model: Project Foundation

**Feature**: `001-project-foundation` | **Date**: 2026-10-25

This feature introduces no business entities. It defines the application database baseline and
the health status shape returned by the API.

## Application database (`DashCraft`)

DashCraft's own SQL Server database, managed by EF Core Code First in Infrastructure.

| Item | Description |
|------|-------------|
| `DashCraftDbContext` | EF Core context in `DashCraft.Infrastructure/Persistence`. No `DbSet`s yet. Users, connections, dashboards, widgets, and saved queries arrive in later features. |
| Migration `InitialCreate` | Empty baseline migration. Applying it creates the database (if missing) and the `__EFMigrationsHistory` table. |
| `__EFMigrationsHistory` | EF Core's version tracking table (`MigrationId`, `ProductVersion`). This is the "schema version" the spec refers to. |
| Default schema | `dbo` (no custom schema until a feature needs one). |

**Rules**

- Schema changes MUST be made only through EF Core migrations in Infrastructure.
- Applying migrations MUST be idempotent: re-running on an up-to-date database changes nothing.
- Data MUST persist across container restarts (named Docker volume `dashcraft-sqldata`).
- EF Core MUST NOT be pointed at user-connected databases (constitution); this context only ever
  targets the application database.

## Health status (API response, not persisted)

Returned by `GET /health`. Full contract: [contracts/health.openapi.yaml](contracts/health.openapi.yaml).

| Field | Type | Description |
|-------|------|-------------|
| `status` | `"Healthy" \| "Degraded" \| "Unhealthy"` | Overall status; the worst of all checks. |
| `totalDurationMs` | number | Time taken to run all checks. |
| `checks` | array of `HealthCheckResult` | One entry per dependency. |

`HealthCheckResult`:

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Dependency name. Initially only `"database"`. |
| `status` | `"Healthy" \| "Degraded" \| "Unhealthy"` | Status of this dependency. |
| `description` | string, optional | Short, non-technical message (e.g., "The application database is not reachable."). MUST NOT contain exception text, connection strings, or host names. |
| `durationMs` | number | Time taken by this check. |

**State transitions**: `database` is `Unhealthy` until SQL Server accepts connections and
migrations are applied, then `Healthy`. It returns to `Unhealthy` if the database later becomes
unreachable. HTTP status follows the overall status: 200 for Healthy/Degraded, 503 for Unhealthy.

## Frontend representation

The web app validates the health response with a Zod schema (`features/health/schemas/health.ts`)
that mirrors the table above, and infers the TypeScript type with `z.infer`. A response that fails
validation, or a network failure, is shown as "API not reachable".
