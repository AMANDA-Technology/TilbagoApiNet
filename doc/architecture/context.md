---
title: "Context Diagram"
tags: ["architecture", "c4-level-1", "context"]
---

# System Context: TilbagoApiNet

TilbagoApiNet acts as a bridge between .NET-based applications (developed by AMANDA-Technology or external consumers) and the Tilbago Easy-API platform, enabling automated debt enforcement and loss certificate processing.

## Diagram

```mermaid
C4Context
    title System Context diagram for TilbagoApiNet

    Person(consumer, ".NET Developer / Application", "A software system or developer building an application that needs to manage debt collection cases.")
    System(tilbagoApiNet, "TilbagoApiNet", "Provides a type-safe, abstracted .NET client library for the Tilbago API.")
    System_Ext(tilbagoApi, "Tilbago Easy-API", "External REST API provided by tilbago AG for debt enforcement, loss certificates, and dunning.")

    Rel(consumer, tilbagoApiNet, "Uses", ".NET API / NuGet")
    Rel(tilbagoApiNet, tilbagoApi, "Makes API calls to", "JSON/HTTPS")
```

## Actors & External Systems

| Name | Type | Description |
|------|------|-------------|
| **.NET Developer / Application** | Actor | The system or individual integrating this library to programmatically manage debt collection workflows. |
| **TilbagoApiNet** | System | The .NET library being documented here. |
| **Tilbago Easy-API** | External System | The upstream service provided by tilbago AG. Manages the lifecycle of a debt collection case in Switzerland. |