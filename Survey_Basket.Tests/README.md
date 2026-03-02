# Survey_Basket.Tests

This project contains automated tests for Survey Basket.

## Test Strategy

- Unit tests validate service-level business rules in isolation using mocks.
- Integration tests validate API behavior with in-memory test host/database setup.

## Structure

- `Unit/`: fast tests focused on individual services and rule branches.
- `Integration/`: endpoint-oriented tests using `WebApplicationFactory`.
- `Abstractions/`: shared test base classes, auth handler, and factory setup.

## Implementation Details

- Uses `xUnit` as test runner.
- Uses `Moq` for unit-test mocking.
- Uses `FluentAssertions` for readable assertions.
- Integration tests override authentication to a deterministic test principal and permissions.
- Integration tests use EF Core InMemory provider to avoid external DB dependency.

## Running Tests

```bash
dotnet test Survey_Basket.Tests/Survey_Basket.Tests.csproj
```

## Common Local Issue

- If tests fail with file lock errors (`Survey_Basket.Api` DLL in use), stop the running API process and rerun tests.
