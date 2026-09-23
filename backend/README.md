# OnLineCV API

ASP.NET Core API for public CV content, administrator editing, and contact submissions.

## Architecture

```text
Endpoints/       HTTP routes and response mapping
Services/        validation, normalization, and business operations
Repositories/    SQL-backed persistence adapters
Models/          API and domain contracts
Infrastructure/  global exception handling
Data/            EF Core SQL Server context and database initialization
data/            first-run CV seed data
database/        useful SSMS queries
tests/           service and validation tests
```

`PUT /api/cv` remains the main admin-editor contract. Every granular operation uses the same
validator and repository, so all write paths enforce the same rules.

Each list section of the CV is described once by a `CvSection<T>` in `Services/CvSections.cs`:
where its list lives, how an entry is addressed, and which field to report on a conflict. One
generic set of operations in `CvService` and one route mapper in `Endpoints/CvSectionEndpoints.cs`
serve every section from those descriptors, so adding a section means adding a descriptor and one
line of wiring rather than another CRUD trio.

## Business rules

- Every CV write is trimmed and normalized before persistence.
- Profile name, role, email, and summary are required.
- Profile and contact email addresses must be valid.
- The CV must have between 1 and 50 projects.
- Project IDs are normalized slugs and must be unique.
- Repository links must be absolute HTTP or HTTPS URLs.
- Skill titles must be unique and scores must be between 0 and 100.
- Experience requires a role and company, and must be unique by the two together.
- Every other list entry requires its title, label, or area, and that text must be unique within
  its section and must contain at least one letter or number. Granular routes address an entry by
  the slug of that text, so duplicates would make an update or delete ambiguous.
- Metrics require a value as well as a label.
- Navigation and contact destinations must use supported link formats.
- Contact name, email, and message lengths are validated.
- Contact submissions have `New`, `Read`, and `Archived` states.
- Login and public contact submission endpoints are rate limited.
- All mutating CV and contact-administration routes require an Admin JWT.

## Public endpoints

- `GET /health`
- `POST /api/auth/login`
- `GET /api/cv`
- `GET /api/profile`
- `GET /api/metrics`
- `GET /api/skills`
- `GET /api/skills/{id}`
- `GET /api/projects`
- `GET /api/projects/{id}`
- `GET /api/experience`
- `GET /api/education`
- `GET /api/certificates`
- `POST /api/contact`

## Admin endpoints

- `PUT /api/cv`
- `PUT /api/profile`
- `GET/POST/PUT/DELETE /api/projects[/{id}]`
- `GET/POST/PUT/DELETE /api/skills[/{id}]`
- `GET/POST/PUT/DELETE /api/experience[/{id}]`
- `GET/POST/PUT/DELETE /api/credentials[/{id}]`
- The same list operations are available for metrics, achievements, navigation, contact methods,
  engineering standards, stack areas, professional details, workflow, strengths, role fit,
  role signals, and languages.
- `PUT /api/collaboration`
- `PUT /api/summary-highlights`
- `PUT /api/tools`
- `PUT /api/ui-text`
- `GET /api/contact/submissions?status=New`
- `GET /api/contact/submissions/{id}`
- `PATCH /api/contact/submissions/{id}/status`
- `DELETE /api/contact/submissions/{id}`

Validation failures use RFC 7807 validation responses. Missing resources return `404`, duplicates return `409`, unauthenticated writes return `401`, and rate limits return `429`.

## Run

```powershell
dotnet run --project backend/OnlineCV.Api.csproj
```

Swagger is available at `http://localhost:8080/swagger`. In local Development mode, use `admin` / `ChangeMe123!` with `POST /api/auth/login`, then paste the returned token into Swagger's **Authorize** dialog.

## Configuration

| Setting | Environment variable | Purpose |
| --- | --- | --- |
| `Admin:Username` | `ADMIN_USERNAME` | Administrator username |
| `Admin:Password` | `ADMIN_PASSWORD` | Administrator password |
| `Jwt:SigningKey` | `JWT_KEY` | JWT signing secret, minimum 32 characters |
| `Jwt:Issuer` | `JWT_ISSUER` | Token issuer |
| `Jwt:Audience` | `JWT_AUDIENCE` | Token audience |
| `Jwt:LifetimeHours` | `JWT_LIFETIME_HOURS` | Token lifetime, clamped to 1-24 hours |
| `ConnectionStrings:OnlineCv` | `ConnectionStrings__OnlineCv` | SQL Server connection string |
| `SeedData:CvPath` | - | Initial CV seed JSON path |
| `SeedData:ContactPath` | - | Optional legacy contact import path |
| `Cors:Origins` | `CORS_ORIGIN` | Allowed frontend origins |

Development uses `.\SQLEXPRESS`, the `OnlineCV` database, and Windows authentication. The API
creates the schema automatically and imports the CV seed only when the database is empty.

`dbo.CvDocuments` stores the complete CV as an atomic JSON document so one admin save cannot leave
related sections out of sync. `dbo.ContactSubmissions` stores contact messages in relational columns.

## Verification

With the API running, execute the end-to-end smoke suite from the project root:

```powershell
.\backend\tests\smoke.ps1
```

The script verifies health, JWT issuance, authorization, CV validation, project and skill CRUD, duplicate conflicts, contact validation, persistence, and status updates. Temporary records are removed in a `finally` block.
