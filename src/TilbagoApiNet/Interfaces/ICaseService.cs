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

using System.IO;
using System.Threading.Tasks;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;

namespace TilbagoApiNet.Interfaces;

/// <summary>
/// Tilbago case service endpoint
/// </summary>
public interface ICaseService
{
    /// <summary>
    /// Add a new case on tilbago
    /// </summary>
    /// <param name="tilbagoCase"></param>
    /// <returns>Case ID</returns>
    public Task<string?> CreateAsync(Case tilbagoCase);

    /// <summary>
    /// Add an attachment to a case on tilbago
    /// </summary>
    /// <returns>Attachment ID</returns>
    public Task<string?> AddAttachmentAsync(string caseId, string fileName, Stream fileContent);

    /// <summary>
    /// Get the status of a case on tilbago
    /// </summary>
    /// <param name="caseId"></param>
    /// <returns></returns>
    public Task<CaseStatusView?> GetStatusAsync(string caseId);

    /// <summary>
    /// Add a new natural person case on tilbago
    /// </summary>
    /// <param name="createNaturalPersonCaseView"></param>
    /// <returns>Case ID</returns>
    public Task<string?> CreateNaturalPersonCaseAsync(CreateNaturalPersonCaseView createNaturalPersonCaseView);

    /// <summary>
    /// Add a new legal person case on tilbago
    /// </summary>
    /// <param name="createLegalPersonCaseView"></param>
    /// <returns>Case ID</returns>
    public Task<string?> CreateLegalPersonCaseAsync(CreateLegalPersonCaseView createLegalPersonCaseView);
}
