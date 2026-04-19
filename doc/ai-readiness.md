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
- **Test Framework:** NUnit. Run using `dotnet test`. Note: Running tests requires environment variables `TilbagoApiNet__ApiKey` and `TilbagoApiNet__BaseUri`.
- **What IS covered:** 
  - `src/TilbagoApiNet.Tests/CreateCase.cs` contains a single End-to-End (E2E) test (`GetCreatedCaseAndAddAttachmentForNaturalPerson`) covering:
    - `CreateNaturalPersonCaseAsync`
    - `GetStatusAsync`
    - `AddAttachmentAsync`
- **What is NOT covered:**
  - `CreateLegalPersonCaseAsync`
  - `CreateAsync` (raw case creation)
  - All error paths (400, 401, 402, 500 status codes).
  - There are zero offline unit tests (mocking the HTTP client).
- **Test Quality:** The existing test is an integration/E2E test that hits the live Tilbago API. This is dangerous for AI development because running `dotnet test` repeatedly could incur costs, create garbage data, or fail entirely if credentials aren't present in the environment. Existing tests are superficial "happy path" checks.

**Rate:** Minimal Coverage

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
| **Offline Unit Testing Suite** | Abstract the HTTP calls or mock `HttpMessageHandler` to allow testing the serialization and logic without requiring a live Tilbago API key. | M | High |
| **Resilient Error Deserialization** | Update `CaseService` to check the `Content-Type` before attempting to deserialize an `ErrorModel`, preventing `JsonException` on 5xx HTML errors. | S | High |
| **Migrate to IHttpClientFactory** | Refactor `TilbagoConnectionHandler` and `TilbagoServiceCollection` to leverage ASP.NET Core's `IHttpClientFactory` to prevent socket exhaustion. | M | Medium |
| **Complete API Implementation** | Audit the official Tilbago Easy-API documentation to identify missing endpoints (e.g., querying cases, fetching PDF documents) and implement them. | L | Low |
