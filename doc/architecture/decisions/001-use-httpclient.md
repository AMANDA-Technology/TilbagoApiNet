---
title: "001: Use HttpClient per Connection Handler"
status: "accepted"
date: "2026-04-19"
tags: ["architecture", "decision", "httpclient", "di"]
---

# 001: Use HttpClient per Connection Handler

## Context
When building a .NET API client, a mechanism is needed to process HTTP requests. Older patterns instantiating `HttpClient` in `using` blocks risk socket exhaustion. In modern .NET, `IHttpClientFactory` is preferred, but managing it within a standalone class library without forcing a heavy dependency on `Microsoft.Extensions.Http` can be challenging.

## Decision
The `TilbagoApiNet` main library instantiates a single `HttpClient` inside the `TilbagoConnectionHandler`. 
When registered via `TilbagoApiNet.AspNetCore`, the handler is registered as `Scoped`. This means a new `HttpClient` instance is created per web request scope in ASP.NET Core environments, and disposed at the end of the scope.

## Consequences
- **Positive:** Keeps the `TilbagoApiNet` base library independent of ASP.NET Core-specific `IHttpClientFactory` implementations.
- **Negative:** `Scoped` registration of `HttpClient` wrapper can theoretically lead to socket exhaustion in incredibly high-throughput scenarios compared to a true `IHttpClientFactory` singleton pooling approach. However, for a 3rd-party API client managing debt collection cases, throughput is rarely high enough to trigger this boundary.

## Notes for Agents
If you modify how `HttpClient` is injected or created, you MUST ensure you do not break the constructor signature of `TilbagoConnectionHandler` which currently accepts only `ITilbagoConfiguration`.