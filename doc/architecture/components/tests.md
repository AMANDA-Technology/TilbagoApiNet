---
title: "Component Diagram: Test Projects"
tags: ["architecture", "c4-level-3", "components", "tests"]
---

# Component Diagram: Test Projects

Unit, integration, and end-to-end test coverage for the library, divided into three distinct projects according to the AMANDA-Technology canonical API client pattern.

## Details

The test suite uses **NUnit** for all three projects.

### 1. TilbagoApiNet.UnitTests
- **Scope:** Isolated service logic and domain models, without HTTP communication.
- **Dependencies:** `NSubstitute` for mocking interfaces (e.g., `ITilbagoConnectionHandler`) and `Shouldly` for assertions.
- **Base Class:** `ServiceTestBase` sets up the mocked connection handler.
- **Key Files:**
  - **`TilbagoConfigurationTests.cs`**: Verifies configuration properties and validation.
  - **`TilbagoConnectionHandlerTests.cs`**: Validates `HttpClient` lifecycle, base URI resolution, and default headers.
  - **`TilbagoApiClientTests.cs`**: Confirms client initialization and contract fulfillment.

### 2. TilbagoApiNet.IntegrationTests
- **Scope:** HTTP client serialization, deserialization, and request building, mocked at the HTTP layer.
- **Dependencies:** `WireMock.Net` to spin up a local HTTP server and stub responses, `Bogus` for fake data generation, and `NSubstitute`.
- **Base Class:** `IntegrationTestBase` manages the `WireMockServer` lifecycle per test.

### 3. TilbagoApiNet.E2eTests
- **Scope:** Real HTTP requests against the live Tilbago Easy-API.
- **Dependencies:** `Bogus` for generating unique cases and debtors to avoid collisions.
- **Base Class:** `E2eTestBase` reads environment variables and initializes a real `TilbagoApiClient`.

#### E2E Configuration
E2E tests expect the following Environment Variables to be present on the host or CI runner:
- `TilbagoApiNet__ApiKey`
- `TilbagoApiNet__BaseUri`

### Key E2E Files
- **`CreateCase.cs`**: Validates the end-to-end flow of initializing the client, creating a natural person case, asserting the ID, fetching its status, and successfully uploading an attachment.
- **`Helpers.cs`**: Provides randomization functions to ensure unique `ExternalRef` strings across test runs, avoiding case collision on the server.