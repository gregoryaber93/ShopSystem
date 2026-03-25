# HANDOVER-PR3

Work only on `docsEng/CHECKLISTA-PR3-grpc-contracts.md`.

## Complete Now
1. Align proto files with current domain DTOs.
2. Add gRPC server endpoints in Product/Promotion/Order services.
3. Add auth + timeout/retry policies.

## Allowed Files
- `ShopService/src/Core/Contracts/Protos/*.proto`
- gRPC API/Infrastructure files in Product, Promotion, Order services.

## Out Of Scope
- Event sourcing and broker implementation.
- Redis/cache changes.

## Validation
- Build all impacted solutions.
- Run contract/integration tests for gRPC calls.

## End Session
- Update `docsEng/CHECKLISTA-PR3-grpc-contracts.md`.
- Update `docsEng/STATUS-PR3.md`.
