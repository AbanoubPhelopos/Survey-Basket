# Survey Basket API

## Overview
Survey Basket is a robust and scalable Web API built with .NET 9, designed to facilitate the creation, management, and participation in surveys and polls. It follows the Clean Architecture principles to ensure maintainability, testability, and separation of concerns.

## Features
- **User Management**: Secure user registration and authentication using ASP.NET Core Identity and JWT Bearer tokens.
- **Role-Based Access Control (RBAC)**: Granular permission management with custom roles and policies.
- **Poll Management**: Create, update, delete, and toggle the status of polls.
- **Question Management**: Add and manage questions for each poll.
- **Voting System**: Secure and validated voting mechanism.
- **Results & Analytics**: View detailed results of polls.
- **Background Jobs**: Automated tasks (e.g., daily notifications) using Hangfire.
- **Validation**: Comprehensive request validation using FluentValidation.
- **Logging**: Structured logging with Serilog.
- **Documentation**: Interactive API documentation via Swagger UI.

## Technologies Used
- **Framework**: .NET 9, ASP.NET Core Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Mapping**: Mapster
- **Validation**: FluentValidation
- **Background Jobs**: Hangfire (with PostgreSQL storage)
- **Logging**: Serilog
- **API Documentation**: Swagger / OpenAPI

## Architecture
The solution is structured following the Clean Architecture pattern:
- **Survey_Basket.Api**: The entry point of the application (Controllers, Middleware).
- **Survey_Basket.Application**: Business logic, Use Cases, Interfaces, DTOs, and Validators.
- **Survey_Basket.Domain**: Enterprise logic, Entities, and Value Objects.
- **Survey_Basket.Infrastructure**: Implementation of external concerns (Database, Identity, File System, etc.).

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

### Background Jobs Dashboard
Monitor background jobs via the Hangfire Dashboard:
- URL: `https://localhost:7194/jobs`
- *Note: Authentication may be required based on configuration.*