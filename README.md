# Survey Basket API

## Overview
Survey Basket is a robust and scalable Web API built with .NET 9, designed to facilitate the creation, management, and participation in surveys and polls. It follows the Clean Architecture principles to ensure maintainability, testability, and separation of concerns.

## Features
- **User Management**: Secure user registration and authentication using ASP.NET Core Identity and JWT Bearer tokens.
- **Role-Based Access Control (RBAC)**: Granular permission management using custom Policies and Requirements (`PermissionAuthorizationHandler`).
- **Poll Management**: Create, update, delete, and toggle the status of polls.
- **Question Management**: Add and manage questions for each poll.
- **Voting System**: Secure and validated voting mechanism.
- **Results & Analytics**: View detailed results of polls.
- **Company Provisioning Flow**: Admin can create company accounts in pending-password state.
- **Company Activation Flow**: Company accounts complete first-time activation by setting their own password with a one-time token.
- **Company User Records**: Company accounts can create company-scoped non-authenticated user records.
- **Background Jobs**: Automated tasks (e.g., daily notifications) using Hangfire.
- **Validation**: Comprehensive request validation using FluentValidation.
- **Caching**: Response caching using built-in mechanisms and HybridCache.
- **Logging**: Structured logging with Serilog.
- **Documentation**: Interactive API documentation via Swagger UI.

## Recent Fixes

- Added compatibility checks for legacy polls/questions so authorized partner/company users can manage older data even when newer ownership link records are missing.
- Added activation endpoint and contract for company accounts (`activate-company`) with secure password setup flow.
- Blocked sign-in for company-user records to enforce non-authenticated record behavior.
- Added admin company-account provisioning endpoint and company-scoped user-record creation endpoint.

## Technologies Used
- **Framework**: .NET 9, ASP.NET Core Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Mapping**: Mapster
- **Caching**: HybridCache (Microsoft.Extensions.Caching.Hybrid)
- **Email**: MailKit
- **Validation**: FluentValidation
- **Background Jobs**: Hangfire (with PostgreSQL storage)
- **Logging**: Serilog
- **API Documentation**: Swagger / OpenAPI

## Architecture
The solution is structured following the Clean Architecture pattern:

### **Survey_Basket.Api**
The entry point of the application. It contains Controllers, Middleware, and API configurations. It depends on the Application and Infrastructure layers.

### **Survey_Basket.Application**
Contains the core business logic and use cases. It defines interfaces and abstractions that are implemented by the Infrastructure layer.
- **Services**: Business logic implementations (e.g., `PollService`, `AuthService`).
- **Contracts**: DTOs (Data Transfer Objects) for requests and responses.
- **Abstractions**: Core interfaces and base classes (e.g., `Result`, `Error`, `IUnitOfWork`), and Constants (in `Const`).
- **Mapping**: Mapster configurations.
- **Validators**: FluentValidation rules.
- **Settings**: Configuration classes (e.g., `MailSettings`).
- **Templates**: Email templates.

### **Survey_Basket.Domain**
The heart of the application, containing enterprise logic and entities.
- **Entities**: Domain models (e.g., `Poll`, `Question`, `Vote`, `ApplicationUser`).
- **Abstractions**: Repository interfaces (`generate generic repository interface`).

### **Survey_Basket.Infrastructure**
Implementation of external concerns.
- **Persistence**: Database context (`ApplicationDbContext`), Repositories, and Unit of Work implementation.
- **Configurations**: Entity Framework Core configurations for database entities.
- **Services**: Infrastructure-specific services (e.g., Email Service).
- **DependencyInjection**: Registers all infrastructure services.

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### Configuration
1. Clone the repository.
2. Update the `appsettings.json` file in `Survey_Basket.Api` with your PostgreSQL connection strings:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=SurveyBasketDb;Username=your_user;Password=your_password",
     "HangFireConnection": "Host=localhost;Port=5432;Database=SurveyBasket_Hangfire;Username=your_user;Password=your_password"
   }
   ```
3. Configure JWT settings and Mail settings as needed.

### Running the Application
1. Navigate to the API project directory:
   ```bash
   cd Survey_Basket.Api
   ```
2. Run the application:
   ```bash
   dotnet run
   ```
3. The API will be available at `https://localhost:7194` (or similar port).

### API Documentation
Once the application is running, you can access the Swagger UI to explore and test the endpoints:
- URL: `https://localhost:7194/swagger`

### Key Endpoints Added in Recent Update

- `POST /api/users/company-accounts` - admin-only company account provisioning.
- `POST /me/company-accounts/{companyAccountUserId}/activation-token` - admin-generated activation token.
- `POST /api/auth/activate-company` - first-time company account activation and password setup.
- `POST /api/users/company-user-records` - company account creates own company user records (non-authenticated).

### Background Jobs Dashboard
Monitor background jobs via the Hangfire Dashboard:
- URL: `https://localhost:7194/jobs`
- **Authentication**: Secured with Basic Authentication (configured in `appsettings.json` under `HangfireSettings`).
