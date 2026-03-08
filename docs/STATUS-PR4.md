# STATUS-PR4

## Objective
Implement reliable outbox-based event publishing and broker delivery.

## Active Checklist
- `docsEng/CHECKLISTA-PR4-outbox-brokers.md`

## Session Scope
- `OrderService`
- `PaymantService`
- messaging publish paths

## Key Files
- outbox persistence and worker files in Order/Payment services
- `ShopService/src/Infrastructure/Messaging/RabbitMqEventPublisher.cs`
- `ShopService/src/Infrastructure/Messaging/KafkaEventPublisher.cs`

## Mandatory Outcomes
- Outbox pattern active in transactional services.
- Placeholder publishers replaced by real broker publishing.
- Idempotent consume strategy by `EventId`.

## Next Step
Implement outbox persistence first, then worker publishing.
