# Feature Specification: Project Foundation

**Feature Branch**: `001-project-foundation`

**Created**: 2026-10-25

**Status**: Draft

**Input**: User description: "Project foundation for DashCraft. A developer can clone the repository, start the whole system locally with one command, and see a working but empty application: a web app that loads in the browser, an API that responds to a health check, and a database that is connected and ready. The backend and frontend each have a working automated test setup with one passing sample test, and integration tests run against a real SQL Server instance. The repository follows the structure defined in the constitution, and the README explains how to run the system and the tests. No product features (connections, dashboards, queries, authentication) are part of this feature."

## User Scenarios & Testing *(mandatory)*

The users of this feature are DashCraft contributors and self-hosters: developers who clone the
repository and need to run, verify, and extend the system.

### User Story 1 - Start the whole system with one command (Priority: P1)

A developer clones the repository onto a machine that meets the documented prerequisites, runs a
single documented command, and the web app, the API, and the application database all start
together. The developer opens the web app in a browser and sees a DashCraft placeholder page, and
can confirm that the API is healthy and connected to its database.

**Why this priority**: Nothing else can be built, demonstrated, or self-hosted until the system
can be started reliably. This is the minimum viable foundation.

**Independent Test**: On a clean machine with only the documented prerequisites installed, clone
the repository, run the documented start command, then open the web app URL and the API health
URL. Both respond successfully and the health check reports the database as ready.

**Acceptance Scenarios**:

1. **Given** a fresh clone and the documented prerequisites installed, **When** the developer runs
   the single documented start command, **Then** the web app, API, and application database all
   start without any further manual steps.
2. **Given** the system is running, **When** the developer opens the web app address in a browser,
   **Then** a DashCraft placeholder page loads without errors and indicates whether the API is
   reachable.
3. **Given** the system is running, **When** the developer requests the API health check,
   **Then** it reports a healthy status, including that the application database is connected.
4. **Given** the system is running, **When** the developer inspects the application database,
   **Then** it exists and its schema is initialized to the current (empty) baseline version
   automatically.
5. **Given** the system is running, **When** the developer runs the documented stop command,
   **Then** all services stop cleanly, and starting again restores a working system.

---

### User Story 2 - Run the automated tests (Priority: P2)

A developer runs the documented commands to execute backend unit tests, frontend tests, and
backend integration tests. Each suite runs and passes, proving the test setup works so future
features can add tests immediately.

**Why this priority**: The constitution makes test-first development non-negotiable; every later
feature depends on working test infrastructure, but it is only useful once the system exists.

**Independent Test**: From a fresh clone, run each documented test command; each completes with
at least one passing test and a success result.

**Acceptance Scenarios**:

1. **Given** a fresh clone with prerequisites installed, **When** the developer runs the backend
   unit test command, **Then** every backend project's test project runs and reports at least one
   passing sample test.
2. **Given** a fresh clone with prerequisites installed, **When** the developer runs the frontend
   test command, **Then** at least one sample component test runs and passes.
3. **Given** a fresh clone with prerequisites installed, **When** the developer runs the
   integration test command, **Then** a real, disposable SQL Server instance is provisioned
   automatically, at least one test verifies the API health check against it, and the instance is
   removed afterward.
4. **Given** a change that breaks a sample test, **When** the tests run, **Then** the run fails
   with a non-zero result so automation can detect it.

---

### User Story 3 - Understand the project from the README (Priority: P3)

A new contributor reads the README and, without outside help, understands what DashCraft is, the
prerequisites, how to start and stop the system, how to run each test suite, and where the
backend, frontend, and tests live.

**Why this priority**: Documentation multiplies the value of Stories 1 and 2 for every future
contributor and self-hoster, but the system must work first.

**Independent Test**: Give the README to a developer unfamiliar with the project; they start the
system and run all tests by following only the README.

**Acceptance Scenarios**:

1. **Given** the README, **When** a new developer follows it step by step, **Then** they can start
   the system and run all three test suites without consulting anything else.
2. **Given** the README, **When** a developer looks for where code belongs, **Then** it describes
   the repository layout consistent with the constitution.

---

### Edge Cases

- **Database not yet ready at startup**: The API MUST wait for or retry the database connection
  rather than failing permanently; the health check reports unhealthy until the database is ready.
- **Database becomes unavailable while running**: The health check MUST report unhealthy (and say
  that the database is the cause) instead of reporting healthy.
- **API unreachable from the web app**: The placeholder page MUST still load and show a clear,
  non-technical message that the API is not reachable.
- **Required ports already in use**: Startup fails with a clear message, and the README documents
  which ports are used and how to change them.
- **Missing prerequisites**: The README lists exact prerequisites so the developer can tell why
  startup or tests fail.
- **Restart with existing data**: Restarting the system MUST NOT reset or corrupt existing
  application database data, and schema initialization MUST be safe to run repeatedly.
- **Integration tests with no container runtime available**: The integration test run fails with
  a clear message rather than hanging.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST start the web app, the API, and the application database with a
  single documented command from the repository root.
- **FR-002**: The system MUST stop all of its services with a single documented command.
- **FR-003**: The web app MUST serve a DashCraft placeholder page in a browser that shows the
  product name and whether the API is reachable and healthy.
- **FR-004**: The API MUST expose an unauthenticated health check that reports overall status and
  the status of the application database connection.
- **FR-005**: The health check MUST report unhealthy when the application database is unreachable.
- **FR-006**: The application database MUST be created and its schema initialized to the baseline
  version automatically on startup, and this initialization MUST be safe to repeat.
- **FR-007**: Application database data MUST persist across stop/start cycles.
- **FR-008**: Configuration (ports, database credentials, connection settings) MUST be provided
  through environment variables or config files, with working local defaults and an example
  configuration file; no real secrets may be committed to the repository.
- **FR-009**: The repository MUST follow the layout defined in the constitution: separate
  backend and frontend apps, backend tests in their own location, layered backend projects each
  with a matching unit test project, and a separate integration test project.
- **FR-010**: Each backend project MUST have a unit test project containing at least one passing
  sample test, runnable with a single documented command.
- **FR-011**: The frontend MUST have a test setup with at least one passing sample component test
  (for the placeholder page), runnable with a single documented command.
- **FR-012**: Integration tests MUST run against a real SQL Server instance that is provisioned and
  cleaned up automatically by the test run, and MUST include a test that calls the health check
  and verifies it reports the database as healthy.
- **FR-013**: Every test command MUST return a failing result when any test fails.
- **FR-014**: The README MUST describe: what DashCraft is, prerequisites, how to start and stop the
  system, the addresses of the web app and health check, how to run each test suite, the
  repository layout, and how to change configuration.
- **FR-015**: The system MUST NOT require any cloud service or send any telemetry.
- **FR-016**: All dependencies introduced MUST have licenses compatible with open-source
  distribution, per the constitution.
- **FR-017**: No product features (database connections, dashboards, queries, user accounts,
  authentication) may be included in this feature.

### Key Entities

- **Application database**: DashCraft's own database that will later store users, connections,
  dashboards, and saved queries. In this feature it only exists, is reachable, and is initialized
  to an empty baseline schema with version tracking.
- **Health status**: The report returned by the health check: an overall status plus the status of
  each dependency (initially just the application database).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer with prerequisites installed goes from a fresh clone to a working web
  app and healthy health check in under 10 minutes, using only the README (excluding one-time
  download time).
- **SC-002**: After the start command finishes, the system becomes healthy within 2 minutes on
  a typical developer laptop.
- **SC-003**: 100% of test suites (backend unit, frontend, integration) pass on a fresh clone with
  no manual setup beyond the documented prerequisites.
- **SC-004**: The placeholder page loads in under 3 seconds once the system is healthy.
- **SC-005**: Stopping and restarting the system 3 times in a row results in a healthy system each
  time with no data loss in the application database.
- **SC-006**: A developer unfamiliar with the project can locate where backend, frontend, and test
  code belong within 2 minutes using only the README.

## Assumptions

- The audience is developers and self-hosters; there is no end-user functionality in this feature.
- Prerequisites are a container runtime with compose support (the constitution's supported
  baseline), plus the backend and frontend toolchains for running tests outside containers.
- The single start command runs the system in containers; local defaults are suitable for
  development only, not production hardening.
- The health check is intentionally unauthenticated because authentication is out of scope; it
  exposes no secrets or sensitive details.
- The application database is SQL Server, as it is the only engine in scope per the constitution.
  User-connected databases are not part of this feature.
- Continuous integration pipeline setup is out of scope for this feature; test commands are
  designed so CI can run them later.
