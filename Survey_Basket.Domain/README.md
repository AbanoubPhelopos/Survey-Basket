# Survey_Basket.Domain

This project defines the core domain model and repository abstractions.

## Purpose

- Represent business entities and relationships.
- Provide framework-agnostic contracts for data access.
- Keep core business language stable across API/infrastructure changes.

## Key Contents

- `Entities/`: domain entities such as `Poll`, `Question`, `Vote`, `Company`, `CompanyUser`, `PollAudience`, `PollOwner`, and identity entities.
- `Abstractions/` and `Abstractions/Repositories/`: repository interfaces and `IUnitOfWork` contract.
- `Models/`: domain-level result projections used by repositories/services.

## Entity Model Themes

- Poll ownership and audience targeting are explicit (`PollOwner`, `PollAudience`).
- Voting is normalized across `Vote` and `VoteAnswers` to support multiple question types.
- Company-centric onboarding/access entities support magic links and poll QR links.
- Auditable entities track creator/updater metadata.

## Dependency Rule

- Domain should not depend on API/Application/Infrastructure projects.
- Other layers are allowed to depend on Domain.
