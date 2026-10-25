I want to build a project which does the following.

DashCraft is an open-source, self-hostable dashboard builder for SQL Server databases that empowers users to visually query relational databases, join tables, select columns, and create interactive dashboards without writing SQL. 

Technologies to use:
Backend API:
 - .NET 10 Minimal API
 - Mapperly for type mapping
 - CQRS using the Mediator library (martinothamar, MIT, source-generated)
 - EF Core Code First for CRUD operations with DashCraft database
 - Dapper for querying data from the user specified database which reads data for the dashboard widgets
 - Testing using xUnit and Shouldly for Assertion

Frontend:
 - React + Typescript
 - Tanstack
 - React Router
 - Tailwind css
 - React hook form + zod validation

## Project Structure

### Repository layout
- Monorepo with two top-level apps: `/src/api` (.NET 10) and `/src/web`
  (React + TypeScript). Tests live alongside each app (`/tests/api`, and
  `*.test.tsx` next to components or under `/src/web/tests`).
- Docker and docker-compose files at the repository root.

### Backend structure
- Follows Clean Architecture
- Layers: Api (endpoints, DI, middleware) -> Application (features, handlers,
  abstractions) -> Domain (entities, rules) <- Infrastructure (EF Core,
  database providers, query execution). Domain has no dependencies on other layers.
- The query builder and SELECT-only validator live in a dedicated, independently
  testable module with no dependency on the web layer.
- EF Core migrations and DbContext live in Infrastructure.
- Database provider implementations (schema discovery, SQL generation) are
  separate classes behind interfaces, one per supported database.

### Frontend structure
- Organize by feature under `src/features/<feature>` (components, hooks,
  API calls, Zod schemas, types). Shared UI primitives go in `src/components`,
  shared utilities in `src/lib`.
- API calls and TanStack Query hooks live in each feature's `api` folder,
  never directly in components.
- Routes are defined centrally with React Router; route components are thin
  and compose feature components.
- Zod schemas sit next to the forms that use them, and types are inferred
  from them.

The application should only generate SQL select queries and should not generate any query which mutates data.

Unit tests for each project and integration tests are mandatory.

There will be two user roles (User and Admin). The Admin has the ability to write SELECT SQL statements ONLY in order to generate complex select queries that is need to generate charts/tables needed for the dashboard which cannot be done easily using
the interactive editor. All the other users DOES NOT have this ability.