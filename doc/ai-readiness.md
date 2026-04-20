---
title: "AI Readiness Assessment"
tags: ["ai", "readiness", "assessment"]
---

# AI Readiness Assessment: TilbagoApiNet

## Section 1: Documentation Quality

**Assessment:**
- **Existing Documentation:** The project contains a clear `README.md` with build commands and basic architectural principles. The `doc/architecture/` folder provides C4 Level 1 and 2 models (Context and Containers) which clearly outline the interaction between the abstractions, the main library, and the ASP.NET Core dependency injection package.
- **Missing/Insufficient:** The project lacks a comprehensive API reference mapping all available Tilbago endpoints to what is currently implemented or missing. While there are XML comments on classes (e.g., `CaseService.cs`), the domain concepts (what a "Debtor" vs. "Creditor" entails specific to Tilbago) rely heavily on external knowledge or the tilbago.ch website.
- **Improvements for AI:** Adding a formal OpenAPI/Swagger JSON (if available from Tilbago) to the documentation would allow AI agents to confidently implement new endpoints without hallucinating request/response structures.

**Rate:** Ready

## Section 2: Test Coverage

**Assessment:**
- **Test Framework:** NUnit across three dedicated projects (`UnitTests`, `IntegrationTests`, `E2eTests`).
- **What IS covered:** 
  - `src/TilbagoApiNet.E2eTests/CreateCase.cs` contains an End-to-End (E2E) test (`GetCreatedCaseAndAddAttachmentForNaturalPerson`) covering:
    - `CreateNaturalPersonCaseAsync`
    - `GetStatusAsync`
    - `AddAttachmentAsync`
  - Integration tests and unit tests infrastructure is set up to decouple offline logic from real Tilbago API testing.
- **What is NOT covered:**
  - `CreateLegalPersonCaseAsync`
  - `CreateAsync` (raw case creation)
  - All error paths (400, 401, 402, 500 status codes).
  - Business logic testing is incomplete despite having the infrastructure ready.
- **Test Quality:** The project is now structured into `UnitTests`, `IntegrationTests` (with WireMock.Net), and `E2eTests` (live Tilbago API). This limits the risk of AI agents creating garbage data when running `dotnet test`, as `E2eTests` will only execute if credentials are provided in the environment.

**Rate:** Baseline Coverage

## Section 3: Technical Debt & Danger Zones

**Assessment:**
- **Fragile Code (Error Deserialization):** In `src/TilbagoApiNet/Services/Connectors/CaseService.cs` (lines 73, 104, 122), error handling assumes the response body is always valid JSON mapping to `ErrorModel`:
  ```csharp
  throw new InvalidOperationException((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
  ```
  *Why it's dangerous:* If the server returns an HTML error page (e.g., 502 Bad Gateway from a proxy), `JsonSerializer` will throw an unhandled `JsonException`, masking the true HTTP error. AI should be careful to handle non-JSON error payloads safely.
- **Risky Areas (API Quirks):** In `CaseService.cs` (line 89), there is a specific workaround:
  ```csharp
  multipartFormContent.Headers.Remove("Content-Type"); // Must be removed or 500 from server
  ```
  *Why it's dangerous:* An AI agent attempting to refactor or "clean up" the `AddAttachmentAsync` method might remove this hack, inadvertently breaking file uploads to the API.
- **Architectural Debt (HttpClient usage):** `src/TilbagoApiNet/Services/TilbagoConnectionHandler.cs` manually instantiates a `new HttpClient()`. 
  *Why it's dangerous:* In high-throughput ASP.NET Core applications, this can lead to socket exhaustion or DNS caching issues. Modifying this requires coordinated changes across the `TilbagoApiNet.AspNetCore` dependency injection setup to use `IHttpClientFactory`.

## Section 4: Backlog Ideas

| Title | Description | Complexity | Priority |
|-------|-------------|------------|----------|
| **Resilient Error Deserialization** | Update `CaseService` to check the `Content-Type` before attempting to deserialize an `ErrorModel`, preventing `JsonException` on 5xx HTML errors. | S | High |
| **Migrate to IHttpClientFactory** | Refactor `TilbagoConnectionHandler` and `TilbagoServiceCollection` to leverage ASP.NET Core's `IHttpClientFactory` to prevent socket exhaustion. | M | Medium |
| **Complete API Implementation** | Audit the official Tilbago Easy-API documentation to identify missing endpoints (e.g., querying cases, fetching PDF documents) and implement them. | L | Low |
