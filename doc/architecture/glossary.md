---
title: "Glossary"
tags: ["architecture", "domain", "glossary"]
---

# Glossary

Domain terminology used within the TilbagoApiNet project and the upstream Tilbago API.

## Core Terms

- **Tilbago**: An online service/platform in Switzerland for managing debt collection, dunning, and loss certificates.
- **Case**: The primary entity representing a single debt collection procedure against a debtor.
- **Creditor**: The person or organization that is owed money.
- **Debtor**: The person (Natural Person) or organization (Legal Person) that owes money.
- **Claim**: The specific financial amount owed, including principal, interest rate, and reasons.
- **Certificate of Loss (Verlustschein)**: A legal document in Switzerland certifying that a debt could not be collected, which can be used to initiate future collections.
- **Payee Reference**: A unique reference string (often associated with an ESR/QR-IBAN) used to match incoming payments to the specific case.
- **External Reference (ExternalRef)**: A unique identifier provided by the calling system (e.g., AMANDA-Technology ERP) to correlate a Tilbago case with the internal record.

## API / Technical Terms

- **ApiNet**: An internal naming convention at AMANDA-Technology denoting a .NET C# API client library (e.g., TilbagoApiNet, BexioApiNet, CashCtrlApiNet).
- **View**: In this architecture, a "View" is a specific Request or Response DTO (Data Transfer Object) that does not perfectly align 1:1 with a core domain `Model`. Example: `CreateNaturalPersonCaseView`.