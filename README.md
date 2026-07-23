# BuyFromArmenia (BFA)

Marketplace for Armenian products: Public storefront, Supplier portal, Admin backoffice.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 9, ASP.NET Core, MediatR (CQRS), EF Core 9 |
| Database | PostgreSQL 16 |
| Jobs | Hangfire (migrations, outbox, seeds) |
| Frontend | Next.js (App Router), TypeScript |

## Project Structure

```
buyfromarmenia/
├── src/
│   ├── BuildingBlocks/          # Domain + Application shared types
│   ├── Modules/                 # Bounded contexts (Catalog, Ordering, …)
│   ├── BFA.Public.Api / .UI
│   ├── BFA.Supplier.Api / .UI
│   ├── BFA.Admin.Api / .UI
│   ├── BFA.Hangfire             # Background jobs + auto-migrations
│   ├── BFA.Persistence          # EF Core + migrations
│   └── BFA.Infrastructure       # Auth, outbox processor, fulfillment
├── tests/BFA.IntegrationTests
├── docker-compose.yml           # PostgreSQL for local dev
├── docker-compose.apps.yml      # Optional: build/run all app containers
├── docker/                      # Dockerfiles for Railway
├── docs/DEVELOPMENT_PLAN.md
├── docs/RAILWAY.md              # Railway + Docker deploy guide
└── README.md
```

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Start infrastructure

```bash
docker compose up -d
```

This starts **PostgreSQL 16** (database `bfa`, user/password `postgres`) on port `5432`.

Check health:

```bash
docker compose ps
```

Restart Postgres only:

```bash
./scripts/restart-docker-infra.sh
```

**DataGrip / IDE data source**

| Field | Value |
|-------|--------|
| Host | `localhost` |
| Port | `5432` |
| User | `postgres` |
| Password | `postgres` |
| Database | **`bfa`** |

JDBC: `jdbc:postgresql://localhost:5432/bfa`

### 2. Apply migrations (via Hangfire)

Hangfire applies EF migrations on startup and seeds the default admin + categories.

```bash
dotnet run --project src/BFA.Hangfire
```

Dashboard: [http://localhost:5103/hangfire](http://localhost:5103/hangfire)

Leave this process running while developing (also processes outbox / fulfillment).

Optional (manual EF update, same DB):

```bash
dotnet ef database update \
  --project src/BFA.Persistence \
  --startup-project src/BFA.Admin.Api
```

### 3. Run APIs (separate terminals)

```bash
dotnet run --project src/BFA.Public.Api      # http://localhost:5100
dotnet run --project src/BFA.Admin.Api       # http://localhost:5101
dotnet run --project src/BFA.Supplier.Api    # http://localhost:5102
```

Connection strings already point at Docker Postgres (`Host=localhost;Port=5432;Database=bfa;…`).

### 4. Run UIs (separate terminals)

```bash
cd src/BFA.Public.UI && npm install && npm run dev     # http://localhost:3200
cd src/BFA.Admin.UI && npm install && npm run dev      # http://localhost:3201
cd src/BFA.Supplier.UI && npm install && npm run dev   # http://localhost:3202
```

Copy `.env.example` → `.env.local` in each UI if needed (`NEXT_PUBLIC_API_URL`).

### Ports summary

| Service | URL |
|---------|-----|
| Public UI | http://localhost:3200 |
| Admin UI | http://localhost:3201 |
| Supplier UI | http://localhost:3202 |
| Public API | http://localhost:5100 |
| Admin API | http://localhost:5101 |
| Supplier API | http://localhost:5102 |
| Hangfire | http://localhost:5103/hangfire |
| PostgreSQL | localhost:5432 |

### Dev credentials

| App | Login | Password |
|-----|-------|----------|
| Admin | `admin` | `admin` |

Supplier: register via `/onboarding`, then `/login`.  
Customer: register via Public `/account/register`.

## Docs

- Development plan: [`docs/DEVELOPMENT_PLAN.md`](./docs/DEVELOPMENT_PLAN.md)
- Railway (Docker) deploy: [`docs/RAILWAY.md`](./docs/RAILWAY.md)

## Production (Railway)

Deploy each API/UI/Hangfire service from this repo using the Dockerfiles under [`docker/`](./docker/). See [`docs/RAILWAY.md`](./docs/RAILWAY.md) for service list, env vars, and build args.
