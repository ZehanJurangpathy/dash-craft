/speckit.plan
Follow the constitution. Create:

Backend (/src/api): a .NET 10 solution with Clean Architecture projects
(Api, Application, Domain, Infrastructure), plus unit test projects per
project and a separate integration test project under /tests/api.
- Packages: Mediator (martinothamar, with its source generator), Riok.Mapperly,
  EF Core SQL Server provider with migrations, Dapper, Microsoft.Data.SqlClient,
  Microsoft.SqlServer.TransactSql.ScriptDom, FluentValidation, xUnit, Shouldly,
  Microsoft.AspNetCore.Mvc.Testing, Testcontainers for SQL Server.
- Minimal API with endpoint groups, a /health endpoint, ProblemDetails error
  handling, structured logging, OpenAPI.
- DbContext for the DashCraft application database in Infrastructure, with an
  initial empty migration.

Frontend (/src/web): Vite + React + TypeScript (strict), React Router,
TanStack Query and Table, Tailwind CSS, React Hook Form with Zod and
@hookform/resolvers, Vitest and React Testing Library. Feature-based folder
structure from the constitution, a home route, and an API client layer that
calls the health endpoint.

Repository root: docker-compose with SQL Server, the API and the web app;
Dockerfiles for the API and the web app; .gitignore, .editorconfig, and a
README with run and test instructions. Add a CI workflow that builds and runs
all unit and integration tests.

Check the latest stable version and license of every package before pinning it,
and confirm each is compatible with .NET 10. Flag any package that does not
meet the constitution's licensing rule.