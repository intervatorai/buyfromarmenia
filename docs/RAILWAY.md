# Railway deployment (Docker)

BuyFromArmenia deploys as **one Railway project** with multiple Docker services from this monorepo.

## Services

| Railway service | Dockerfile | Healthcheck | Notes |
|-----------------|------------|-------------|--------|
| `postgres` | Railway Postgres plugin | — | Shared DB |
| `hangfire` | `docker/hangfire.Dockerfile` | `/health` | Migrations + jobs — deploy first |
| `public-api` | `docker/public-api.Dockerfile` | `/health` | Storefront API |
| `admin-api` | `docker/admin-api.Dockerfile` | `/health` | Admin API |
| `supplier-api` | `docker/supplier-api.Dockerfile` | `/health` | Partner API |
| `public-ui` | `docker/public-ui.Dockerfile` | `/` | Next.js storefront |
| `admin-ui` | `docker/admin-ui.Dockerfile` | `/` | Next.js admin |
| `supplier-ui` | `docker/supplier-ui.Dockerfile` | `/` | Next.js partner LK |

Containers listen on Railway `$PORT` (.NET entrypoint / Next `server.js`).

## Critical UI settings (avoids Next.js 404)

With `output: "standalone"`, **`next start` must not be used** — it serves a broken 404 shell.

### Option A — Docker (recommended, monorepo)

1. **Root Directory**: empty (repo root)
2. **Builder**: Dockerfile  
3. **Dockerfile path**: `docker/public-ui.Dockerfile` (etc.)
4. **Custom Start Command**: leave **empty** (use image `CMD`: `node server.js`)
5. Docker Build Args: `NEXT_PUBLIC_API_URL=https://<public-api>.up.railway.app`

### Option B — Nixpacks (Root Directory = `src/BFA.Public.UI`)

Uses `src/BFA.Public.UI/railway.toml`: build prepares standalone, start runs `node .next/standalone/server.js`.

Still set `NEXT_PUBLIC_API_URL` as a **build-time** variable so it is inlined into the client bundle.

**Common mistake:** pointing `NEXT_PUBLIC_API_URL` at the **public UI** domain (or leaving it empty). The browser then calls `/api/products` on Next.js, gets an HTML 404 page, and the home “Popular products” section used to dump that HTML on screen. It must be the **public-api** URL, e.g. `https://<public-api>.up.railway.app` (no trailing slash). After changing it, **rebuild** public-ui.

## Per-service settings

For **every** API / Hangfire Docker service:

1. **Root Directory**: leave empty (repository root) — Docker needs the whole monorepo.
2. **Builder**: Dockerfile
3. **Dockerfile path**: as in the table above
4. Generate a public domain (or attach custom domains)

Recommended watch paths (optional, speeds up rebuilds):

- APIs: `/src/BFA.<Name>.Api/**`, `/src/BFA.<Name>.Application/**`, `/src/Modules/**`, `/src/BFA.Persistence/**`, `/src/BFA.Infrastructure/**`, `/docker/**`
- UIs: `/src/BFA.<Name>.UI/**`, `/docker/**`
- Hangfire: `/src/BFA.Hangfire/**`, `/src/BFA.Hangfire.Application/**`, `/src/BFA.Persistence/**`, `/docker/**`

## Environment variables

### Shared (all APIs + Hangfire)

```
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
```

`${{Postgres.DATABASE_URL}}` is usually a `postgresql://…` URI. The apps convert it to Npgsql key=value form automatically. You can also paste an explicit Npgsql string:

`Host=…;Port=…;Database=…;Username=…;Password=…;SSL Mode=Require;Trust Server Certificate=true`

### Public API

```
Jwt__Secret=<long random>
Cors__AllowedOrigins__0=https://<public-ui>.up.railway.app
Stripe__Enabled=true|false
Stripe__SecretKey=…
Stripe__PublishableKey=…
Stripe__WebhookSecret=…
Stripe__SuccessUrl=https://<public-ui>/orders/{ORDER_ID}?checkout=success
Stripe__CancelUrl=https://<public-ui>/checkout?checkout=cancelled
Media__PublicBaseUrl=…
```

### Admin / Supplier API

```
Jwt__Secret=<long random, distinct per API>
Cors__AllowedOrigins__0=https://<matching-ui>.up.railway.app
```

Admin API also needs the **same** public customer JWT settings as Public API (for “Open as customer” impersonation):

```
PublicJwt__Secret=<same as Public API Jwt__Secret>
PublicJwt__Issuer=BFA.Public.Api
PublicJwt__Audience=BFA.Public.UI
PublicJwt__ExpirationHours=2
```

### UI build args (Docker)

`NEXT_PUBLIC_*` is baked in at **image build** time. In Railway service settings → Build → Docker Build Args:

| Service | Build arg | Value |
|---------|-----------|--------|
| `public-ui` | `NEXT_PUBLIC_API_URL` | `https://<public-api>.up.railway.app` |
| `public-ui` | `NEXT_PUBLIC_SUPPLIER_API_URL` | `https://<supplier-api>.up.railway.app` (optional) |
| `public-ui` | `NEXT_PUBLIC_SUPPLIER_URL` | `https://<supplier-ui>.up.railway.app` (partner portal footer link) |
| `public-ui` | `NEXT_PUBLIC_GOOGLE_MAPS_API_KEY` | maps key (optional) |
| `admin-ui` | `NEXT_PUBLIC_API_URL` | `https://<admin-api>.up.railway.app` |
| `admin-ui` | `NEXT_PUBLIC_PUBLIC_SITE_URL` | `https://<public-ui>.up.railway.app` |
| `supplier-ui` | `NEXT_PUBLIC_API_URL` | `https://<supplier-api>.up.railway.app` |

After API public URLs change, **rebuild** the matching UI.

## Deploy order

1. Add Railway Postgres
2. Deploy `hangfire` (applies EF migrations)
3. Deploy the three APIs
4. Deploy the three UIs with build args pointing at API URLs
5. Point custom domains + update CORS / Stripe URLs

## Local Docker smoke test

```bash
docker compose up -d
docker compose -f docker-compose.yml -f docker-compose.apps.yml up --build
```

## Build one image manually

```bash
docker build -f docker/public-api.Dockerfile -t bfa-public-api .
docker build -f docker/public-ui.Dockerfile \
  --build-arg NEXT_PUBLIC_API_URL=http://localhost:5100 \
  -t bfa-public-ui .
```
