# Online CV

<<<<<<< HEAD
The project is separated into the original React/Vite frontend and an ASP.NET Core backend API.

```text
frontend/   React + Vite CV interface
backend/    ASP.NET Core API, business services, persistence, and tests
=======
Interactive Blazor WebAssembly CV for Corniță Cristian. The app includes a responsive online CV, project case studies, professional details, education and certificates, compact print styles for PDF export, and JSON translations for English, Romanian, and Russian.

## Scripts

```bash
dotnet run
dotnet build
>>>>>>> 1dfc6c7e26fdc9d0648c26378dfbc7fbedd188db
```

The backend stores data in Microsoft SQL Server. The Development configuration connects to
`.\SQLEXPRESS` with Windows authentication and creates the `OnlineCV` database automatically.

<<<<<<< HEAD
## Run locally
=======
- `Pages/Home.razor` contains the Blazor CV rendering and interactions.
- `Models/CvContent.cs` contains the typed CV content model.
- `wwwroot/data/translations.json` contains the English, Romanian, and Russian translation resources.
- `wwwroot/css/app.css` contains the screen, responsive, and print layouts.
- `wwwroot/favicon.svg` is the browser favicon.
>>>>>>> 1dfc6c7e26fdc9d0648c26378dfbc7fbedd188db

Start the API from the project root:

```bash
dotnet run --project backend/OnlineCV.Api.csproj
```

The API runs at `http://localhost:8080` and opens Swagger UI automatically at `http://localhost:8080/swagger`. Useful routes include:

- `GET /`
- `GET /health`
- `GET /api/cv`
- `GET /api/profile`
- `GET /api/metrics`
- `GET /api/skills`
- `POST/PUT/DELETE /api/skills/{id}` (Admin JWT required)
- `GET /api/projects`
- `POST/PUT/DELETE /api/projects/{id}` (Admin JWT required)
- `GET /api/experience`
- `GET /api/education`
- `GET /api/certificates`
- `POST /api/contact`
- `GET/PATCH/DELETE /api/contact/submissions` (Admin JWT required)
- `GET /swagger`

Start the original frontend in a second terminal:

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:5173` and loads all CV content and interface text from `GET http://localhost:8080/api/cv`.

If the API is offline, returns an error, or returns incomplete CV content, the public site automatically displays a maintenance page with a retry action. Preview that state without stopping the API at `http://localhost:5173/?maintenance=1`.

## Admin editor

Open `http://localhost:5173/admin` to sign in and edit the complete CV content with labeled forms. The editor groups content by section, previews unsaved changes, and provides a synchronized **Section JSON** editor containing only the selected category. In local Development mode, the initial credentials are:

```text
Username: admin
Password: ChangeMe123!
```

Change these before deployment by setting `ADMIN_USERNAME`, `ADMIN_PASSWORD`, and a random `JWT_KEY` of at least 32 characters. The editor keeps the JWT in session storage and saves through the protected `PUT /api/cv` endpoint.

The backend validates and normalizes every write, rejects duplicate project IDs and skill groups, enforces valid scores, email addresses, and links, and persists contact submissions separately from public CV content. See [`backend/README.md`](backend/README.md) for the complete API and architecture.

## Build

```bash
cd frontend
npm run build
dotnet build ../backend/OnlineCV.Api.csproj
```

Run the backend unit suite from the repository root:

```powershell
dotnet test backend/OnlineCV.Api.slnx
```

Both services can also be started with Docker. Create a local `.env` from `.env.example`, replace the password and JWT key, then run:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Docker runs SQL Server on port `1433` and persists it in the `sqlserver-data` volume.

## Open the database in SSMS

For a normal Visual Studio or `dotnet run` launch, connect SSMS with:

```text
Server name: .\SQLEXPRESS
Authentication: Windows Authentication
Database: OnlineCV
```

The API creates `dbo.CvDocuments` and `dbo.ContactSubmissions` on first startup. The current CV
is seeded from `backend/data/cv-content.json` only when `dbo.CvDocuments` is empty. Useful SSMS
queries are provided in `backend/database/inspect.sql`.
