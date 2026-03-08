# CHECKLIST PR5 - Event Sourcing

## Goal
Implement event sourcing where it gives real business value.

## Domain Scope
- [ ] `OrderService` aggregate.
- [ ] Loyalty ledger for user-specific promotions.

## Implementation Tasks
- [ ] Define event stream model and event versioning.
- [ ] Add optimistic concurrency on aggregate version.
- [ ] Add snapshot strategy.
- [ ] Add replay and aggregate rehydration.
- [ ] Add read projections for query APIs.

## Tests
- [ ] Aggregate rebuild from event stream restores correct state.
- [ ] Version conflict is detected.
- [ ] Snapshot restore is consistent with full replay.

## Acceptance Criteria
- [ ] ES is stable for selected aggregates.
- [ ] Projections are consistent and event-driven.
