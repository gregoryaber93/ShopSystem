# PR7 - Event Routing Matrix

## How to use this document
- One row = one versioned event.
- `RabbitMQ` means workflow channel (fast reaction, retry, DLQ).
- `Kafka` means stream/replay/analytics channel.
- `PartitionKey` must preserve ordering semantics for an aggregate.

## Matrix

| Event | Producer | RabbitMQ | Kafka | PartitionKey | Consumers (workflow) | Consumers (stream) | Notes |
|---|---|---|---|---|---|---|---|
| UserCreated.v1 | AuthService | YES | OPTIONAL | UserId | UserService | Dashboard/Logger (optional) | Profile provisioning |
| OrderPlaced.v1 | OrderService | YES | YES | OrderId or UserId | PromotionService (optional), Payment orchestration | DashboardService, LoggerService | Hybrid event |
| PaymentAuthorized.v1 | PaymantService | YES | YES | OrderId | OrderService | DashboardService, LoggerService | Order status update |
| PaymentFailed.v1 | PaymantService | YES | YES | OrderId | OrderService | DashboardService, LoggerService | Compensation workflow |
| PointsEarned.v1 | PromotionService | OPTIONAL | YES | UserId | (as needed) | DashboardService, LoggerService | Loyalty ledger |
| PointsSpent.v1 | PromotionService | OPTIONAL | YES | UserId | (as needed) | DashboardService, LoggerService | Loyalty ledger |
| LoyaltyProfileUpdated.v1 | PromotionService | OPTIONAL | YES | UserId | (as needed) | DashboardService, LoggerService | Read model + analytics |

## Technical conventions
- Rabbit exchange: `<ServiceName>.exchange`
- Rabbit routing key: `<domain>.<event>.v1` or keep current `<eventType>.ToLowerInvariant()` (consistency required)
- Kafka topic: `<prefix>.<eventname>.v1`
- Headers minimum:
  - `eventType`
  - `eventVersion`
  - `occurredOnUtc`
  - `correlationId`
  - `causationId` (optional)

## Decisions to approve
- [x] `OrderPlaced.v1` zostaje eventem hybrydowym (Rabbit + Kafka).
- [x] `OrderPlaced.v1` remains a hybrid event (Rabbit + Kafka).
- [x] `UserCreated.v1` does not go to Kafka for now (Rabbit workflow provisioning only).
- [x] Loyalty events remain Kafka-first; Rabbit only optional when immediate workflow is required.

## Verified Stage 7 flows
- RabbitMQ workflow:
  - `PaymantService` -> `OrderService` (`PaymentAuthorized.v1`, `PaymentFailed.v1`) with `ack/nack`, `requeue`, and idempotency by `EventId`.
  - `AuthService` -> `UserService` (`UserCreated.v1`) as workflow provisioning.
- Kafka stream/analytics:
  - Publishers `OrderService` and `PaymantService` publish with `PartitionKey` key and headers (`eventType`, `eventVersion`, `occurredOnUtc`, `correlationId`).
  - `DashboardService` consumes into projections: `orders_projection`, `payments_projection`, `loyalty_projection`.
  - `LoggerService` consumes events for central audit.
  - `group.id` split: `dashboard-projections-v1`, `logger-audit-v1`.

## Review checklist
- [ ] Each event has an explicitly selected channel and rationale.
- [ ] Each event has a defined partition key.
- [ ] Workflow consumer has idempotency by EventId.
- [ ] Stream consumer has replay strategy (offset reset/checkpointing).
