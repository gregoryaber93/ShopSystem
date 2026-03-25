# PR6 - Redis, LoggerService, Observability

## Scope
- Add Redis cache and central endpoint/problem logging.
- Stop relying on in-memory logger as the only storage.

## Redis
1. Use cases:
- product cache per shop,
- active promotions cache,
- user eligibility cache.
2. Additionally:
- idempotency keys for `POST /orders` and `POST /payments`,
- distributed locks for critical promotion/points logic.

## LoggerService integration
- Target endpoint:
  - `POST /api/logs` (`LoggerService/src/API/Controllers/LogsController.cs`)
- In each service:
  - request/response logging middleware,
  - exception/problem logging middleware,
  - `X-Correlation-Id` propagation,
  - fallback to local `ILogger` when logger is unavailable.

## Log persistence
- Current:
  - `LoggerService/src/Infrastructure/Logging/InMemoryLogStore.cs`
- Change:
  - move to DB (e.g., PostgreSQL) or log search engine.
  - indexes by `CorrelationId`, `Source`, `CreatedAtUtc`.

## Metrics and tracing
- OpenTelemetry for:
  - traces,
  - metrics,
  - log correlation.

## DoD
- 100% of endpoints log requests and errors with `CorrelationId`.
- Logs are persistent and filterable.
- Cache has measurable latency impact.
