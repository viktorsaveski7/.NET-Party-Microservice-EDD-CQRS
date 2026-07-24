# Party Microservices

A **children's birthday party management system** built on a **.NET 9 microservices architecture**, demonstrating Clean Architecture, CQRS, event-driven communication, the Transactional Outbox pattern, and API Gateway routing.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                         CLIENT                               │
│                    (Browser / SPA)                           │
└─────────────────────┬────────────────────────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────────────────────────┐
│                 ApiGateway (port 5000)                       │
│                   YARP Reverse Proxy                         │
│           /api/parties/*  ──▶  PartyService                  │
│           /api/guests/*   ──▶  GuestService                  │
└──────────┬──────────────────────────────────┬───────────────┘
           │                                  │
           ▼                                  ▼
┌──────────────────────┐       ┌──────────────────────────────┐
│    PartyService      │       │       GuestService           │
│    (port 5001)       │       │       (port 5002)            │
│                      │       │                              │
│  ┌────────────────┐  │       │  ┌────────────────────────┐  │
│  │  REST API      │  │       │  │  REST API              │  │
│  │  Controllers   │  │       │  │  Controllers           │  │
│  └───────┬────────┘  │       │  └───────────┬────────────┘  │
│  ┌───────▼────────┐  │       │  ┌───────────▼────────────┐  │
│  │  MediatR/CQRS  │  │       │  │  MediatR/CQRS          │  │
│  └───────┬────────┘  │       │  └───────────┬────────────┘  │
│  ┌───────▼────────┐  │       │  ┌───────────▼────────────┐  │
│  │ Dapper + Redis │  │       │  │ EF Core (SQL Server)   │  │
│  └────────────────┘  │       │  │ + CachedParty table    │  │
│                      │       │  └────────────────────────┘  │
│  ┌────────────────┐  │       │                              │
│  │ Outbox         │──┼───────┼──▶  RabbitMQ                │
│  │ Processor      │  │       │     (party events)          │
│  │ (Background)   │  │       │                              │
│  └────────────────┘  │       │  ┌────────────────────────┐  │
└──────────────────────┘       │  │ PartyEventConsumer     │  │
                               │  │ (BackgroundService)    │  │
         ┌─────────────────────┼──│ consumes party events  │  │
         │                     │  └────────────────────────┘  │
         ▼                     └──────────────────────────────┘
┌────────────────────┐
│     RabbitMQ       │
│  Exchange:         │
│  "party-events"    │
│  (Topic Exchange)  │
└────────────────────┘
```

---

## Tech Stack

| Category           | Technology                                      |
|--------------------|-------------------------------------------------|
| Language           | C# (.NET 9)                                     |
| Web Framework      | ASP.NET Core (Controller-based Web API)         |
| API Gateway        | YARP Reverse Proxy 2.2                          |
| CQRS / Mediator    | MediatR 14.0                                    |
| Validation         | FluentValidation 12.1                           |
| Data Access (Party)| Dapper 2.1 + DbUp 5.0 (migrations)              |
| Data Access (Guest)| Entity Framework Core 9.0                       |
| Database           | SQL Server (two databases)                      |
| Cache              | Redis (StackExchange.Redis)                     |
| Message Broker     | RabbitMQ 7.2 (Topic Exchange)                   |
| API Docs           | Swashbuckle / Swagger 6.6                       |
| Containerization   | Docker (PartyEventConsumerService)              |

---

## Services

### 1. ApiGateway (port 5000)

Single entry point using YARP Reverse Proxy. All client requests go through this gateway.

| Route               | Forwards To               |
|---------------------|---------------------------|
| `/api/parties/*`    | `http://localhost:5001`   |
| `/api/guests/*`     | `http://localhost:5002`   |

CORS is configured for `http://localhost:3000` (SPA frontend).

---

### 2. PartyService (port 5001)

Manages birthday party CRUD operations and publishes domain events.

**Architecture:** Clean Architecture (Presentation → Application → Domain ← Infrastructure)

| Layer          | Responsibility                                   |
|----------------|--------------------------------------------------|
| **Domain**     | `Party` entity (pure domain logic)               |
| **Application**| CQRS commands/queries, DTOs, validators, interfaces|
| **Infrastructure**| Dapper repositories, Redis cache, RabbitMQ publisher, Outbox processor |
| **Presentation**| REST controllers, global exception middleware |

**Endpoints:**

| Method   | Route               | Description                  |
|----------|---------------------|------------------------------|
| `POST`   | `/api/parties`      | Create a new party           |
| `GET`    | `/api/parties`      | Get all parties (cached)     |
| `GET`    | `/api/parties/{id}` | Get party by ID (cached)     |
| `PUT`    | `/api/parties/{id}` | Update a party               |
| `DELETE` | `/api/parties/{id}` | Delete a party               |

**Key Design Decisions:**
- **Dapper** over EF Core for lightweight, high-performance queries
- **DbUp** for SQL-based, script-first migrations (runs at startup)
- **Redis caching** via decorator pattern (`CachedPartyRepository` wraps `PartyRepository`). 5-minute sliding expiration, cache invalidated on writes.
- **Transactional Outbox Pattern** for reliable event publishing (see below)

---

### 3. GuestService (port 5002)

Manages guest CRUD operations and maintains a local cache of party data synchronized via RabbitMQ events.

**Architecture:** Clean Architecture (same 4-layer structure as PartyService)

| Layer          | Responsibility                                      |
|----------------|-----------------------------------------------------|
| **Domain**     | `Guest` entity, `CachedParty` entity                |
| **Application**| CQRS handlers, event handlers, DTOs, interfaces     |
| **Infrastructure**| EF Core DbContext, repositories, RabbitMQ consumer (BackgroundService) |
| **Presentation**| REST controllers, global exception middleware    |

**Endpoints:**

| Method   | Route               | Description                          |
|----------|---------------------|--------------------------------------|
| `POST`   | `/api/guests`       | Create a guest (validates party exists) |
| `GET`    | `/api/guests`       | Get all guests                       |
| `GET`    | `/api/guests/{id}`  | Get guest by ID                      |
| `PUT`    | `/api/guests/{id}`  | Update a guest                       |
| `DELETE` | `/api/guests/{id}`  | Delete a guest                       |

**Key Design Decisions:**
- **EF Core** for richer ORM capabilities on the guest side
- **CachedParties** table — a local read-model of parties synchronized via RabbitMQ events. When creating a guest, the service validates the parent party exists in its local cache rather than calling PartyService synchronously.
- **Idempotent event handling** — `PartyCreatedEventHandler` checks for duplicates before inserting.

---

### 4. PartyEventConsumerService (standalone console app)

A standalone .NET console application that also consumes the `guest-service-party-events` queue from RabbitMQ. This exists as an alternative deployment model where event consumption runs as a separate process/container.

> **Note:** This service shares the same queue as GuestService's built-in `PartyEventConsumer`. In production only one consumer model should be used to avoid competing consumer behavior.

**Has a Dockerfile** for containerized deployment.

---

## Communication Patterns

### Synchronous (REST)

```
Client ──▶ ApiGateway (YARP) ──▶ PartyService / GuestService
```

Standard HTTP REST/JSON for all CRUD operations. The API Gateway routes requests based on the URL path.

### Asynchronous (Event-Driven)

```
PartyService ──(publishes)──▶ RabbitMQ Topic Exchange "party-events"
                                    │
                    ┌───────────────┼───────────────┐
                    ▼                               ▼
           guest-service-party-events queue    (extensible to more consumers)
                    │
                    ▼
            GuestService (PartyEventConsumer)
```

**Events published:**

| Event             | Routing Key       | Triggered By               |
|-------------------|-------------------|----------------------------|
| `PartyCreatedEvent`| `party.created`   | `POST /api/parties`        |
| `PartyUpdatedEvent`| `party.updated`   | `PUT /api/parties/{id}`    |
| `PartyDeletedEvent`| `party.deleted`   | `DELETE /api/parties/{id}` |

**RabbitMQ Configuration:**
- Exchange: `party-events` (Topic, durable)
- Queue: `guest-service-party-events` (durable)
- Auth: `admin` / `admin123`

---

## The Transactional Outbox Pattern

PartyService implements the **Transactional Outbox Pattern** to guarantee reliable event delivery:

1. **Write phase:** When a party is created/updated/deleted, instead of publishing directly to RabbitMQ, the domain event is saved to an `OutboxMessages` SQL table **in the same database transaction** as the party data change.
2. **Publish phase:** A background `OutboxProcessorService` (runs every 5 seconds) polls the outbox table for unprocessed messages and publishes them to RabbitMQ.
3. **Retry strategy:** Exponential backoff — 5s, 10s, 20s, 40s, 80s, 160s (cap at 300s). Retry count and last error are recorded for each message.

```
┌──────────────┐     ┌──────────────────┐     ┌──────────┐
│ Create Party │────▶│ DB Transaction   │────▶│ RabbitMQ │
│  (Command)   │     │  ├─ Parties table│     │ (async)  │
└──────────────┘     │  └─ Outbox table │     └──────────┘
                     └────────┬─────────┘
                              │ (poll every 5s)
                     ┌────────▼─────────┐
                     │ OutboxProcessor  │
                     │ (BackgroundSvc)  │
                     └──────────────────┘
```

This ensures **at-least-once delivery** even if RabbitMQ is temporarily unavailable.

---

## Design Patterns Used

| Pattern                      | Where                        | Purpose                                       |
|------------------------------|------------------------------|-----------------------------------------------|
| **Clean Architecture**       | Both services                | Domain-centric, dependency inversion          |
| **CQRS**                     | Both services                | Separate read/write models via MediatR        |
| **Repository Pattern**       | Both services                | Data access abstraction                       |
| **Decorator Pattern**        | PartyService                 | CachedPartyRepository wraps PartyRepository   |
| **Transactional Outbox**     | PartyService                 | Reliable event publishing                     |
| **Publish-Subscribe**        | Cross-service                | RabbitMQ Topic Exchange                       |
| **Database-per-Service**     | Both services                | Each service owns its own database            |
| **Background Service**       | Both services                | OutboxProcessorService, PartyEventConsumer    |
| **Middleware Pipeline**      | Both services                | GlobalExceptionMiddleware                     |
| **MediatR Pipeline Behavior**| Both services                | FluentValidation as a pipeline step           |
| **API Gateway Pattern**      | ApiGateway                   | YARP Reverse Proxy                            |

---

## Database Schema

### PartyServiceDB
- **Parties** — party records (id, name, date, location, description)
- **OutboxMessages** — outbox table for reliable event publishing (id, event_type, event_data, processed, created_at, retry_count, last_error)

### GuestServiceDB
- **Guests** — guest records (id, name, age, party_id, rsvp_status)
- **CachedParties** — local read-model of parties (id, party_external_id, name, date, location, is_active)

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local or Docker)
- Redis (local or Docker)
- RabbitMQ (local or Docker)

---

## Getting Started

### 1. Start infrastructure

```bash
# SQL Server (if using Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Redis
docker run -p 6379:6379 -d redis:latest

# RabbitMQ with management UI
docker run -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin123 -d rabbitmq:3-management
```

### 2. Update connection strings

Edit `appsettings.json` in each project to match your environment (server name, credentials).

### 3. Run the services

```bash
# Terminal 1 — ApiGateway
cd ApiGateway
dotnet run

# Terminal 2 — PartyService
cd PartyService/PartyService.Presentation
dotnet run

# Terminal 3 — GuestService
cd GuestService/GuestService.Presentation
dotnet run
```

### 4. Access

| Service       | URL                          |
|---------------|------------------------------|
| API Gateway   | `http://localhost:5000`      |
| Party Swagger | `http://localhost:5001/swagger` |
| Guest Swagger | `http://localhost:5002/swagger` |
| RabbitMQ UI   | `http://localhost:15672`     |

---

## Testing

The project includes **88 tests** across 4 test projects: unit tests for handlers, validators, and behaviors; integration tests for the full HTTP controller pipeline.

| Test Suite | Type | Tests | Command |
|-----------|------|-------|---------|
| PartyService.Application.Tests | Unit | 31 | `dotnet test PartyService/PartyService.Application.Tests` |
| GuestService.Application.Tests | Unit | 36 | `dotnet test GuestService/GuestService.Application.Tests` |
| PartyService.IntegrationTests | Integration | 10 | `dotnet test PartyService/PartyService.IntegrationTests` |
| GuestService.IntegrationTests | Integration | 11 | `dotnet test GuestService/GuestService.IntegrationTests` |

**Unit tests** (xUnit + Moq): validate handlers, FluentValidation validators, the MediatR validation pipeline behavior, and event handlers in isolation with mocked dependencies.

**Integration tests** (WebApplicationFactory + Moq): spin up the full ASP.NET Core pipeline — controllers, MediatR, FluentValidation, and global exception middleware — testing every endpoint for success, not-found, and validation-failure responses.

Coverage includes:
- All 5 Party endpoints (Create, Read, Update, Delete, List)
- All 5 Guest endpoints (Create, Read, Update, Delete, List)
- PartyCreated/Updated/Deleted event handlers in GuestService
- Happy path, not found (404), validation failure (422), and ID mismatch (400) scenarios

---

## Areas for Improvement

### High Priority

| Area                     | Issue                                               |
|--------------------------|-----------------------------------------------------|
| **docker-compose**       | No single command to spin up the full stack.        |
| **Configuration**        | Database server name and RabbitMQ credentials are hardcoded in `appsettings.json`. Should use environment variables or .NET User Secrets. |
| **Health checks**        | No `/health` endpoints; YARP has no health-check-based routing. |
| **Authentication**       | All APIs are wide open — no JWT, OAuth, or API key protection. |
| **Duplicate consumer**   | Both GuestService and PartyEventConsumerService compete for the same queue. Pick one model. |
| **Secrets in source**    | RabbitMQ credentials (`admin`/`admin123`) committed to config files. |

### Medium Priority

| Area                        | Issue                                             |
|-----------------------------|---------------------------------------------------|
| **Shared event contracts**  | Party event classes are duplicated in both services. Extract to a shared NuGet package or contracts library. |
| **Event versioning**        | Events lack a version field — changes will break consumers silently. |
| **Service discovery**       | API Gateway uses hardcoded upstream addresses.    |
| **Observability**           | No structured logging (Serilog), no OpenTelemetry tracing, no metrics. |
| **CORS**                    | PartyService allows `AllowAnyOrigin`; only ApiGateway has properly scoped CORS. |
| **Redis error handling**    | Catch blocks silently swallow exceptions and return null — could mask connectivity issues. |

---

## Project Structure

```
PartyProject/
├── PartyMicroservices.sln
├── ApiGateway/                          # YARP Reverse Proxy
│   ├── Program.cs
│   ├── ApiGateway.csproj
│   └── appsettings.json
├── PartyService/                        # Party Microservice
│   ├── PartyService.sln
│   ├── PartyService.Presentation/       # Controllers, middleware, startup
│   ├── PartyService.Application/        # CQRS handlers, DTOs, validators, events
│   ├── PartyService.Domain/             # Entities (Party, OutboxMessage)
│   ├── PartyService.Infrastructure/     # Dapper, Redis, RabbitMQ, Outbox processor, SQL scripts
│   ├── PartyService.Application.Tests/  # 31 unit tests (xUnit + Moq)
│   └── PartyService.IntegrationTests/   # 10 integration tests (WebApplicationFactory)
├── GuestService/                        # Guest Microservice
│   ├── GuestService.sln
│   ├── GuestService.Presentation/       # Controllers, middleware, startup
│   ├── GuestService.Application/        # CQRS handlers, event handlers, DTOs
│   ├── GuestService.Domain/             # Entities (Guest, CachedParty)
│   ├── GuestService.Infrastructure/     # EF Core, RabbitMQ consumer, migrations
│   ├── GuestService.Application.Tests/  # 36 unit tests (xUnit + Moq)
│   └── GuestService.IntegrationTests/   # 11 integration tests (WebApplicationFactory)
├── PartyEventConsumerService/           # Standalone RabbitMQ consumer (alternative deployment)
│   ├── Program.cs
│   └── Dockerfile
└── RabbitMQTest/                        # RabbitMQ connectivity test utility
    └── Program.cs
```
