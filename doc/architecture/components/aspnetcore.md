---
title: "Component Diagram: TilbagoApiNet.AspNetCore"
tags: ["architecture", "c4-level-3", "components", "aspnetcore"]
---

# Component Diagram: TilbagoApiNet.AspNetCore

A helper package for standardizing dependency injection in ASP.NET Core applications.

## Diagram

```mermaid
C4Component
    title Component diagram for TilbagoApiNet.AspNetCore

    Container(app, "Consumer Web App", "ASP.NET Core")
    Container(mainLib, "TilbagoApiNet", ".NET Library")

    Container_Boundary(aspNetCore, "TilbagoApiNet.AspNetCore") {
        Component(extensions, "TilbagoServiceCollection", "Extension Methods", "Provides IServiceCollection.AddTilbagoServices")
    }

    Rel(app, extensions, "Invokes at startup", "C#")
    Rel(extensions, mainLib, "Registers types from", "DI")
```

## Key Components

- **`TilbagoServiceCollection`**: Contains static extension methods for `IServiceCollection`.
  - Registers `TilbagoConfiguration` as a Singleton.
  - Registers `ITilbagoConnectionHandler` as Scoped.
  - Registers `ITilbagoApiClient` as Scoped.

By scoping the client and connection handler, the library natively supports request-scoped scenarios common in web applications, without retaining stale connections unnecessarily.