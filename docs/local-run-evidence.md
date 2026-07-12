# Local Run Evidence

## Purpose

This document records a real local execution of the complete Banking API
laboratory. It is not a claim about production readiness. Its purpose is to
show that a student can reproduce the local environment, exercise the main
vertical flow, and inspect the supporting infrastructure.

## Verified on 2026-07-12

Environment:

- Windows PowerShell
- .NET SDK `8.0.421`
- Docker Compose v5.1.4
- Project revision `1f3f9a8644b13d662296ffebe6e4074392381472`

The default RabbitMQ ports were already used by another local project. The
stack was therefore started with isolated host ports. Internal Docker network
ports remain unchanged.

```powershell
$env:API_PORT='15000'
$env:POSTGRES_PORT='15432'
$env:REDIS_PORT='16379'
$env:RABBITMQ_PORT='15673'
$env:RABBITMQ_MANAGEMENT_PORT='15674'
docker compose up --build -d
```

Running services after startup:

| Service | Host endpoint | Result |
|---|---|---|
| API | `http://localhost:15000` | Running |
| PostgreSQL | `localhost:15432` | Running |
| Redis | `localhost:16379` | Running |
| RabbitMQ AMQP | `localhost:15673` | Running |
| RabbitMQ management | `http://localhost:15674` | Running |
| Outbox worker | Docker service `worker` | Running |

## Health and API surface

The following endpoints returned HTTP `200`:

```powershell
Invoke-WebRequest http://localhost:15000/health/live
Invoke-WebRequest http://localhost:15000/swagger/v1/swagger.json
```

## Executed business flow

A fresh user was registered through the API, then two accounts were opened for
that user. The following workflow completed successfully:

1. Deposit `100.00` into the source account with an `Idempotency-Key`.
2. Repeat the identical deposit request with the same key.
3. Transfer `40.00` from the source account to the destination account.
4. Retrieve the source account statement.

Observed results:

| Check | Observed value |
|---|---:|
| First deposit amount | `100.00` |
| Balance after deposit | `100.00` |
| Replayed deposit account and balance | Identical to first response |
| Transfer status | `Completed` |
| Transfer amount | `40.00` |
| Source statement entries | `2` |
| Final source balance | `60.00` |

The resulting outbox query showed two processed integration messages, one for
the deposit and one for the transfer:

```sql
SELECT status, count(*)
FROM outbox_messages
GROUP BY status
ORDER BY status;

-- Processed | 2
```

## Code quality verification

The following checks were run after the local startup preparation:

| Check | Result |
|---|---|
| `dotnet build --configuration Release` | Passed, 0 warnings and 0 errors |
| `dotnet format --verify-no-changes --no-restore` | Passed |
| Unit tests | 22 passed |
| Architecture tests | 4 passed |
| Integration tests | 22 passed |

## Startup readiness

The initial verification revealed that the Worker could try RabbitMQ before the
broker accepted AMQP connections. The Compose configuration now defines health
checks for PostgreSQL, Redis, and RabbitMQ, and waits for the required services
to become healthy before starting API and Worker. This makes local startup
deterministic while retaining MassTransit reconnection as a runtime safeguard.

## Re-run checklist

```powershell
dotnet restore
dotnet build
dotnet format --verify-no-changes
dotnet test tests/Banking.UnitTests/Banking.UnitTests.csproj
dotnet test tests/Banking.ArchitectureTests/Banking.ArchitectureTests.csproj
dotnet test tests/Banking.IntegrationTests/Banking.IntegrationTests.csproj
```

Use `docker compose ps` to verify all five services. Stop the laboratory when
finished with `docker compose down`; this preserves the PostgreSQL named volume.
