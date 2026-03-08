# HANDOVER-PR6

Work only on `docsEng/CHECKLISTA-PR6-redis-logger-observability.md`.

## Complete Now
1. Add CorrelationId + exception logging middleware in all APIs.
2. Integrate LoggerService client with fallback.
3. Add Redis cache to selected read paths.

## Allowed Files
- API middleware and DI files across services.
- LoggerService storage implementation files.
- Cache integration files in Infrastructure/Application.

## Out Of Scope
- Security role redesign.
- Event sourcing and broker architecture work.

## Validation
- Build impacted solutions.
- Run endpoint tests with correlation/log assertions.
- Validate cache hit/miss behavior.

## End Session
- Update `docsEng/CHECKLISTA-PR6-redis-logger-observability.md`.
- Update `docsEng/STATUS-PR6.md`.
