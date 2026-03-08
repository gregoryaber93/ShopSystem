# CHECKLIST PR1 - Security and Roles

## Goal
Remove unauthorized role escalation and close manager creation flow to admin only.

## Implementation Tasks
- [ ] `AuthenticationService`: limit public register to `User` role.
- [ ] `AuthenticationService`: remove/ignore `Roles` in public register DTO.
- [ ] `UserService`: limit `AllowedRoles` to `Manager`, `User`.
- [ ] `UserService`: return clear error when trying to create `Admin`.
- [ ] `UserService`: keep `[Authorize(Roles = "Admin")]` for create user.

## Tests
- [ ] Register with `Admin` returns 400/403.
- [ ] Register with `Manager` returns 400/403.
- [ ] Register without role creates `User`.
- [ ] Admin creates manager via `UserService`.
- [ ] Admin cannot create `Admin` via `UserService`.

## Acceptance Criteria
- [ ] No role escalation via public endpoints.
- [ ] Unit and integration tests pass.
