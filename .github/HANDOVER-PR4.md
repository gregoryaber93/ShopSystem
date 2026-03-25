# HANDOVER-PR4

Work only on `docsEng/CHECKLISTA-PR4-outbox-brokers.md`.

## Complete Now
1. Add outbox in Order and Payment services.
2. Implement outbox publisher worker.
3. Replace placeholder Rabbit/Kafka publishers.

## Allowed Files
- OrderService outbox files.
- PaymantService outbox files.
- `ShopService/src/Infrastructure/Messaging/*.cs`

## Out Of Scope
- Event sourcing aggregate rehydration.
- Read-side projections and Redis caching.

## Validation
- Build impacted solutions.
- Test broker retry/DLQ and idempotency scenarios.

## End Session
- Update `docsEng/CHECKLISTA-PR4-outbox-brokers.md`.
- Update `docsEng/STATUS-PR4.md`.
