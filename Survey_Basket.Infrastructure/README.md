# Survey_Basket.Infrastructure

This project implements persistence and external integrations for Survey Basket.

## Purpose

- Implement repository and unit-of-work interfaces defined by Domain.
- Configure EF Core mappings and migrations for PostgreSQL.
- Register concrete services for DI (auth wiring, email, file storage, Hangfire, caching).

## Key Areas

- `Data/`: `ApplicationDbContext` and entity configurations.
- `Persistence/Repositories/`: repository implementations and query logic.
- `Persistence/UnitOfWork.cs`: transaction boundary for services.
- `Migrations/`: EF Core migration history.
- `Services/`: infrastructure services (email sender, file answer storage, etc.).
- `DependencyInjection.cs`: central registration of infrastructure services.

## Implementation Notes

- Uses ASP.NET Identity with EF Core store.
- Uses PostgreSQL for application data and Hangfire storage.
- Applies model configurations from assembly and enforces restricted delete behaviors.
- Automatically stamps audit fields (`CreatedById`, `UpdatedById`, timestamps) through `ApplicationDbContext.SaveChangesAsync`.
- Registers JWT auth, custom authorization policy components, hybrid cache, and background workers.

## Operational Considerations

- Keep migrations in sync with domain model changes.
- If runtime errors reference missing relations/tables, apply latest EF migrations.
- Running API process can lock DLLs during build/test; stop API before full test runs.
