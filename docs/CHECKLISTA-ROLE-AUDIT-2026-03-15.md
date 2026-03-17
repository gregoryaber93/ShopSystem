# CHECKLIST - Role and Feature Audit (2026-03-15)

## Audit scope
- Admin: add, edit, and delete manager.
- Manager: add shop, manage products, manage promotions, dashboard.
- User: browse products and promotions, cart checkout with promotion calculation, purchase history.

## Implementation status

### 1) Admin
- [x] Add manager: implemented (`UserService` -> `POST /api/users/create`, `Admin` only).
- [x] Edit manager: implemented (`UserService` -> `PUT /api/users/{id}` + identity sync to `AuthService`).
- [x] Delete manager: implemented (`UserService` -> `DELETE /api/users/{id}` + identity deletion in `AuthService`).

### 2) Manager
- [x] Add new shop: implemented (`ShopService` -> `AddShop` allows `Admin,Manager`).
- [x] Add products to shop: implemented (`ProductService`, `Manager` role).
- [x] Define promotions: implemented (`PromotionService`, `Manager` role).
- [x] Dashboard access: implemented (JWT + `Admin,Manager` authorization in `DashboardService`).

### 3) User
- [x] Browse products: implemented (`ProductService`, GET by `shopId`, `User` role allowed).
- [x] Browse promotions: implemented (`PromotionService`, promotion GET endpoints, `User` role allowed).
- [x] Create order from cart and apply promotions: implemented (`OrderService` checkout + `PromotionService` integration).
- [x] Purchase history: implemented (`OrderService` -> `GET /api/orders/my`).

## Implementation instructions (gaps)

## P0 - Critical
- [x] DashboardService: add JWT authentication and authorization.
  - File: `DashboardService/src/API/Program.cs`
  - Tasks:
    - add `AddAuthentication(...)` and `AddAuthorization(...)`,
    - add `UseAuthentication()` and `UseAuthorization()`.
  - Acceptance criteria:
    - anonymous: `GET /api/dashboard` -> `401`,
    - manager/admin: `GET /api/dashboard` -> `200`,
    - user: `GET /api/dashboard` -> `403`.

- [x] DashboardService: restrict dashboard endpoints to `Admin,Manager`.
  - File: `DashboardService/src/API/Controllers/DashboardController.cs`
  - Tasks:
    - add `[Authorize(Roles = "Admin,Manager")]` on controller or endpoints.

- [x] ShopService: unlock shop creation for manager.
  - File: `ShopService/src/API/Controllers/ShopsController.cs`
  - Tasks:
    - change `AddShop` from `[Authorize(Roles = "Admin")]` to `[Authorize(Roles = "Admin,Manager")]`.
  - Acceptance criteria:
    - manager can create shop,
    - user cannot create shop.

## P1 - High
- [x] UserService: add manager edit endpoint.
  - File: `UserService/src/API/Controllers/UsersController.cs`
  - Tasks:
    - add `PUT /api/users/{id}` (`Admin` only),
    - add command/handler for manager data updates.

- [ ] UserService + AuthService: add manager deletion endpoint as public admin flow.
  - Files:
    - `UserService/src/API/Controllers/UsersController.cs`
    - `AuthService/src/API/Controllers/InternalAuthenticationController.cs` (reuse integration)
  - Tasks:
    - [x] add `DELETE /api/users/{id}` (`Admin` only) in `UserService`,
    - [x] in deletion flow, delete identity in `AuthService` + delete profile in `UserService`,
    - [ ] ensure no orphan records on partial failure (compensation/retry).

## P2 - Best practices / hardening
- [x] ProductService: ownership for manager add/update/delete product operations.
  - `ShopService`: new endpoint `GET /api/shops/internal/{shopId}/owner` (`InternalShopsController`).
  - `ProductService`: `IShopOwnershipClient` -> `HttpShopOwnershipClient` (HTTP call to ShopService).
  - `ProductService`: `ICurrentUserService` -> `HttpContextCurrentUserService`.
  - Handlers `AddProduct`, `UpdateProduct`, `DeleteProduct`: Manager can operate only on products of owned shops (`OwnerUserId == currentUserId`).
  - Controller: `UnauthorizedAccessException` -> `403`.
  - Configuration: `ShopService:BaseUrl` section in `appsettings.json`.
- [x] PromotionService: ownership for manager add/update/delete promotion operations.
  - `PromotionEntity`: new field `Guid? CreatedByUserId`.
  - `ICurrentUserService` -> `HttpContextCurrentUserService`.
  - Handlers `AddPromotion` (sets `CreatedByUserId`), `UpdatePromotion`, `DeletePromotion`: Manager can operate only on own promotions.
  - Migration: `AddPromotionCreatedByUserId`.
  - Controller: `UnauthorizedAccessException` -> `403`.
- [x] ShopService: ownership for manager add/update/delete shop operations (field `OwnerUserId` + validation in command handlers + migration `AddShopOwnerUserId`).

## Proposed implementation order
1. P0 Dashboard authn/authz.
2. P0 Manager add shop.
3. P1 Admin edit/delete manager.
4. P2 Ownership checks.

## Notes
- Promotion calculation in checkout is done backend-side in `OrderService` by calling `PromotionService` and validating the cart.
- User purchase history is available through `GET /api/orders/my`.