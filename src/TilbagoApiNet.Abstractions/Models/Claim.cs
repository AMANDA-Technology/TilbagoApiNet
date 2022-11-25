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
/// Tilbago claim
/// </summary>
public class Claim
{
    /// <summary>
    /// The claims external reference. Must be unique, otherwise an an exception is thrown
    /// </summary>
    [JsonPropertyName("externalRef")]
    public required string ExternalRef { get; set; }

    /// <summary>
    /// Claim amount in Rappen
    /// </summary>
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }

    /// <summary>
    /// The reason for this claim
    /// </summary>
    [JsonPropertyName("reason")]
    public required string Reason { get; set; }

    /// <summary>
    /// The interest start date, format YYYY-MM-DD
    /// </summary>
    [JsonPropertyName("interestDateFrom")]
    public required string InterestDateFrom { get; set; }

    /// <summary>
    /// The interest rate, supplied as string, decimal separator is point, value range 0 - 99.99999
    /// </summary>
    [JsonPropertyName("interestRate")]
    public required string InterestRate { get; set; }

    /// <summary>
    /// The collocation class for the claim, must be string, allowed values are 1, 2 or 3
    /// </summary>
    [JsonPropertyName("collocationClass")]
    public required string CollocationClass { get; set; }
}
