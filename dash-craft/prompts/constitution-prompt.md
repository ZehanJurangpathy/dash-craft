/speckit.constitution

Create the constitution for DashCraft, an open-source, self-hostable dashboard
builder that lets users visually query SQL Server databases, join tables,
select columns, and build interactive dashboards without writing SQL.

## Core Principles

### I. Read-Only Data Access (NON-NEGOTIABLE)
- DashCraft MUST only ever generate and execute read-only SELECT queries
  against user-connected databases. No INSERT, UPDATE, DELETE, MERGE, DROP,
  ALTER, TRUNCATE, CREATE, EXEC, GRANT, or any other statement that mutates
  data or schema.
- Queries produced by the visual editor MUST be built from a structured query
  model (tables, joins, columns, filters, aggregations, sorting, limits)
  compiled to T-SQL by the dedicated query builder.
- Raw SQL is accepted ONLY from users with the Admin role (see Principle II).
  For every other user, raw SQL from the client MUST be rejected.
- Every query, whether generated or Admin-written, MUST pass through the
  SELECT-only validator before execution. The validator MUST:
  - parse the SQL with Microsoft's ScriptDom
    (Microsoft.SqlServer.TransactSql.ScriptDom) and validate the parsed AST,
    never with regex or string matching;
  - reject any input with parse errors;
  - allow exactly one statement, and only a SELECT statement. Use an
    allowlist of permitted AST node types rather than a denylist of
    forbidden statements, so unknown constructs are rejected by default;
  - reject SELECT INTO, locking/update table hints, data-modifying
    constructs, stored procedure execution (EXEC), dynamic SQL, and
    external data access functions (e.g., OPENROWSET, OPENQUERY,
    OPENDATASOURCE);
  - reject known side-effecting functions and objects.
- All values MUST be parameterized (Dapper parameters). Identifiers produced by
  the visual editor MUST be validated against discovered schema metadata and
  safely quoted (e.g., QUOTENAME semantics).
- Defense in depth: DashCraft is NOT responsible for granting read-only
  access. The database administrator is. The connection string a user adds
  MUST use a SQL Server account that has read-only permissions. This is a
  documented, mandatory setup requirement, and the UI MUST state it when a
  connection is added. DashCraft's validator is an additional safeguard and
  never a substitute for read-only database permissions.
- DashCraft SHOULD check the effective permissions of the connecting account
  when a connection is saved and warn the user if write permissions are
  detected. Execution MUST enforce row limits and command timeouts.
- Any feature or change that could weaken these guarantees MUST be rejected.

### II. Role-Based Capabilities
- There are two roles: User and Admin.
- User: builds queries and dashboards through the visual editor only. Users
  can view the generated SQL read-only but CANNOT write, edit, or submit SQL.
- Admin: has all User abilities and may additionally write SELECT SQL
  statements in order to build complex queries for charts and tables that
  cannot easily be built in the visual editor. Admin SQL is subject to all
  rules in Principle I and cannot bypass the validator.
- Authorization MUST be enforced on the server for every endpoint and
  command. UI hiding is a convenience, never a security control.
- Admin-written SQL MUST be audit logged (who, when, which connection) without
  logging secrets or result data.

### III. Visual-First, No-SQL User Experience
- Every core capability MUST be achievable through the UI without writing SQL.
  Admin SQL is an escape hatch, not the primary workflow.
- Error messages MUST be understandable to non-technical users.

### IV. Self-Hostable and Open Source
- The project MUST run on a user's own infrastructure with simple setup
  (Docker and docker-compose are the supported baseline).
- No required cloud services or telemetry. Configuration is via environment
  variables and config files.
- Secrets, such as stored connection strings, MUST be encrypted at rest and
  never returned to the client or written to logs.
- Dependencies MUST have licenses compatible with an open-source
  distribution. Do not use libraries with copyleft or commercial-use
  restrictions incompatible with the project license (e.g., AutoMapper 15+,
  MediatR 13+). Verify the license of every dependency before adopting it
  (ScriptDom is expected to be MIT-licensed).

### V. SQL Server Only
- DashCraft targets SQL Server databases only. Other database engines are
  out of scope.
- All SQL Server-specific logic (schema discovery, T-SQL generation, ScriptDom
  validation, connection handling) lives in the Infrastructure layer behind
  interfaces defined in Application, so it can be tested in isolation.
- Do not build abstractions or features for hypothetical support of other
  databases. Adding another engine requires a constitutional amendment.

### VI. Test-First Quality (NON-NEGOTIABLE)
- Unit tests for every project and integration tests are mandatory. A feature
  is not complete without both.
- Backend tests use xUnit with Shouldly for assertions.
- The query builder and SELECT-only validator are critical code and MUST have
  thorough unit tests, including adversarial cases (SQL injection attempts,
  stacked statements, comments, SELECT INTO, data-modifying constructs,
  OPENROWSET and similar functions, malformed identifiers, T-SQL-specific tricks).
- Integration tests MUST cover API endpoints, authorization (User vs Admin),
  EF Core persistence, and Dapper query execution against a real SQL Server
  instance (e.g., via Testcontainers).
- Frontend forms and critical flows require component tests.
- Tests are written before or alongside implementation, and CI MUST pass
  before merge.

## Technology Standards

### Backend
- .NET 10 with ASP.NET Core Minimal API (endpoints grouped by feature using
  endpoint groups or extension methods; no MVC controllers).
- CQRS using the Mediator library (martinothamar, MIT, source-generated):
  commands for state changes to DashCraft's own data, queries for reads.
  Handlers contain the business logic; endpoints stay thin. Cross-cutting
  concerns (validation, logging, authorization, transactions) are implemented
  as pipeline behaviors.
- Mapperly (source-generated) for mapping between entities, DTOs, and
  request/response models. One mapper class per feature. Unmapped members MUST
  fail the build.
- EF Core Code First (with migrations) for all CRUD operations against the
  DashCraft application database (users, roles, connections, dashboards,
  widgets, saved queries).
- Dapper (with Microsoft.Data.SqlClient) for executing queries against
  user-specified SQL Server databases to read data for dashboard widgets.
  Dapper is used ONLY for this read path and only after validation. EF Core
  is never used against user-specified databases.
- Request validation at the API boundary (e.g., FluentValidation or pipeline
  behaviors) with consistent ProblemDetails error responses.
- Structured logging, async/await throughout, and cancellation tokens on all I/O.

### Frontend
- React with TypeScript in strict mode. No `any` unless justified in a comment.
- React Router for routing.
- TanStack Query for all server state (fetching, caching, mutations). Use
  TanStack Table for tabular data where appropriate.
- Tailwind CSS for styling; keep a consistent, accessible, responsive design.
- React Hook Form with Zod for every form. Zod schemas are the single source
  of truth, and TypeScript types are inferred with z.infer.
- Functional components and hooks only.

## Project Structure

### Repository layout
- Monorepo with two top-level apps: `/src/api` (.NET 10) and `/src/web`
  (React + TypeScript). Backend tests live in `/tests/api`; frontend tests are
  `*.test.tsx` files next to components or under `/src/web/tests`.
- Docker and docker-compose files live at the repository root.

### Backend structure
- Follows Clean Architecture.
- Layers: Api (endpoints, DI, middleware) -> Application (features, handlers,
  abstractions) -> Domain (entities, rules) <- Infrastructure (EF Core,
  Dapper, SQL Server schema discovery, query execution). Domain has no
  dependencies on other layers. Infrastructure implements abstractions
  defined in Application.
- The query builder and SELECT-only validator live in a dedicated,
  independently testable module with no dependency on the web layer.
- EF Core migrations and DbContext live in Infrastructure.
- Each project has a matching test project (unit tests), plus a separate
  integration test project.

### Frontend structure
- Organize by feature under `src/features/<feature>` (components, hooks, API
  calls, Zod schemas, types). Shared UI primitives go in `src/components`,
  shared utilities in `src/lib`.
- API calls and TanStack Query hooks live in each feature's `api` folder,
  never directly in components.
- Routes are defined centrally with React Router; route components are thin
  and compose feature components. Admin-only routes and UI are guarded by role.
- Zod schemas sit next to the forms that use them, and types are inferred
  from them.

## Security
- Authentication and authorization protect all endpoints. Users only access
  connections and dashboards they are permitted to see.
- Validate and sanitize all input on both client and server; the server is
  the source of truth.
- Follow OWASP guidance. SQL injection prevention and role enforcement are
  verified by automated tests.

## Governance
- This constitution supersedes other practices. Pull requests MUST be checked
  for compliance, especially Principles I, II, and VI.
- Amendments require documented rationale and a version bump.
- Principle I (Read-Only Data Access) cannot be waived or amended without
  major-version review. Its requirement that the user-supplied connection
  uses a read-only database account cannot be removed.
- Keep it simple. New dependencies need a clear justification.

Version: 1.0.0