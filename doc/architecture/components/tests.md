---
title: "Component Diagram: TilbagoApiNet.Tests"
tags: ["architecture", "c4-level-3", "components", "tests"]
---

# Component Diagram: TilbagoApiNet.Tests

Unit and integration test coverage for the library.

## Details

The test suite uses **NUnit**. 

Currently, the primary mechanism of testing involves executing real requests against the Tilbago API. This makes it an integration/E2E test suite rather than purely isolated unit tests.

### Configuration
Tests expect the following Environment Variables to be present on the host or CI runner:
- `TilbagoApiNet__ApiKey`
- `TilbagoApiNet__BaseUri`

### Key Files
- **`CreateCase.cs`**: Validates the end-to-end flow of initializing the client, creating a natural person case, asserting the ID, fetching its status, and successfully uploading an attachment.
- **`Helpers.cs`**: Provides randomization functions to ensure unique `ExternalRef` strings across test runs, avoiding case collision on the server.