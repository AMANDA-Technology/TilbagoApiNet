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

namespace TilbagoApiNet.Abstractions.Models;

/// <summary>
/// Tilbago case
/// </summary>
public class Case
{
    /// <summary>
    /// The cases external reference. Must be unique, otherwise an an exception is thrown
    /// </summary>
    [JsonPropertyName("externalRef")]
    public string? ExternalRef { get; set; }

    /// <summary>
    /// Declares this case to be based on a certificate of loss. Possible values true/false
    /// </summary>
    [JsonPropertyName("certificateOfLoss")]
    public string? CertificateOfLoss { get; set; }

    /// <summary>
    /// The responsible person for this case. Must be an employee of the creditor.
    /// </summary>
    [JsonPropertyName("responsiblePerson")]
    public ResponsiblePerson? ResponsiblePerson { get; set; }

    /// <summary>
    /// Optional reference key to identify the calling system, write only
    /// </summary>
    [JsonPropertyName("sourceRefKey")]
    public string? SourceRefKey { get; set; }

    /// <summary>
    /// Optional email address of contact person of the calling system, write only
    /// </summary>
    [JsonPropertyName("sourceRefEmail")]
    public string? SourceRefEmail { get; set; }

    /// <summary>
    /// Reference to be used for payments by the office. Must fulfill check sum test, if account is not of type QR-IBAN
    /// account
    /// </summary>
    [JsonPropertyName("payeeReference")]
    public string? PayeeReference { get; set; }

    /// <summary>
    /// The debtor description. Depending on the debtor properties, the debtor will be a physical or legal person
    /// </summary>
    [JsonPropertyName("debtor")]
    public Debtor? Debtor { get; set; }

    /// <summary>
    /// The creditor object is optional. If it exists this expresses that the customer wishes to build a case as
    /// representative. If the customer is not a representative an error will be raised. Attention! If the creditor already
    /// exists (referenced via externalRef) and is linked to other cases, any changes to the creditor will affect those
    /// cases as well!
    /// </summary>
    [JsonPropertyName("creditor")]
    public Creditor? Creditor { get; set; }

    /// <summary>
    /// Claim description
    /// </summary>
    [JsonPropertyName("claim")]
    public Claim? Claim { get; set; }

    /// <summary>
    /// Subsidiary claim descriptions
    /// </summary>
    [JsonPropertyName("subsidiaryClaims")]
    public List<Claim>? SubsidiaryClaims { get; set; }

    /// <summary>
    /// Case status: { 002 }
    /// <br />- "002": User has to review the case before submit
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
