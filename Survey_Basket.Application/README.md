# Survey_Basket.Application

This project contains the business logic and use-case orchestration for Survey Basket.

## Purpose

- Define business contracts (requests/responses).
- Implement business services for auth, poll lifecycle, voting, analytics, roles, and company operations.
- Centralize validation, domain errors, and permission abstractions.

## Architecture Role

- Sits between API and Infrastructure.
- Knows domain concepts and rules, but not storage details.
- Depends on domain abstractions (`IUnitOfWork`, repository interfaces), not EF Core implementations.

## Key Areas

- `Services/`: use-case services (`AuthService`, `PollService`, `VoteService`, `ResultService`, `UserServices`, `RoleService`).
- `Contracts/`: DTOs for each capability.
- `Errors/`: typed error catalog and global exception handling components.
- `Abstractions/`: result wrappers, constants, pagination helpers.
- `Services/AuthServices/Filter/`: permission policy provider/handler and authorization attributes.
- `Extensions/` and `Helpers/`: utility helpers used by multiple flows.

## Implementation Style

- Services return `Result` / `Result<T>` instead of throwing for expected business failures.
- Validation is done with FluentValidation validators per request contract.
- Permission checks are role/claim based and reused across endpoints.
- Business workflows compose multiple repositories through `IUnitOfWork`.

## Example Business Rules Implemented Here

- Company first-login and password setup rules.
- Poll audience targeting by selected companies.
- Poll access token redemption for QR/link onboarding.
- One-response-per-identity protection for votes.
- Poll analytics authorization and aggregation orchestration.
