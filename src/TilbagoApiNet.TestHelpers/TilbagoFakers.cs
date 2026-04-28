/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Globalization;
using Bogus;
using TilbagoApiNet.Abstractions.Enums;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;

namespace TilbagoApiNet.TestHelpers;

/// <summary>
///     Centralised Bogus fakers for Tilbago domain types, shared across unit, integration and end-to-end test projects.
///     Each faker populates every settable property on its target type so consumers can assert full
///     <c>CreateNaturalPersonCaseAsync</c> / <c>CreateLegalPersonCaseAsync</c> mappings without seeing default-valued
///     fields.
/// </summary>
public static class TilbagoFakers
{
    /// <summary>
    ///     Faker for <see cref="Address" /> populating every property — including the optional <c>Pob</c>.
    /// </summary>
    public static readonly Faker<Address> AddressFaker = new Faker<Address>()
        .RuleFor(x => x.Zip, f => f.Address.ZipCode("####"))
        .RuleFor(x => x.City, f => f.Address.City())
        .RuleFor(x => x.Street, f => f.Address.StreetName())
        .RuleFor(x => x.StreetNumber, f => f.Address.BuildingNumber())
        .RuleFor(x => x.Pob, f => f.Random.Replace("####"));

    /// <summary>
    ///     Faker for <see cref="Claim" /> populating every required property with realistic culture-invariant strings.
    /// </summary>
    public static readonly Faker<Claim> ClaimFaker = new Faker<Claim>()
        .CustomInstantiator(f => new Claim
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Amount = f.Random.Int(100, 100_000_00),
            Reason = f.Lorem.Sentence(),
            InterestDateFrom = f.Date.Past().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            InterestRate = f.Random.Decimal(0, 10).ToString("F2", CultureInfo.InvariantCulture),
            CollocationClass = f.PickRandom("1", "2", "3")
        });

    /// <summary>
    ///     Faker for <see cref="ResponsiblePerson" /> producing a single email address.
    /// </summary>
    public static readonly Faker<ResponsiblePerson> ResponsiblePersonFaker = new Faker<ResponsiblePerson>()
        .RuleFor(x => x.Email, f => f.Internet.Email());

    /// <summary>
    ///     Faker for <see cref="Creditor" /> populating every property inherited from
    ///     <see cref="TilbagoApiNet.Abstractions.Models.Base.LegalOrPhysicalPerson" /> plus the creditor-specific
    ///     <c>ExternalRef</c>, <c>TradeRegisterUrl</c> and <c>Iban</c> flags.
    /// </summary>
    public static readonly Faker<Creditor> CreditorFaker = new Faker<Creditor>()
        .CustomInstantiator(f => new Creditor
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            TradeRegisterUrl = f.Random.Bool(),
            Iban = f.Random.Bool(),
            Company = f.Company.CompanyName(),
            CompanyUid = $"CHE-{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}",
            ContactPerson = f.Name.FullName(),
            IsRegistered = true,
            LegalSeat = f.Address.City(),
            Address = AddressFaker.Generate(),
            EMail = f.Internet.Email(),
            Phone1 = f.Phone.PhoneNumber(),
            Phone2 = f.Phone.PhoneNumber(),
            Phone3 = f.Phone.PhoneNumber(),
            Fax = f.Phone.PhoneNumber(),
            Name = f.Name.FirstName(),
            Surname = f.Name.LastName(),
            NameAddon = f.Lorem.Word(),
            Title = f.Name.JobTitle(),
            BirthName = f.Name.LastName(),
            Sex = f.PickRandom<Sex>(),
            DateOfBirth = f.Date.Past(60).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Nationality = "CH",
            PreferredLanguage = Language.de
        });

    /// <summary>
    ///     Faker for <see cref="DebtorNaturalPersonView" /> populating every property — required and optional —
    ///     so case-creation tests can assert the full natural-person mapping (Title, BirthName, Phone2, Phone3, Fax, ...).
    /// </summary>
    public static readonly Faker<DebtorNaturalPersonView> DebtorNaturalFaker = new Faker<DebtorNaturalPersonView>()
        .CustomInstantiator(f => new DebtorNaturalPersonView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Name = f.Name.FirstName(),
            Surname = f.Name.LastName(),
            Sex = f.PickRandom<Sex>(),
            DateOfBirth = f.Date.Past(60).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            BirthName = f.Name.LastName(),
            Title = f.Name.JobTitle(),
            Address = AddressFaker.Generate(),
            EMail = f.Internet.Email(),
            Phone1 = f.Phone.PhoneNumber(),
            Phone2 = f.Phone.PhoneNumber(),
            Phone3 = f.Phone.PhoneNumber(),
            Fax = f.Phone.PhoneNumber(),
            Nationality = "CH",
            PreferredLanguage = Language.de
        });

    /// <summary>
    ///     Faker for <see cref="DebtorLegalPersonView" /> populating every property — required and optional —
    ///     including <c>NameAddon</c>, <c>Phone2</c>, <c>Phone3</c> and <c>Fax</c> so case-creation tests can assert the
    ///     full legal-person mapping.
    /// </summary>
    public static readonly Faker<DebtorLegalPersonView> DebtorLegalFaker = new Faker<DebtorLegalPersonView>()
        .CustomInstantiator(f => new DebtorLegalPersonView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Company = f.Company.CompanyName(),
            CompanyUid = $"CHE-{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}",
            NameAddon = f.Lorem.Word(),
            ContactPerson = f.Name.FullName(),
            IsRegistered = true,
            LegalSeat = f.Address.City(),
            Address = AddressFaker.Generate(),
            EMail = f.Internet.Email(),
            Phone1 = f.Phone.PhoneNumber(),
            Phone2 = f.Phone.PhoneNumber(),
            Phone3 = f.Phone.PhoneNumber(),
            Fax = f.Phone.PhoneNumber(),
            PreferredLanguage = Language.de
        });

    /// <summary>
    ///     Faker for <see cref="CreateNaturalPersonCaseView" /> populating every property — required and optional —
    ///     including <c>ResponsiblePerson</c>, <c>SubsidiaryClaims</c>, <c>SourceRefEmail</c>, <c>SourceRefKey</c>,
    ///     <c>PayeeReference</c> and <c>Creditor</c>.
    /// </summary>
    public static readonly Faker<CreateNaturalPersonCaseView> NaturalCaseFaker =
        new Faker<CreateNaturalPersonCaseView>()
            .CustomInstantiator(f => new CreateNaturalPersonCaseView
            {
                ExternalRef = f.Random.AlphaNumeric(12),
                CertificateOfLoss = false,
                Debtor = DebtorNaturalFaker.Generate(),
                Claim = ClaimFaker.Generate(),
                ResponsiblePerson = ResponsiblePersonFaker.Generate(),
                SubsidiaryClaims = ClaimFaker.Generate(2),
                SourceRefEmail = f.Internet.Email(),
                SourceRefKey = f.Random.AlphaNumeric(8),
                PayeeReference = f.Random.ReplaceNumbers("###############"),
                Creditor = CreditorFaker.Generate()
            });

    /// <summary>
    ///     Faker for <see cref="CreateLegalPersonCaseView" /> populating every property — required and optional —
    ///     including <c>ResponsiblePerson</c>, <c>SubsidiaryClaims</c>, <c>SourceRefEmail</c>, <c>SourceRefKey</c>,
    ///     <c>PayeeReference</c> and <c>Creditor</c>.
    /// </summary>
    public static readonly Faker<CreateLegalPersonCaseView> LegalCaseFaker = new Faker<CreateLegalPersonCaseView>()
        .CustomInstantiator(f => new CreateLegalPersonCaseView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            CertificateOfLoss = false,
            Debtor = DebtorLegalFaker.Generate(),
            Claim = ClaimFaker.Generate(),
            ResponsiblePerson = ResponsiblePersonFaker.Generate(),
            SubsidiaryClaims = ClaimFaker.Generate(2),
            SourceRefEmail = f.Internet.Email(),
            SourceRefKey = f.Random.AlphaNumeric(8),
            PayeeReference = f.Random.ReplaceNumbers("###############"),
            Creditor = CreditorFaker.Generate()
        });
}
