# PR1 - Security and Roles

## Scope
- Block public account creation with `Admin` and `Manager` roles in `AuthenticationService`.
- Keep manager creation available only for admin in `UserService`.
- Standardize role validation and API responses.

## Required Changes

### AuthenticationService
- File: `AuthenticationService/src/Core/Application/Features/Authentication/Commands/Register/RegisterCommandHandler.cs`
- Change:
  - Allow only `User` role for public register.
  - Alternatively ignore roles from request and always assign `User`.
- File: `AuthenticationService/src/Core/Contracts/Dtos/RegisterRequestDto.cs`
- Change:
  - Consider removing `Roles` from the public register DTO (breaking API change).

### UserService
- File: `UserService/src/Core/Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`
- Change:
  - Limit `AllowedRoles` to `Manager`, `User`.
  - Reject attempts to create `Admin`.
- File: `UserService/src/API/Controllers/UsersController.cs`
- Change:
  - Keep `[Authorize(Roles = "Admin")]`.
  - Keep error mapping to 400/403/409.

## Tests
- Test: anonymous register with `Admin` => 400.
- Test: anonymous register with `Manager` => 400.
- Test: anonymous register with no role => creates `User`.
- Test: admin creates manager through `UserService` => 200.
- Test: admin attempts to create admin through `UserService` => 400.

## DoD
- No possibility to create `Admin/Manager` outside admin-only flow.
- All role-based tests pass.
