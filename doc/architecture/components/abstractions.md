---
title: "Component Diagram: TilbagoApiNet.Abstractions"
tags: ["architecture", "c4-level-3", "components", "abstractions"]
---

# Component Diagram: TilbagoApiNet.Abstractions

This package contains no business logic. It provides the Type definitions for payloads sent to and from the Tilbago API.

## Diagram

```mermaid
C4Component
    title Component diagram for TilbagoApiNet.Abstractions

    Container_Boundary(abstractions, "TilbagoApiNet.Abstractions") {
        Component(models, "Models", "Entities", "Root entities mapping to Tilbago resources (e.g., Case, Debtor, Creditor).")
        Component(views, "Views", "DTOs", "Specific input/output payloads (e.g., CreateNaturalPersonCaseView, CaseStatusView).")
        Component(enums, "Enums", "Constants", "Standardized enumerated values (e.g., Language, Sex).")
    }
```

## Key Components

- **`Models`**: 
  - `Case`: The primary resource in Tilbago representing a debt collection case.
  - `Debtor`, `Creditor`, `ResponsiblePerson`: Person/entity objects linked to a case.
  - `Claim`: Represents the financial demand.
- **`Views`**: 
  - Used for specific request structures where a full `Case` object may be too verbose or requires specific validation formats (e.g. `CreateLegalPersonCaseView`). 
  - The `ErrorModel` maps upstream 4xx/5xx responses to manageable objects.
- **`Enums`**: 
  - Strongly typed alternatives to magic strings required by the Tilbago API.