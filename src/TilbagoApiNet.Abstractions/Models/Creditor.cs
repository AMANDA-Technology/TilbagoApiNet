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

using System.Text.Json.Serialization;
using TilbagoApiNet.Abstractions.Enums;

namespace TilbagoApiNet.Abstractions.Models;

/// <summary>
/// Tilbago creditor
/// </summary>
public class Creditor
{
    /// <summary>
    /// The creditor external reference. If a creditor already exists, all properties are updated
    /// </summary>
    [JsonPropertyName("externalRef")]
    public string? ExternalRef { get; set; }

    /// <summary>
    /// Company name. If set, the creditor will be a legal person. Otherwise a physical person
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; set; }

    /// <summary>
    /// Name of the physical person
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Surname of the physical person
    /// </summary>
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>
    /// Name addon of the legal person
    /// </summary>
    [JsonPropertyName("nameAddon")]
    public string? NameAddon { get; set; }

    /// <summary>
    /// Sex of the physical person { M , F , U }
    /// </summary>
    [JsonPropertyName("sex")]
    public Sex? Sex { get; set; }

    /// <summary>
    /// Date of birth of the physical person. Valid formats are yyyy-mm-dd and dd.mm.yyyy and yyyy
    /// </summary>
    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Birth name of the physical person if applicable
    /// </summary>
    [JsonPropertyName("birthName")]
    public string? BirthName { get; set; }

    /// <summary>
    /// Title of the physical person
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Nationality of the physical person. Case insensitive two-letter ISO 3166-1 alpha-2 code
    /// </summary>
    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    /// <summary>
    /// UID of the legal person. Must adher to format CHE-XXX.XXX.XXX or CHEXXXXXXXXX, where X stands for digit, also must be checkable value
    /// </summary>
    [JsonPropertyName("companyUid")]
    public string? CompanyUid { get; set; }

    /// <summary>
    /// Legal seat of the legal person
    /// </summary>
    [JsonPropertyName("legalSeat")]
    public string? LegalSeat { get; set; }

    /// <summary>
    /// Contact person of the legal person
    /// </summary>
    [JsonPropertyName("contactPerson")]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// true if the actor is registered in Swiss trade register
    /// </summary>
    [JsonPropertyName("isRegistered")]
    public bool? IsRegistered { get; set; }

    /// <summary>
    /// URL of the trade register entry of the legal person
    /// </summary>
    [JsonPropertyName("tradeRegisterUrl")]
    public bool? TradeRegisterUrl { get; set; }

    /// <summary>
    /// IBAN of the creditor
    /// </summary>
    [JsonPropertyName("iban")]
    public bool? Iban { get; set; }

    /// <summary>
    /// Preferred language of the creditor { de , fr , it }
    /// </summary>
    [JsonPropertyName("preferredLanguage")]
    public Language? PreferredLanguage { get; set; }

    /// <summary>
    /// Phone number of the creditor
    /// <br/>Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone1")]
    public string? Phone1 { get; set; }

    /// <summary>
    /// Second phone number of the creditor
    /// <br/>Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone2")]
    public string? Phone2 { get; set; }

    /// <summary>
    /// Third phone number of the creditor
    /// <br/>Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone3")]
    public string? Phone3 { get; set; }

    /// <summary>
    /// fax number of the creditor
    /// <br/>Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("fax")]
    public string? Fax { get; set; }

    /// <summary>
    /// email of the creditor
    /// </summary>
    [JsonPropertyName("email")]
    public string? EMail { get; set; }

    /// <summary>
    /// Preferred language of the creditor
    /// </summary>
    [JsonPropertyName("address")]
    public Address? Address { get; set; }
}
