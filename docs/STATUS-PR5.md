# STATUS-PR5

## Objective
Implement selective event sourcing for Order and Loyalty domains.

## Active Checklist
- `docs/CHECKLISTA-PR5-event-sourcing.md`

## Session Scope
- `OrderService` aggregate event stream
- loyalty ledger event stream

## Key Files
- aggregate models and versioning logic
- event store and snapshot handling files
- projection handlers

## Mandatory Outcomes
- Aggregate rebuild from event stream.
- Optimistic concurrency and snapshots.
- Stable read projections fed from events.

## Next Step
Run replay/snapshot/version-conflict tests and verify projection consistency under concurrent writes.
