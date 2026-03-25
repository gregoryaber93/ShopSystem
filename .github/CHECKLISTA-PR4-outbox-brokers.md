# CHECKLIST PR4 - Outbox and Brokers

## Goal
Implement reliable event publishing and real broker flows.

## Implementation Tasks
- [x] Add Outbox table in `OrderService`.
- [x] Add Outbox table in `PaymantService`.
- [x] Implement worker that publishes from outbox.
- [x] Replace placeholder `RabbitMqEventPublisher` with real publishing.
- [x] Replace placeholder `KafkaEventPublisher` with real publishing.
- [x] Add DLQ/retry strategy for RabbitMQ.
- [x] Add topic naming and partition key policy for Kafka.
- [x] Add idempotent consumer by `EventId`.

## Tests
- [ ] Event is not lost during broker failure.
- [ ] Event is not applied twice (idempotency).
- [ ] Outbox replay works after restart.

## Acceptance Criteria
- [ ] Event publishing is reliable.
- [ ] Retry + DLQ are configured and tested.
