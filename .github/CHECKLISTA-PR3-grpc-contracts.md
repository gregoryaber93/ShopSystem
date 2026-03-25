# CHECKLIST PR3 - gRPC Contracts and Integration

## Goal
Enable real gRPC communication between services and align contracts.

## Implementation Tasks
- [x] Add gRPC server endpoints in `ProductService`.
- [x] Add gRPC server endpoints in `PromotionService`.
- [x] Add gRPC server endpoints in `OrderService`.
- [x] Align `products.proto` with domain model.
- [x] Align `promotions.proto` with domain model.
- [x] Align `orders.proto` with domain model.
- [x] Add service-to-service JWT auth.
- [x] Add timeout/retry/circuit policies.

## Tests
- [ ] Contract tests for proto vs DTO.
- [ ] Integration tests for gRPC call chain.
- [ ] Inter-service auth tests.

## Acceptance Criteria
- [ ] gRPC works end-to-end.
- [ ] No contract/domain mismatch.
