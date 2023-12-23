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

using System.Text;
using System.Text.Json;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Interfaces.Connectors;

namespace TilbagoApiNet.Services.Connectors;

/// <summary>
/// Tilbago case service endpoint
/// </summary>
public class CaseService : ICaseService
{
    /// <summary>
    /// Tilbago connection handler
    /// </summary>
    private readonly ITilbagoConnectionHandler _tilbagoConnectionHandler;

    /// <summary>
    /// Inject connection handler at construction
    /// </summary>
    /// <param name="tilbagoConnectionHandler"></param>
    public CaseService(ITilbagoConnectionHandler tilbagoConnectionHandler)
    {
        _tilbagoConnectionHandler = tilbagoConnectionHandler;
    }

    /// <summary>
    /// Add a new case on tilbago
    /// </summary>
    /// <param name="tilbagoCase"></param>
    /// <returns>Case ID</returns>
    public async Task<string?> CreateAsync(Case tilbagoCase)
    {
        // PUT /case
        var response = await _tilbagoConnectionHandler.Client.PutAsync("case",
            new StringContent(JsonSerializer.Serialize(tilbagoCase), Encoding.UTF8, "application/json"));
        var responseContent = await response.Content.ReadAsStreamAsync();

        // 200 OK -> case id
        if (response.IsSuccessStatusCode)
            return (await JsonSerializer.DeserializeAsync<CaseCreateResultView>(responseContent))?.CaseId;

        // 401 Unauthorized = Invalid api_key
        // 402 Payment Required = Insufficient account balance
        // default = Unexpected error
        throw new InvalidOperationException((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
    }

    /// <summary>
    /// Add an attachment to a case on tilbago
    /// </summary>
    /// <returns>Attachment ID</returns>
    public async Task<string?> AddAttachmentAsync(string caseId, string fileName, Stream fileContent)
    {
        using var multipartFormContent = new MultipartContent();

        // Load the file and set the file's Content-Type header
        var fileStreamContent = new StreamContent(fileContent);

        // Add the file
        multipartFormContent.Add(fileStreamContent);
        multipartFormContent.Headers.Remove("Content-Type"); // Must be removed or 500 from server
        multipartFormContent.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + "\"");

        // Send it
        var response = await _tilbagoConnectionHandler.Client.PutAsync($"case/{caseId}/attachment", multipartFormContent);
        var responseContent = await response.Content.ReadAsStreamAsync();

        // 200 OK -> attachmentId
        if (response.IsSuccessStatusCode)
        {
            return (await JsonSerializer.DeserializeAsync<AddAttachmentResultView>(responseContent))?.AttachmentId;
        }

        // 401 Unauthorized = Invalid api_key
        // default = Unexpected error
        throw new InvalidOperationException((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
    }

    /// <summary>
    /// Get the status of a case on tilbago
    /// </summary>
    /// <param name="caseId"></param>
    /// <returns></returns>
    public async Task<CaseStatusView?> GetStatusAsync(string caseId)
    {
        // GET /case/{caseId}/status
        var response = await _tilbagoConnectionHandler.Client.GetAsync($"case/{caseId}/status");
        var responseContent = await response.Content.ReadAsStreamAsync();
        // 200 OK -> caseStatus
        if (response.IsSuccessStatusCode) return await JsonSerializer.DeserializeAsync<CaseStatusView>(responseContent);

        // 401 Unauthorized = Invalid api_key
        // default = Unexpected error
        throw new InvalidOperationException((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
    }

    /// <summary>
    /// Add a new natural person case on tilbago
    /// </summary>
    /// <param name="createNaturalPersonCaseView"></param>
    /// <returns></returns>
    public async Task<string?> CreateNaturalPersonCaseAsync(CreateNaturalPersonCaseView createNaturalPersonCaseView)
    {
        var caseMapped = new Case
        {
            Claim = createNaturalPersonCaseView.Claim,
            Debtor = new()
            {
                Address = createNaturalPersonCaseView.Debtor.Address,
                Fax = createNaturalPersonCaseView.Debtor.Fax,
                Name = createNaturalPersonCaseView.Debtor.Name,
                Nationality = createNaturalPersonCaseView.Debtor.Nationality,
                Phone1 = createNaturalPersonCaseView.Debtor.Phone1,
                Phone2 = createNaturalPersonCaseView.Debtor.Phone2,
                Phone3 = createNaturalPersonCaseView.Debtor.Phone3,
                Sex = createNaturalPersonCaseView.Debtor.Sex,
                Surname = createNaturalPersonCaseView.Debtor.Surname,
                Title = createNaturalPersonCaseView.Debtor.Title,
                BirthName = createNaturalPersonCaseView.Debtor.BirthName,
                EMail = createNaturalPersonCaseView.Debtor.EMail,
                ExternalRef = createNaturalPersonCaseView.Debtor.ExternalRef,
                PreferredLanguage = createNaturalPersonCaseView.Debtor.PreferredLanguage,
                DateOfBirth = createNaturalPersonCaseView.Debtor.DateOfBirth
            },
            ExternalRef = createNaturalPersonCaseView.ExternalRef,
            PayeeReference = createNaturalPersonCaseView.PayeeReference,
            ResponsiblePerson = createNaturalPersonCaseView.ResponsiblePerson,
            SubsidiaryClaims = createNaturalPersonCaseView.SubsidiaryClaims,
            CertificateOfLoss = Convert.ToString(createNaturalPersonCaseView.CertificateOfLoss),
            SourceRefEmail = createNaturalPersonCaseView.SourceRefEmail,
            SourceRefKey = createNaturalPersonCaseView.SourceRefKey,
            Creditor = createNaturalPersonCaseView.Creditor
        };

        return await CreateAsync(caseMapped);
    }

    /// <summary>
    /// Add a new legal person case on tilbago
    /// </summary>
    /// <param name="createLegalPersonCaseView"></param>
    /// <returns></returns>
    public async Task<string?> CreateLegalPersonCaseAsync(CreateLegalPersonCaseView createLegalPersonCaseView)
    {
        var caseMapped = new Case
        {
            Claim = createLegalPersonCaseView.Claim,
            Debtor = new()
            {
                Address = createLegalPersonCaseView.Debtor.Address,
                Fax = createLegalPersonCaseView.Debtor.Fax,
                Phone1 = createLegalPersonCaseView.Debtor.Phone1,
                Phone2 = createLegalPersonCaseView.Debtor.Phone2,
                Phone3 = createLegalPersonCaseView.Debtor.Phone3,
                EMail = createLegalPersonCaseView.Debtor.EMail,
                ExternalRef = createLegalPersonCaseView.Debtor.ExternalRef,
                PreferredLanguage = createLegalPersonCaseView.Debtor.PreferredLanguage,
                Company = createLegalPersonCaseView.Debtor.Company,
                CompanyUid = createLegalPersonCaseView.Debtor.CompanyUid,
                ContactPerson = createLegalPersonCaseView.Debtor.ContactPerson,
                IsRegistered = createLegalPersonCaseView.Debtor.IsRegistered,
                LegalSeat = createLegalPersonCaseView.Debtor.LegalSeat,
                NameAddon = createLegalPersonCaseView.Debtor.NameAddon
            },
            ExternalRef = createLegalPersonCaseView.ExternalRef,
            PayeeReference = createLegalPersonCaseView.PayeeReference,
            ResponsiblePerson = createLegalPersonCaseView.ResponsiblePerson,
            SubsidiaryClaims = createLegalPersonCaseView.SubsidiaryClaims,
            CertificateOfLoss = Convert.ToString(createLegalPersonCaseView.CertificateOfLoss),
            SourceRefEmail = createLegalPersonCaseView.SourceRefEmail,
            SourceRefKey = createLegalPersonCaseView.SourceRefKey,
            Creditor = createLegalPersonCaseView.Creditor
        };

        return await CreateAsync(caseMapped);
    }
}
