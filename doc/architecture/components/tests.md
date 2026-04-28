---
title: "Component Diagram: Test Projects"
tags: ["architecture", "c4-level-3", "components", "tests"]
---

# Component Diagram: Test Projects

Unit, integration, and end-to-end test coverage for the library, divided into four test projects plus a shared test-helper project. The four test projects follow the AMANDA-Technology canonical API client pattern; `TilbagoApiNet.TestHelpers` is a non-packed support library shared across them.

## Details

The test suite uses **NUnit** for all test projects. Bogus faker definitions are centralised in `TilbagoApiNet.TestHelpers` so that unit, integration, and E2E projects assert against the same generated shapes.

### 0. TilbagoApiNet.TestHelpers (shared)
- **Scope:** Non-packed support project (`<IsPackable>false</IsPackable>`) consumed by all three test projects. Holds Bogus fakers and assertion helpers — no test fixtures of its own.
- **Dependencies:** `Bogus` and a project reference to `TilbagoApiNet.Abstractions`.
- **Key Type:**
  - **`TilbagoFakers`** (static): exposes `public static readonly Faker<T>` fields for every domain type used by case-creation tests — `AddressFaker`, `ClaimFaker`, `ResponsiblePersonFaker`, `CreditorFaker`, `DebtorNaturalFaker`, `DebtorLegalFaker`, `NaturalCaseFaker`, and `LegalCaseFaker`. Each faker populates **every** settable property (including optionals such as `Pob`, `Phone2`, `Phone3`, `Fax`, `Title`, `BirthName`, `NameAddon`) so consumers can assert full mappings without seeing default-valued fields.
  - Also provides `AssertNaturalCaseMapped`, `AssertLegalCaseMapped`, `AssertAddressMapped`, `AssertClaimMapped`, and `AssertCreditorMapped` helper methods that centralise per-property `Shouldly` assertions reused by both unit and integration tests.

### 1. TilbagoApiNet.UnitTests
- **Scope:** Isolated service logic and domain models. Connector tests stub the `HttpMessageHandler` so no real network traffic is issued.
- **Dependencies:** `NSubstitute` for mocking interfaces (e.g., `ITilbagoConnectionHandler`), `Shouldly` for assertions, and a project reference to `TilbagoApiNet.TestHelpers` for shared Bogus fakers and mapping-assertion helpers.
- **Base Class:** `ServiceTestBase` sets up the mocked connection handler.
- **Key Files:**
  - **`TilbagoConfigurationTests.cs`**: Verifies configuration properties and validation.
  - **`TilbagoConnectionHandlerTests.cs`**: Validates `HttpClient` lifecycle, base URI resolution, and default headers.
  - **`TilbagoApiClientTests.cs`**: Confirms client initialization and contract fulfillment.
  - **`Services/Connectors/CaseServiceTests.cs`**: Covers `CreateNaturalPersonCaseAsync`, `CreateLegalPersonCaseAsync`, `GetStatusAsync`, and `AddAttachmentAsync` on `CaseService`. Uses a stub `HttpMessageHandler` and the shared `NaturalCaseFaker` / `LegalCaseFaker` to assert request shape (HTTP verb, path, body, multipart content) and to simulate both 2xx success and 4xx/5xx error paths. The `_OnSuccess_SendsPutToCaseEndpointWithMappedBody` tests assert **every** mapped property — including `Fax`, `Phone2`, `Phone3`, `Title`, `BirthName`, `NameAddon`, `PreferredLanguage`, `PayeeReference`, `ResponsiblePerson`, `SubsidiaryClaims`, `SourceRefEmail`, `SourceRefKey`, and `Creditor`.

### 2. TilbagoApiNet.AspNetCore.UnitTests
- **Scope:** Dependency-injection wiring for the `TilbagoApiNet.AspNetCore` package. Drives both `IServiceCollection.AddTilbagoServices` overloads (string `apiKey`/`baseUri` and `ITilbagoConfiguration` instance) through a real `ServiceCollection` and asserts both descriptor metadata and resolution behaviour.
- **Dependencies:** `Microsoft.Extensions.DependencyInjection`, `NSubstitute`, and `Shouldly`. No project reference to `TestHelpers` — the tests cover wiring, not domain payloads.
- **Key Files:**
  - **`TilbagoServiceCollectionTests.cs`**: 10 tests verifying that `ITilbagoConfiguration` is registered as Singleton with the supplied values, `ITilbagoConnectionHandler` and `ITilbagoApiClient` are registered as Scoped, and `provider.GetRequiredService<ITilbagoApiClient>()` returns a non-null `TilbagoApiClient` instance for both overloads.

### 3. TilbagoApiNet.IntegrationTests
- **Scope:** HTTP client serialization, deserialization, and request building, mocked at the HTTP layer. Verifies wire-level behaviour including request methods, paths, header handling (like `api_key`), and response parsing.
- **Dependencies:** `WireMock.Net` to spin up a local HTTP server and stub responses, `Shouldly` for assertions, and a project reference to `TilbagoApiNet.TestHelpers` for shared Bogus fakers and mapping-assertion helpers.
- **Base Class:** `IntegrationTestBase` manages the `WireMockServer` lifecycle per test.
- **Key Files:**
  - **`Services/Connectors/CaseServiceIntegrationTests.cs`**: Comprehensive integration tests for `CaseService`.
    - Covers `CreateNaturalPersonCaseAsync`, `CreateLegalPersonCaseAsync`, `GetStatusAsync`, and `AddAttachmentAsync`.
    - Verifies that JSON bodies sent to the API match the expected `camelCase` format driven by `[JsonPropertyName]` annotations. The `_OnSuccess_SendsPutWith*BodyAndApiKeyHeader` tests assert **every** mapped field — `Fax`, `Phone2`, `Phone3`, `Title`, `BirthName`, `PreferredLanguage`, `PayeeReference`, `ResponsiblePerson`, `SubsidiaryClaims`, `SourceRefEmail`, `SourceRefKey`, `Creditor`, and `NameAddon` — using the shared `NaturalCaseFaker` / `LegalCaseFaker` and `Assert*Mapped` helpers.
    - Specifically asserts the **Content-Type removal quirk** in `AddAttachmentAsync` by inspecting the wire-level request to ensure the header is absent from the multipart content.
    - Validates error mapping by mocking 4xx/5xx responses and asserting that `InvalidOperationException` is thrown with the correct message.

### 4. TilbagoApiNet.E2eTests
- **Scope:** Real HTTP requests against the live Tilbago Easy-API.
- **Dependencies:** `Shouldly` for assertions and a project reference to `TilbagoApiNet.TestHelpers` for shared Bogus fakers (used to generate unique cases and debtors to avoid collisions).
- **Base Class:** `E2eTestBase` reads environment variables, initializes a real `TilbagoApiClient`, and provides a LIFO cleanup stack for resources created during test bodies.

#### E2E Configuration
E2E tests expect the following Environment Variables to be present on the host or CI runner:
- `TilbagoApiNet__ApiKey`
- `TilbagoApiNet__BaseUri`

### Key E2E Files
- **`CreateCase.cs`**: Validates the end-to-end flow of initializing the client, creating natural person and legal person cases, asserting IDs, fetching statuses, and successfully uploading attachments.