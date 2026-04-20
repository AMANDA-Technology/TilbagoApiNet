# TilbagoApiNet

Unofficial .NET API client implementation for the [tilbago Easy-API](https://tilbago.ch/), an online service for debt enforcement and loss certificates in Switzerland.

> This library was designed for our specific needs at AMANDA Technology. Feel free to create an issue or pull request when you encounter a missing feature or a bug — contributions are welcome!

With special thanks to tilbago AG for the handy online service. See [tilbago website](https://tilbago.ch/).

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/main.yml)
[![CodeQL](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/codeql-analysis.yml)
[![SonarCloud](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/sonar-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/TilbagoApiNet/actions/workflows/sonar-analysis.yml)

## Packages

This solution ships three NuGet packages:

| Package | Description |
|---------|-------------|
| `TilbagoApiNet` | Main HTTP client and connector services to interact with the tilbago Easy-API |
| `TilbagoApiNet.Abstractions` | Shared domain models, views, and enums (e.g. `Case`, `Claim`, `Debtor`, `Address`) |
| `TilbagoApiNet.AspNetCore` | Dependency injection integration for ASP.NET Core applications |

- **TilbagoApiNet.Abstractions** — use this alone in shared model projects that only need the types without the HTTP implementation.
- **TilbagoApiNet** — depends on `TilbagoApiNet.Abstractions`. Use for manual (non-DI) setups.
- **TilbagoApiNet.AspNetCore** — depends on `TilbagoApiNet`. Registers the client and its dependencies via `IServiceCollection`.

## Installation

```sh
# For ASP.NET Core projects (recommended)
dotnet add package TilbagoApiNet.AspNetCore

# For manual / non-DI usage
dotnet add package TilbagoApiNet

# For shared model projects (types only)
dotnet add package TilbagoApiNet.Abstractions
```

## Usage

### Setup service manually

```csharp
using TilbagoApiNet.Services;

var configuration = new TilbagoConfiguration(
    apiKey: "your-api-key",
    baseUri: "https://api.tilbago.ch/v1/"
);

using var connectionHandler = new TilbagoConnectionHandler(configuration);
using var client = new TilbagoApiClient(connectionHandler);

// client.CaseService is now ready to use
```

### Setup service with dependency injection

In an ASP.NET Core application, register the services in `Program.cs`:

```csharp
using TilbagoApiNet.AspNetCore;

builder.Services.AddTilbagoServices(
    apiKey: builder.Configuration["Tilbago:ApiKey"]!,
    baseUri: builder.Configuration["Tilbago:BaseUri"]!
);
```

Then inject `ITilbagoApiClient` wherever you need it:

```csharp
using TilbagoApiNet.Interfaces;

public class MyService(ITilbagoApiClient tilbagoClient)
{
    public async Task DoSomethingAsync()
    {
        var status = await tilbagoClient.CaseService.GetStatusAsync("some-case-id");
        // ...
    }
}
```

### Create a case (natural person example)

```csharp
using TilbagoApiNet.Abstractions.Enums;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;

var caseView = new CreateNaturalPersonCaseView
{
    ExternalRef = "my-case-001",          // Must be unique across your cases
    CertificateOfLoss = false,
    Debtor = new DebtorNaturalPersonView
    {
        ExternalRef = "debtor-001",       // Must be unique; existing debtor is updated on match
        Name = "Max",
        Surname = "Mustermann",
        Sex = Sex.M,
        DateOfBirth = "1980-01-15",       // Formats: yyyy-MM-dd, dd.MM.yyyy, or yyyy
        Nationality = "CH",               // ISO 3166-1 alpha-2 (case-insensitive)
        PreferredLanguage = Language.De,
        Address = new Address
        {
            Street = "Musterstrasse",
            StreetNumber = "1",
            Zip = "8001",
            City = "Zürich"
        }
    },
    Claim = new Claim
    {
        ExternalRef = "claim-001",        // Must be unique
        Amount = 150000,                  // In Rappen (CHF 1'500.00 = 150000)
        Reason = "Unpaid invoice",
        InterestDateFrom = "2024-01-01",  // Format: yyyy-MM-dd
        InterestRate = "5.0",             // Decimal string, range 0–99.99999
        CollocationClass = "3"            // Allowed: "1", "2", or "3"
    }
};

string? caseId = await client.CaseService.CreateNaturalPersonCaseAsync(caseView);
Console.WriteLine($"Created case: {caseId}");
```

### Add an attachment

```csharp
await using var fileStream = File.OpenRead("invoice.pdf");

string? attachmentId = await client.CaseService.AddAttachmentAsync(
    caseId: caseId!,
    fileName: "invoice.pdf",
    fileContent: fileStream
);

Console.WriteLine($"Attachment ID: {attachmentId}");
```

### Get case state

```csharp
CaseStatusView? status = await client.CaseService.GetStatusAsync(caseId!);

Console.WriteLine($"Status code : {status?.StatusCode}");
Console.WriteLine($"Description : {status?.Description}");
Console.WriteLine($"eSchKG code : {status?.ESchKgCode}");  // null if not applicable
```

## Known limitations

- **API scope**: The tilbago Easy-API currently supports limited querying. The client focuses primarily on `PUT` (create case / add attachment) and `GET` (status) flows.
- **Attachment upload quirk**: The `Content-Type` header is intentionally removed from the multipart content when uploading attachments. Including it causes a `500` response from the tilbago server.
- **Error handling**: API errors (`4xx` / `5xx`) are raised as `InvalidOperationException` containing the message returned by tilbago.
- **End-to-End tests**: Tests in `TilbagoApiNet.E2eTests` target the live API and require the `TilbagoApiNet__ApiKey` and `TilbagoApiNet__BaseUri` environment variables. Unit and Integration tests run offline.

## License

[MIT](LICENSE) — Copyright (c) 2022 AMANDA Technology GmbH
