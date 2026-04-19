---
title: "Component Diagram: TilbagoApiNet"
tags: ["architecture", "c4-level-3", "components", "main-library"]
---

# Component Diagram: TilbagoApiNet (Main Library)

The main library manages the HTTP lifecycle and aggregates API connectors.

## Diagram

```mermaid
C4Component
    title Component diagram for TilbagoApiNet (Main Library)

    Container(aspNetCore, "TilbagoApiNet.AspNetCore", "DI Extensions")
    System_Ext(tilbagoApi, "Tilbago Easy-API", "HTTPS")

    Container_Boundary(mainLib, "TilbagoApiNet") {
        Component(config, "ITilbagoConfiguration", "Configuration", "Holds the BaseUri and ApiKey required to connect.")
        Component(connHandler, "ITilbagoConnectionHandler", "Connection Manager", "Maintains the HttpClient instance and default headers.")
        
        Component(apiClient, "ITilbagoApiClient", "Facade", "Root object aggregating all individual service endpoints.")
        Component(caseService, "ICaseService", "Service Connector", "Implements operations against /case endpoints (Create, Status, Attachments).")
    }

    Rel(aspNetCore, config, "Provides", "Settings")
    Rel(aspNetCore, connHandler, "Registers", "Scoped")
    Rel(aspNetCore, apiClient, "Registers", "Scoped")
    
    Rel(apiClient, caseService, "Delegates to", "ICaseService")
    Rel(caseService, connHandler, "Uses", "HttpClient")
    Rel(connHandler, tilbagoApi, "Sends requests to", "JSON/HTTPS")
```

## Key Components

- **`ITilbagoApiClient` / `TilbagoApiClient`**: The main entry point. Exposes `CaseService`.
- **`ITilbagoConnectionHandler` / `TilbagoConnectionHandler`**: Centralizes `HttpClient` creation. Sets the `BaseAddress` and injects the `api_key` header from the provided configuration. Implements `IDisposable` to properly dispose the underlying client.
- **`ICaseService` / `CaseService`**: Responsible for serialization, API path formatting (`/case`, `/case/{id}/status`), and deserializing responses. Translates specific views (like `CreateNaturalPersonCaseView`) into the underlying `Case` model before transmitting.