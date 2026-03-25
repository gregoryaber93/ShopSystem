# CHECKLIST PR7 - RabbitMQ + Kafka (responsibility split)

## Goal
Implement both brokers in one system, with a clear split of responsibilities:
- RabbitMQ: workflow/integration commands (short response time, retry, DLQ).
- Kafka: domain event stream (retention, replay, analytics, independent consumer groups).

## Scope
- Transactional services: AuthService, UserService, OrderService, PaymantService, PromotionService.
- Read/observability services: DashboardService, LoggerService.

## Stage 0 - Baseline and configuration
- [x] Confirm the broker stack is running: `docker compose -f docker-compose.brokers.yml up -d`.
- [ ] Standardize channel naming:
  - Rabbit exchange per service, routing key per workflow.
  - Kafka topic: `<prefix>.<eventname>.v1`.
- [x] For OrderService and PaymantService, add missing broker env vars in docker-compose:
  - `MessageBrokers__RabbitMq__Host/Port/Username/Password/Exchange/DeadLetterExchange`
  - `MessageBrokers__Kafka__BootstrapServers/TopicPrefix`
- [x] Add the event routing document `docs/PR7-event-routing-matrix.md` (per Stage 1).

## Stage 1 - Event Routing Matrix (design decision)
For each event, define the target channel and partition key.

Minimum matrix:
- `UserCreated.v1`
  - RabbitMQ: YES (Auth -> User provisioning)
  - Kafka: OPTIONAL (audit stream)
  - PartitionKey: `UserId`
- `OrderPlaced.v1`
  - RabbitMQ: YES (workflow: downstream reactions)
  - Kafka: YES (analytics/replay)
  - PartitionKey: `UserId` or `OrderId`
- `PaymentAuthorized.v1`
  - RabbitMQ: YES (workflow update of order status)
  - Kafka: YES (audit, BI)
  - PartitionKey: `OrderId`
- `PaymentFailed.v1`
  - RabbitMQ: YES (compensation workflow)
  - Kafka: YES (audit, alerting)
  - PartitionKey: `OrderId`
- `PointsEarned.v1`, `PointsSpent.v1`, `LoyaltyProfileUpdated.v1`
  - RabbitMQ: optional (if immediate workflow action is required)
  - Kafka: YES (ledger/replay/analytics)
  - PartitionKey: `UserId`

## Stage 2 - RabbitMQ as workflow channel
### Auth + User (already partially done)
- [x] Keep the current `UserCreated` flow with Auth outbox and User consumer.
- [ ] Add a retry policy and DLQ validated by failure scenarios.

### Payment -> Order (new implementation)
- [x] Add Rabbit consumer worker in OrderService:
  - `OrderService/src/Infrastructure/Messaging/PaymentAuthorizedConsumerWorker.cs`
- [x] Handle events:
  - `PaymentAuthorized` -> set order status to `Paid`.
  - `PaymentFailed` -> set order status to `PaymentFailed` or trigger compensation.
- [x] Add idempotent consumer table check by `EventId`.
- [x] Add explicit ack/nack + requeue policy.

### Order -> Promotion (optional workflow)
- [ ] If you need immediate loyalty profile updates:
  - add a Rabbit consumer in PromotionService for `OrderPlaced`.
- [ ] Profile update must be idempotent and based on `EventId`.

## Stage 3 - Kafka as event stream channel
### Publisher side
- [x] Keep Kafka publishing from outbox in OrderService and PaymantService.
- [x] Ensure message key = `PartitionKey`.
- [x] Add metadata headers:
  - `eventType`, `eventVersion`, `occurredOnUtc`, `correlationId`.

### Consumer side (NEW)
- [x] DashboardService: create Kafka consumer(s) for read model projections:
  - `orders_projection`
  - `payments_projection`
  - `loyalty_projection`
- [x] LoggerService: add an optional Kafka consumer for centralized domain-event auditing.
- [x] Use separate `group.id` for each bounded context (dashboard, logger, analytics).
- [x] Add offset checkpointing and restart/recovery mechanism.

## Stage 4 - Clarify Rabbit vs Kafka boundary
Recommended educational variant in 2 steps:

1) Phase A (easier): dual publish for selected events
- Rabbit for workflow.
- Kafka for streaming and analytics.

2) Phase B (target): intentional split
- Strict workflow events only on Rabbit.
- Strict domain events only on Kafka.
- Hybrid events (for example `OrderPlaced`) may stay on both, but only with business justification.

## Stage 5 - Mandatory tests
### RabbitMQ
- [ ] Retry and DLQ: disable consumer, send event, confirm move to DLQ.
- [x] Idempotency: send the same `EventId` 2x, business effect happens only once.
- [ ] Recovery: after worker restart, pending messages are still processed.

### Kafka
- [ ] Ordering in partition: events for one `OrderId/UserId` arrive in correct order.
- [ ] Replay: rebuild DashboardService projections from offset 0.
- [ ] Consumer group isolation: dashboard and logger consume independently.

### End-to-end
- [ ] `OrderPlaced` -> `PaymentAuthorized` -> order status update.
- [ ] Loyalty update after order and/or payment events.
- [ ] No duplicate side effects despite message redelivery.

## Stage 6 - Observability and operations
- [ ] Add metrics:
  - outbox lag (time from `CreatedAt` to publish),
  - Kafka consumer lag,
  - Rabbit retry count,
  - DLQ message count.
- [ ] Add structured logging with `EventId`, `CorrelationId`, `EventType`.
- [ ] Prepare a broker failure runbook (restart, replay, DLQ draining).

## PR7 acceptance criteria
- [x] For each event, there is an explicit Rabbit/Kafka decision in the matrix.
- [ ] Critical workflow runs through Rabbit with retry + DLQ + idempotency.
- [ ] Domain stream runs through Kafka and can be replayed.
- [x] Dashboard/Logger can consume Kafka independently from workflow.
- [ ] Failure and replay tests pass locally and in CI.

## Suggested implementation order (2 sprints)
Sprint 1:
- Stage 0, 1, 2 (Auth/User stabilization + Payment->Order workflow),
- Rabbit tests.

Sprint 2:
- Stage 3, 4, 6,
- Kafka consumers in Dashboard/Logger,
- replay and end-to-end tests.
