# STATUS-PR3

## Objective
Deliver real gRPC server-to-server communication and aligned contracts.

## Active Checklist
- `docsEng/CHECKLISTA-PR3-grpc-contracts.md`

## Session Scope
- `ProductService`
- `PromotionService`
- `OrderService`
- shared proto contracts in `ShopService`

## Key Files
- `ShopService/src/Core/Contracts/Protos/products.proto`
- `ShopService/src/Core/Contracts/Protos/promotions.proto`
- `ShopService/src/Core/Contracts/Protos/orders.proto`
- gRPC registration and service mapping files in API/Infrastructure layers

## Mandatory Outcomes
- Running gRPC endpoints for selected services.
- Proto contracts aligned with domain DTOs.
- Inter-service auth/retry policy in place.

## Next Step
Align proto definitions with domain fields before implementing server endpoints.
