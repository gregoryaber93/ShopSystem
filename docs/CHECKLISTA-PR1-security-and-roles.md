# CHECKLIST PR1 - Security and Roles

## Goal
Remove unauthorized role escalation and close manager creation flow to admin only.

## Implementation Tasks
- [x] `AuthenticationService`: limit public register to `User` role.
- [x] `AuthenticationService`: remove/ignore `Roles` in public register DTO.
- [x] `UserService`: limit `AllowedRoles` to `Manager`, `User`.
- [x] `UserService`: return clear error when trying to create `Admin`.
- [x] `UserService`: keep `[Authorize(Roles = "Admin")]` for create user.

## Tests
- [ ] Register with `Admin` returns 400/403.
- [ ] Register with `Manager` returns 400/403.
- [ ] Register without role creates `User`.
- [ ] Admin creates manager via `UserService`.
- [ ] Admin cannot create `Admin` via `UserService`.

## Acceptance Criteria
- [ ] No role escalation via public endpoints.
- [ ] Unit and integration tests pass.
