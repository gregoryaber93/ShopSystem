# CHECKLIST PR4 - Outbox and Brokers

## Goal
Implement reliable event publishing and real broker flows.

## Implementation Tasks
- [ ] Add Outbox table in `OrderService`.
- [ ] Add Outbox table in `PaymantService`.
- [ ] Implement worker that publishes from outbox.
- [ ] Replace placeholder `RabbitMqEventPublisher` with real publishing.
- [ ] Replace placeholder `KafkaEventPublisher` with real publishing.
- [ ] Add DLQ/retry strategy for RabbitMQ.
- [ ] Add topic naming and partition key policy for Kafka.
- [ ] Add idempotent consumer by `EventId`.

## Tests
- [ ] Event is not lost during broker failure.
- [ ] Event is not applied twice (idempotency).
- [ ] Outbox replay works after restart.

## Acceptance Criteria
- [ ] Event publishing is reliable.
- [ ] Retry + DLQ are configured and tested.
