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
/// Base class for all physical persons (creditor, debtor)
/// </summary>
public abstract class PhysicalPerson
{
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
}
