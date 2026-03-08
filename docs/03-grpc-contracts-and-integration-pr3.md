# PR3 - gRPC Contracts and Integration

## Scope
- Move from gRPC client-only to real service-to-service communication.
- Align `.proto` contracts with domain model.

## Current Starting Point
- Protos:
  - `ShopService/src/Core/Contracts/Protos/products.proto`
  - `ShopService/src/Core/Contracts/Protos/promotions.proto`
  - `ShopService/src/Core/Contracts/Protos/orders.proto`
- Clients are registered in:
  - `ShopService/src/Infrastructure/DependencyInjection.cs`

## Required Changes
1. Add gRPC server-side endpoints in:
- `ProductService`
- `PromotionService`
- `OrderService`

2. Align contracts:
- `products.proto`: currently has `sku`, while domain has `Type/Price/ShopId`.
- `promotions.proto`: align with actual promotion fields.
- `orders.proto`: extend with fields needed for orchestration.

3. Add:
- service-to-service auth (JWT),
- timeout and retry policies,
- gRPC error mapping to ProblemDetails.

## Tests
- Contract tests for proto and DTO compatibility.
- Integration tests for gRPC calls between services.
- Inter-service authorization tests.

## DoD
- Working gRPC client + server.
- No mismatch between contracts and domain model.
