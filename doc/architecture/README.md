---
title: "Architecture Overview"
tags: ["architecture", "readme"]
---

# TilbagoApiNet Architecture Overview

Welcome to the architecture documentation for the TilbagoApiNet project. This library is a .NET client for the Tilbago Easy-API, allowing seamless integration with Tilbago's debt collection services.

## Purpose
The primary goal of this library is to encapsulate the HTTP communication, serialization, and endpoint routing required to interface with Tilbago, presenting a clean, strongly-typed C# API to consumer applications. It follows the established AMANDA-Technology "ApiNet" family conventions (similar to BexioApiNet and CashCtrlApiNet).

## Key Constraints
- Must remain compatible with standard `IServiceCollection` Dependency Injection setups.
- Avoids heavy 3rd-party dependencies where possible (uses native `System.Text.Json` and `HttpClient`).

## Tech Stack
| Component | Technology | Use Case |
|-----------|------------|----------|
| Language | C# 10+ | Core development language |
| HTTP | `HttpClient` | Used via `TilbagoConnectionHandler` for API calls |
| JSON | `System.Text.Json` | Serialization/Deserialization of requests and responses |
| Tests | NUnit | Integration and E2E testing framework |

## Documentation Map
Explore the C4 model documentation below to understand the system at different levels of abstraction:

1. **[System Context (C4 Level 1)](context.md)**: High-level view of the library and its external dependencies.
2. **[Containers (C4 Level 2)](containers.md)**: Breakdown of the library into NuGet packages/projects.
3. **Components (C4 Level 3)**:
   - [TilbagoApiNet (Main Library)](components/tilbagoapinet.md)
   - [TilbagoApiNet.Abstractions](components/abstractions.md)
   - [TilbagoApiNet.AspNetCore](components/aspnetcore.md)
   - [TilbagoApiNet.Tests](components/tests.md)
4. **[Architecture Decision Records (ADRs)](decisions/)**: Log of significant technical decisions.
5. **[Glossary](glossary.md)**: Domain terminology definition.