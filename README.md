# Survey Basket

Survey Basket is a multi-tenant polling platform for organizations that want to run controlled surveys across selected companies and collect trusted responses.

## Business Purpose

Survey Basket solves three practical business needs:

- Run surveys for one company, multiple companies, or all companies.
- Control who can answer using secure company-generated links and QR codes.
- Produce reliable analytics while preventing repeated submissions from the same participant identity.

In short: admins create and target polls, companies distribute secure access, and participants answer once with traceable identity rules.

## Core Business Flows

### 1) Company onboarding

- Admin provisions a company account.
- Company signs in through a one-time secure link.
- Company is forced to set a password on first login.

### 2) Poll targeting

- Poll creators select one or more target companies.
- Poll visibility is filtered by audience, not global by default.
- Company users see only polls that are relevant to their company scope.

### 3) QR-based participation

- Company generates a poll access link/QR for a selected poll.
- Participant opens the link and must provide valid email and mobile.
- Participant is redirected to the poll and can submit once.

### 4) Trust and anti-duplication

- Vote submission is blocked if the same poll already has a response from the same identity (email or mobile).
- Returning user sees that a vote already exists, with read access to submitted answers.

## What Is in This Repository

- `Survey_Basket.Api`: ASP.NET Core API surface (controllers, middleware, auth wiring, swagger).
- `Survey_Basket.Application`: business use cases, contracts, validation, policies, service orchestration.
- `Survey_Basket.Domain`: entities and domain abstractions.
- `Survey_Basket.Infrastructure`: EF Core, PostgreSQL, repositories, migrations, Hangfire, storage/email integrations.
- `Survey_Basket.UI`: Angular frontend for admin, company, and participant experiences.
- `Survey_Basket.Tests`: unit and integration tests.

## Technology Stack

- Backend: .NET 9, ASP.NET Core, EF Core, FluentValidation, ASP.NET Identity
- Database: PostgreSQL
- Auth: JWT + claims/permissions policy model
- Jobs: Hangfire (PostgreSQL storage)
- Frontend: Angular 21, TypeScript, Tailwind-based styling
- Logging: Serilog

## Local Setup

### Prerequisites

- .NET 9 SDK
- Node.js + npm
- PostgreSQL

### API configuration

Set connection strings in `Survey_Basket.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=SurveyBasketDb;Username=your_user;Password=your_password",
  "HangFireConnection": "Host=localhost;Port=5432;Database=SurveyBasket_Hangfire;Username=your_user;Password=your_password"
}
```

### Run backend

```bash
dotnet run --project Survey_Basket.Api
```

Swagger: `https://localhost:7194/swagger` (port may vary).

### Run frontend

```bash
cd Survey_Basket.UI
npm install
npm start
```

UI: `http://localhost:4200`

## Database Migrations

Create migration:

```bash
dotnet ef migrations add <MigrationName> --project Survey_Basket.Infrastructure --startup-project Survey_Basket.Api
```

Apply migration:

```bash
dotnet ef database update --project Survey_Basket.Infrastructure --startup-project Survey_Basket.Api
```

## Operational Notes

- If build/test fails with locked DLL errors, stop any running `Survey_Basket.Api` process before re-running commands.
- Hangfire recurring registration is guarded to avoid startup crashes when distributed lock is temporarily held.

## Product Capabilities Snapshot

- Role and permission based access
- Company account lifecycle (provision, activation, first-password)
- Poll create/update/publish with targeted audience assignment
- Company poll QR/link generation
- Participant onboarding via secure poll link
- One-response-per-identity enforcement
- Poll analytics (row data, per day, per question)
