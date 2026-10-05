# Claims Platform

Claims management platform built for the Chubb APAC technical assessment.

## Technology

- Angular
- ASP.NET Core .NET 10
- EF Core
- PostgreSQL
- Docker

## Prerequisites

- Docker Desktop or another Docker Compose-compatible runtime
- .NET 10 SDK
- Node.js 24 LTS and npm 11

## Run locally

Use three terminals from the repository root.

### 1. Start PostgreSQL

```bash
docker compose up -d postgres
```

### 2. Start the API

```bash
cd backend
dotnet restore
dotnet run --project ClaimsPlatform.Api --launch-profile http
```

The API starts at `http://localhost:5234`. In Development it automatically
applies EF Core migrations and seeds deterministic demo users and claims.
OpenAPI JSON is available at `http://localhost:5234/openapi/v1.json`.

### 3. Start Angular

On the first run:

```bash
cd frontend
npm install
```

Then start the development server:

```bash
npm start
```

Open `http://localhost:4200` and choose a seeded claimant, claims officer, or
manager. Angular stores the selection locally and sends it using the
`X-Demo-User` header.

The development proxy forwards `/api` to `http://localhost:5234`, so no local
CORS configuration is required. Use the API's HTTP launch profile so the proxy
target matches.

## Verify the application

```bash
cd backend
dotnet test ClaimsPlatform.slnx
```

```bash
cd frontend
npm test
npm run build
```

## Stop PostgreSQL

```bash
docker compose down
```

This retains the database volume. To deliberately delete local database data,
run `docker compose down --volumes`.

## Project documentation

- `docs/assessment.md` contains the original requirements.
- `docs/architecture.md` explains the architecture and scope choices.
- `docs/ai_working_journal.md` records significant AI-assisted decisions.
