# URL Shortener — Implementation Plan

A distributed, resume-worthy URL shortener built with ASP.NET Core, EF Core, MySQL, Redis, RabbitMQ, and Docker/Azure. Designed to demonstrate system design thinking (scaling, caching, async processing) — not just CRUD.

**Goal:** each phase should be fully working and testable before moving to the next. Don't skip ahead.

---

## Phase 1 — Core API (ASP.NET Core + EF Core + MySQL)

**Objective:** working create/redirect/stats endpoints with proper DTOs, validation, and status codes.

### Setup

- [ ] `dotnet new webapi -n UrlShortener`
- [ ] Add packages: `Pomelo.EntityFrameworkCore.MySql`, `Microsoft.EntityFrameworkCore.Design`
- [ ] Folder structure: `Controllers/`, `Models/`, `DTOs/`, `Data/`, `Services/`

### Schema

- [ ] `Urls` table: Id, ShortCode (unique index), OriginalUrl, CreatedAt, ExpiresAt (nullable)
- [ ] `Clicks` table: Id, UrlId (FK), ClickedAt, Referrer (nullable), IpAddress (nullable)
- [ ] Create `UrlShortenerDbContext`, run `dotnet ef migrations add InitialCreate`, `dotnet ef database update`

### Endpoints

- [ ] `POST /api/urls` — accepts `CreateUrlDto`, generates short code, returns `UrlResponseDto` (201)
- [ ] `GET /{shortCode}` — looks up code, logs click, redirects (302); 404 if not found
- [ ] `GET /api/urls/{shortCode}/stats` — total clicks + clicks per day; 404 if not found

### Core logic

- [ ] Short code generator: random 7-char base62 string, retry-on-collision loop, use `Random.Shared`
- [ ] Build full `ShortUrl` in response using `Request.Scheme` + `Request.Host`
- [ ] Log click (UrlId, ClickedAt, Referrer, IpAddress) synchronously inside redirect endpoint
- [ ] Stats query using `GroupBy` on click date

### Validation & polish

- [ ] Validate `OriginalUrl` with `Uri.TryCreate(..., UriKind.Absolute, ...)`
- [ ] Global exception handling middleware (no raw stack traces)
- [ ] `ILogger` logging in each endpoint
- [ ] Swagger with `[ProducesResponseType]` attributes documenting 200/201/400/404
- [ ] Keep controllers thin — logic lives in `UrlService`
- [ ] Connection string in `appsettings.json`, not hardcoded

**Checkpoint:** Shorten a URL, visit it in a browser, confirm redirect works and stats endpoint shows click counts.

---

## Phase 2 — Docker

**Objective:** containerize the API and MySQL so the whole stack runs with one command.

- [ ] Write a `Dockerfile` for the API
- [ ] Write `docker-compose.yml` with two services: `api` and `mysql`
- [ ] Move connection string to environment variables (don't bake secrets into the image)
- [ ] Confirm `docker-compose up` builds and runs the full stack from a clean machine
- [ ] Confirm EF Core migrations run correctly against the containerized DB

**Checkpoint:** `docker-compose up` gives you a fully working app with no manual setup steps.

---

## Phase 3 — Redis Caching

**Objective:** reduce DB load on the hot path (redirects) using a cache-aside pattern.

- [ ] Add Redis service to `docker-compose.yml`
- [ ] Add `StackExchange.Redis` package
- [ ] On redirect: check Redis for `shortCode → originalUrl` first; if miss, query MySQL, then populate cache
- [ ] Set a reasonable TTL (e.g., 24h) on cached entries
- [ ] On URL creation, optionally pre-populate the cache
- [ ] Be ready to explain: cache invalidation strategy, what happens on cache miss under load (thundering herd), why cache-aside vs write-through

**Checkpoint:** Redirects hit Redis instead of MySQL on repeat requests — verify with logging or Redis CLI (`MONITOR`).

---

## Phase 4 — Async Click Tracking (RabbitMQ)

**Objective:** stop blocking the redirect response on a DB write; move click logging to a background worker.

- [ ] Add RabbitMQ service to `docker-compose.yml`
- [ ] Redirect endpoint publishes a "click happened" event (UrlId, timestamp, referrer, IP) to a queue instead of writing directly to MySQL
- [ ] Build a separate background worker (`BackgroundService` or a small consumer app) that subscribes to the queue and writes clicks to MySQL
- [ ] Handle failure cases: what happens if the worker is down, message retry/dead-letter queue basics
- [ ] Be ready to explain: why this decouples redirect latency from analytics writes, and the eventual-consistency tradeoff (click count isn't instantly accurate)

**Checkpoint:** Redirect response is faster (measure before/after), and clicks still show up in stats shortly after.

---

## Phase 5 — Distributed ID Generation + Azure Deployment

**Objective:** replace auto-increment IDs with an ID scheme that works across multiple instances/shards, then deploy live.

- [ ] Implement a Snowflake-style ID generator (timestamp + machine ID + sequence) OR base62-encode the auto-increment ID as an interim step
- [ ] Explain why plain auto-increment breaks down with multiple API instances/DB shards
- [ ] Deploy to Azure App Service (or AKS if you want the extra depth) — free tier is fine
- [ ] Set up environment variables/secrets properly in Azure (not committed to repo)
- [ ] Confirm the live deployed URL works end-to-end (create, redirect, stats)

**Checkpoint:** You have a real, live URL you can put in your resume/GitHub README.

---

## Phase 6 — Load Testing, Metrics, and Polish

**Objective:** get real performance numbers and make the project presentable.

- [ ] Load test with k6 or JMeter — measure requests/sec and p99 latency on the redirect endpoint
- [ ] Add structured logging (Serilog) and/or metrics (Application Insights or Prometheus/Grafana)
- [ ] Write a README with:
  - [ ] Architecture diagram (API, MySQL, Redis, RabbitMQ, worker)
  - [ ] Explicit design decisions and tradeoffs (why Redis, why async, how you'd shard at scale)
  - [ ] Real load test numbers
  - [ ] Live demo link
- [ ] Add xUnit tests for core logic (short code generation, validation, service layer) — mock DbContext or use SQLite in-memory
- [ ] Set up GitHub Actions CI (build + test on push)

**Checkpoint:** Resume bullet ready, e.g.: _"Designed distributed URL shortener handling X req/s with sub-Yms p99 latency using Redis caching and async event processing via RabbitMQ."_

---

## Order of Operations Summary

1. Core API (this week) — familiar territory, move fast
2. Docker — mechanical, few hours
3. Redis — new concept, budget a day
4. RabbitMQ + async worker — biggest conceptual jump, budget the most time
5. Distributed IDs + Azure deploy — mostly mechanical once containerized
6. Load testing + polish — the fun part, makes numbers real

**Rule of thumb:** don't move to the next phase until the current one's checkpoint is genuinely working — not "mostly working."
