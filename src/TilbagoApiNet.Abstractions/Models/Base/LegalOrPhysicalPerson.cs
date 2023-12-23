/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

using System.Text.Json.Serialization;
using TilbagoApiNet.Abstractions.Enums;

namespace TilbagoApiNet.Abstractions.Models.Base;

/// <summary>
/// Base class for all legal or physical persons (actor, debtor)
/// </summary>
public abstract class LegalOrPhysicalPerson : PhysicalPerson
{
    /// <summary>
    /// Company name. If set, the actor will be a legal person. Otherwise a physical person
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; set; }

    /// <summary>
    /// UID of the legal person. Must adhere to format CHE-XXX.XXX.XXX or CHEXXXXXXXXX, where X stands for digit, also must be checkable value
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
    /// True if the actor is registered in Swiss trade register
    /// </summary>
    [JsonPropertyName("isRegistered")]
    public bool? IsRegistered { get; set; }

    /// <summary>
    /// Preferred language of the actor { de , fr , it }
    /// </summary>
    [JsonPropertyName("preferredLanguage")]
    public Language? PreferredLanguage { get; set; }

    /// <summary>
    /// Phone number of the actor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone1")]
    public string? Phone1 { get; set; }

    /// <summary>
    /// Second phone number of the actor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone2")]
    public string? Phone2 { get; set; }

    /// <summary>
    /// Third phone number of the actor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("phone3")]
    public string? Phone3 { get; set; }

    /// <summary>
    /// fax number of the actor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    [JsonPropertyName("fax")]
    public string? Fax { get; set; }

    /// <summary>
    /// email of the actor
    /// </summary>
    [JsonPropertyName("email")]
    public string? EMail { get; set; }

    /// <summary>
    /// Preferred language of the actor
    /// </summary>
    [JsonPropertyName("address")]
    public Address? Address { get; set; }
}
