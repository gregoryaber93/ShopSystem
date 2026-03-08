# PR4-PR5 - Event Driven, Outbox, Event Sourcing

## Scope
- Implement reliable event delivery and selective event sourcing.
- Split responsibilities between RabbitMQ and Kafka.

## Architecture Decisions
- RabbitMQ:
  - workflow/integration commands,
  - retry + DLQ.
- Kafka:
  - domain event stream,
  - replay and analytics.
- Event sourcing:
  - `OrderService`,
  - loyalty ledger (user/promotions).
- Do not use event sourcing for simple catalog CRUD.

## Required Changes

### Outbox
- Add outbox tables per transactional service (`OrderService`, `PaymantService`).
- Publish events after DB transaction commit.
- Add idempotent consumer by `EventId`.

### Publishers
- `ShopService/src/Infrastructure/Messaging/RabbitMqEventPublisher.cs`
- `ShopService/src/Infrastructure/Messaging/KafkaEventPublisher.cs`
- Replace placeholders with real publishing.

### Event contracts
- Add versioned events:
  - `OrderPlaced`,
  - `PromotionApplied`,
  - `PaymentAuthorized`,
  - `PaymentFailed`,
  - `PointsEarned`,
  - `PointsSpent`.

### Event Store
- For ES aggregates add:
  - optimistic concurrency (version),
  - snapshots,
  - replay.

## Tests
- Double-publish test (idempotency).
- Broker failure + retry test.
- Replay test to restore aggregate state correctly.

## DoD
- No event loss.
- Events are idempotent and versioned.
- ES works for selected domains.
