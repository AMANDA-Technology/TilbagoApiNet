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

using TilbagoApiNet.Abstractions.Enums;
using TilbagoApiNet.Abstractions.Models;

namespace TilbagoApiNet.Abstractions.Views;

/// <summary>
/// View to create a debtor as natural person
/// </summary>
public class DebtorLegalPersonView
{
    /// <summary>
    /// The debtors external reference. If a debtor already exists, all properties are updated
    /// </summary>
    public required string ExternalRef { get; set; }

    /// <summary>
    /// Company name. If set, the debtor will be a legal person. Otherwise a physical person
    /// </summary>
    public required string Company { get; set; }

    /// <summary>
    /// Name addon of the legal person
    /// </summary>
    public string? NameAddon { get; set; }

    /// <summary>
    /// UID of the legal person. Must adher to format CHE-XXX.XXX.XXX or CHEXXXXXXXXX, where X stands for digit, also must
    /// be checkable value
    /// </summary>
    public required string CompanyUid { get; set; }

    /// <summary>
    /// Legal seat of the legal person
    /// </summary>
    public required string LegalSeat { get; set; }

    /// <summary>
    /// Contact person of the legal person
    /// </summary>
    public required string ContactPerson { get; set; }

    /// <summary>
    /// true if the actor is registered in Swiss trade register
    /// </summary>
    public required bool IsRegistered { get; set; }

    /// <summary>
    /// Preferred language of the debtor { de , fr , it }
    /// </summary>
    public Language? PreferredLanguage { get; set; }

    /// <summary>
    /// Phone number of the debtor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    public string? Phone1 { get; set; }

    /// <summary>
    /// Second phone number of the debtor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    public string? Phone2 { get; set; }

    /// <summary>
    /// Third phone number of the debtor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    public string? Phone3 { get; set; }

    /// <summary>
    /// fax number of the debtor
    /// <br />Note: A phone number. Must match the following regex: /(\s(\/|-|+|(|))\d+)+/
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// email of the debtor
    /// </summary>
    public string? EMail { get; set; }

    /// <summary>
    /// address of the debtor
    /// </summary>
    public required Address Address { get; set; }
}
