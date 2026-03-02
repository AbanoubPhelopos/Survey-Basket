# Survey_Basket.Api

This project is the HTTP entry point of Survey Basket.

## Purpose

- Expose REST endpoints for auth, polls, questions, votes, results, roles, and company flows.
- Apply authentication/authorization at endpoint level.
- Host operational middleware like Swagger, Serilog request logging, and Hangfire dashboard.

## How It Is Implemented

- `Program.cs` wires all runtime services by calling `AddInfrastructure(...)`.
- Controllers are thin: they validate route/body shape, call application services, and return standardized problem details/results.
- Permission checks use custom attributes/policies (`HasPermission`) from the application layer.
- JWT bearer is the default auth scheme.
- Swagger is enabled in development with bearer token support.
- Hangfire dashboard and recurring notification registration are enabled outside testing.

## Key Folders

- `Controllers/`: API endpoints grouped by business capability.
- `Models/`: API-specific transport wrappers where needed.
- `Program.cs`: bootstrapping and middleware pipeline.

## Request Pipeline (High Level)

1. Incoming HTTP request
2. CORS + authentication
3. Authorization policy/permission check
4. Controller action invokes application service
5. Service returns `Result` / data
6. Controller returns HTTP response (success or problem details)

## Design Notes

- Business rules are intentionally not in controllers.
- Controllers depend on interfaces/services from `Survey_Basket.Application`.
- Data persistence and external integrations are delegated to `Survey_Basket.Infrastructure`.
