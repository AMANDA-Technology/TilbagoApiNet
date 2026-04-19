---
title: "Container Diagram"
tags: ["architecture", "c4-level-2", "containers"]
---

# Container Diagram: TilbagoApiNet Packages

The TilbagoApiNet system is broken down into modular packages (containers in C4 terminology) delivered as NuGet packages. This follows the AMANDA-Technology ApiNet pattern.

## Diagram

```mermaid
C4Container
    title Container diagram for TilbagoApiNet

    Person(app, "Consumer Application", "A .NET Application")

    System_Boundary(tilbagoApiNet, "TilbagoApiNet Ecosystem") {
        Container(abstractions, "TilbagoApiNet.Abstractions", ".NET Library", "Provides domain models, enums, and views (DTOs). Contains no logic.")
        Container(mainLib, "TilbagoApiNet", ".NET Library", "Implements the core HTTP connection handler, serialization, and service endpoints.")
        Container(aspNetCore, "TilbagoApiNet.AspNetCore", ".NET Library", "Provides IServiceCollection extensions for seamless ASP.NET Core DI registration.")
    }

    System_Ext(tilbagoApi, "Tilbago Easy-API", "External REST API")

    Rel(app, aspNetCore, "Registers via", "AddTilbagoServices()")
    Rel(app, mainLib, "Calls endpoints via", "ITilbagoApiClient")
    Rel(app, abstractions, "Uses types from", "Domain Models")

    Rel(aspNetCore, mainLib, "Configures", "DI")
    Rel(mainLib, abstractions, "References", "Models")
    
    Rel(mainLib, tilbagoApi, "Communicates with", "JSON/HTTPS")
```

## Packages

| Container | Technology | Responsibility |
|-----------|------------|----------------|
| **[[tilbagoapinet]]** (`src/TilbagoApiNet`) | C# / .NET | The core execution layer. Holds the connection handler, HTTP client, and connector implementations mapping to the upstream endpoints. |
| **[[abstractions]]** (`src/TilbagoApiNet.Abstractions`) | C# / .NET | Shared definitions. Holds the `Case`, `Creditor`, `Debtor` models, enumerations, and specific view models used in requests/responses. |
| **[[aspnetcore]]** (`src/TilbagoApiNet.AspNetCore`) | C# / .NET | Integration layer. Contains the `TilbagoServiceCollection` for dependency injection inside modern ASP.NET Core apps. |