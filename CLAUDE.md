# TilbagoApiNet

TilbagoApiNet is a .NET API client implementation for the tilbago Easy-API, an online service for debt enforcement and loss certificates in Switzerland. It provides an abstracted, type-safe way for .NET applications to create cases (natural/legal persons), query status, and upload attachments to Tilbago, following the established architectural patterns seen across AMANDA-Technology API clients (like BexioApiNet and CashCtrlApiNet).

## Tech Stack
- **Framework:** .NET (C# 10+)
- **Build System:** MSBuild / `dotnet` CLI
- **HTTP Client:** `HttpClient` with `System.Text.Json` for serialization
- **Testing:** NUnit (`nunit`) framework

## Solution Structure
- `src/TilbagoApiNet`: Main library implementing the API connectors, services, and HTTP client handler.
- `src/TilbagoApiNet.Abstractions`: Core domain models (Cases, Debtors, Creditors), views, and enums.
- `src/TilbagoApiNet.AspNetCore`: Dependency Injection integration (`IServiceCollection` extensions) for ASP.NET Core apps.
- `src/TilbagoApiNet.TestHelpers`: Non-packed shared test infrastructure exposing `TilbagoFakers` — Bogus fakers (`AddressFaker`, `ClaimFaker`, `CreditorFaker`, `ResponsiblePersonFaker`, `DebtorNaturalFaker`, `DebtorLegalFaker`, `NaturalCaseFaker`, `LegalCaseFaker`) plus `Assert*Mapped` helpers consumed by all test projects.
- `src/TilbagoApiNet.UnitTests`: NUnit tests with NSubstitute for mocking, Bogus for fake test data (via `TilbagoApiNet.TestHelpers`), and Shouldly for assertions; connector tests stub `HttpMessageHandler` for offline logic testing.
- `src/TilbagoApiNet.AspNetCore.UnitTests`: NUnit tests for `TilbagoServiceCollection.AddTilbagoServices` overloads, asserting registered descriptor lifetimes (Singleton config, Scoped handler/client) and resolved types via `IServiceCollection`/`IServiceProvider`.
- `src/TilbagoApiNet.IntegrationTests`: NUnit tests with WireMock.Net to mock the Tilbago API.
- `src/TilbagoApiNet.E2eTests`: NUnit tests targeting the live Tilbago API, using Bogus for data generation and Shouldly for assertions.

## Build Commands
- **Restore:** `dotnet restore`
- **Build:** `dotnet build`
- **Test:** `dotnet test` (Requires environment variables `TilbagoApiNet__ApiKey` and `TilbagoApiNet__BaseUri` for E2E tests).

## Key Conventions
- **Connector Pattern:** API interactions are grouped by domain into "Services" or "Connectors" (e.g., `CaseService.cs`), mirroring the Tilbago API structure.
- **Dependency Injection:** Clients are expected to register the API via `services.AddTilbagoServices(apiKey, baseUri)`. The `ITilbagoApiClient` is scoped and aggregates all individual service connectors.
- **Connection Handler:** A central `TilbagoConnectionHandler` manages the singleton-like `HttpClient` instance, including default headers (e.g., `api_key`).
- **Domain Modeling:** Entities are placed in `.Abstractions/Models` and view models (used for specific request bodies like `CreateNaturalPersonCaseView`) are placed in `.Abstractions/Views`.
- **Serialization:** `System.Text.Json.Serialization` is used heavily, particularly `[JsonPropertyName]` to map to the API's camelCase naming.
- **Error Handling:** 4xx and 5xx responses are captured and throw an `InvalidOperationException` containing the serialized `ErrorModel.Message` from the response.

## Architecture Patterns
This repository follows the AMANDA-Technology "ApiNet" family of patterns:
1. **Aggregator Interface:** The root interface `ITilbagoApiClient` serves as a facade to access modular service interfaces (e.g., `ICaseService`).
2. **Abstractions Separation:** Core types are isolated in an `Abstractions` package to allow sharing models without forcing the HTTP implementation dependency.
3. **ASP.NET Core Integration:** A lightweight `.AspNetCore` package provides standard `IServiceCollection` extension methods to register the configuration, connection handler, and the API client natively.

## Important File Locations
- **Entry Point:** `src/TilbagoApiNet/Services/TilbagoApiClient.cs`
- **DI Registration:** `src/TilbagoApiNet.AspNetCore/TilbagoServiceCollection.cs`
- **Connection Management:** `src/TilbagoApiNet/Services/TilbagoConnectionHandler.cs`
- **Case Endpoints:** `src/TilbagoApiNet/Services/Connectors/CaseService.cs`
- **Core Domain Model:** `src/TilbagoApiNet.Abstractions/Models/Case.cs`

## Known Constraints
- The `CaseService.AddAttachmentAsync` method removes the `Content-Type` header for the `MultipartContent` manually, as the server responds with a 500 error if it is included. This behavior is explicitly verified in `CaseServiceIntegrationTests.cs`.
- `dotnet test` will execute Unit and Integration tests without requiring any credentials. Only `E2eTests` require a valid Tilbago API key via environment variables.
- Unlike BexioApiNet or CashCtrlApiNet which may implement robust querying abstractions, Tilbago API currently supports limited querying and focuses mainly on `PUT` / `GET status` flows.

## Adding a New API Endpoint
1. **Model:** Add new entity models in `src/TilbagoApiNet.Abstractions/Models/` and request/response views in `src/TilbagoApiNet.Abstractions/Views/`. Use `[JsonPropertyName]` for all fields.
2. **Interface:** Create a new service interface (e.g., `INewEndpointService.cs`) in `src/TilbagoApiNet/Interfaces/Connectors/`.
3. **Implementation:** Create the class implementing the new interface (e.g., `NewEndpointService.cs`) in `src/TilbagoApiNet/Services/Connectors/`. Inject `ITilbagoConnectionHandler` in the constructor. Use `_tilbagoConnectionHandler.Client` for HTTP calls.
4. **Registration:** Update `ITilbagoApiClient` and `TilbagoApiClient` to expose the new service as a property (e.g., `public INewEndpointService NewEndpointService { get; set; }`). Instantiate it in the constructor.
5. **Test:** Add new test methods in `src/TilbagoApiNet.UnitTests/`, `src/TilbagoApiNet.IntegrationTests/`, and `src/TilbagoApiNet.E2eTests/` to verify behavior at all levels. Reuse fakers from `TilbagoApiNet.TestHelpers.TilbagoFakers` rather than redefining Bogus rules locally; if a new domain type needs faker coverage, add it to `TilbagoFakers` so all three test projects share the definition.