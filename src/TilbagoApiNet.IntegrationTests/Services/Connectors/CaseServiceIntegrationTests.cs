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

using System.Net;
using System.Text.Json;
using Shouldly;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Services;
using TilbagoApiNet.Services.Connectors;
using TilbagoApiNet.TestHelpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Case = TilbagoApiNet.Abstractions.Models.Case;

namespace TilbagoApiNet.IntegrationTests.Services.Connectors;

/// <summary>
///     Integration tests for <see cref="CaseService" /> exercising the full HTTP stack against a
///     <see cref="WireMock.Server.WireMockServer" /> stub. Verifies wire-level behaviour including request method, path,
///     body shape (driven by <c>[JsonPropertyName]</c> annotations), header handling and 4xx/5xx error mapping.
/// </summary>
[TestFixture]
public class CaseServiceIntegrationTests : IntegrationTestBase
{
    private const string ApiPathPrefix = "/api/v1";
    private const string ApiKey = "test-api-key";

    private TilbagoApiClient _apiClient = null!;

    /// <summary>
    ///     Wires a real <see cref="TilbagoApiClient" /> to the per-test WireMock server URL with a fake API key.
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();
        var configuration = new TilbagoConfiguration(ApiKey, $"{Server.Url}{ApiPathPrefix}");
        _apiClient = new TilbagoApiClient(new TilbagoConnectionHandler(configuration));
    }

    /// <summary>
    ///     Disposes the API client (and its <see cref="HttpClient" />) before the WireMock server is torn down.
    /// </summary>
    public override void TearDown()
    {
        _apiClient.Dispose();
        base.TearDown();
    }

    /// <summary>
    ///     Verifies the case id from the JSON body of a 200 response is parsed and returned to the caller.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_ReturnsCaseIdFromResponse()
    {
        const string expectedCaseId = "case-natural-42";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new CaseCreateResultView { CaseId = expectedCaseId })));
        var view = TilbagoFakers.NaturalCaseFaker.Generate();

        var result = await _apiClient.CaseService.CreateNaturalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies the request reaches WireMock as a PUT to <c>/api/v1/case</c>, with the api_key header populated and a
    ///     body whose JSON property names match the <c>[JsonPropertyName]</c> annotations on <see cref="Case" />,
    ///     <see cref="Debtor" /> and <see cref="Claim" />. Asserts every property of the natural-person view —
    ///     required and optional — including <c>Fax</c>, <c>Phone2</c>, <c>Phone3</c>, <c>Title</c>, <c>BirthName</c>,
    ///     <c>PreferredLanguage</c>, <c>PayeeReference</c>, <c>ResponsiblePerson</c>, <c>SubsidiaryClaims</c>,
    ///     <c>SourceRefEmail</c>, <c>SourceRefKey</c> and <c>Creditor</c>.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_SendsPutWithCamelCaseBodyAndApiKeyHeader()
    {
        var view = TilbagoFakers.NaturalCaseFaker.Generate();
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" })));

        await _apiClient.CaseService.CreateNaturalPersonCaseAsync(view);

        var entries = Server.LogEntries.ToList();
        entries.Count.ShouldBe(1);
        var request = entries[0].RequestMessage!;
        request.Method.ShouldBe("PUT");
        request.AbsolutePath.ShouldBe($"{ApiPathPrefix}/case");

        request.Headers.ShouldNotBeNull();
        request.Headers!.ShouldContainKey("api_key");
        request.Headers["api_key"].ShouldContain(ApiKey);

        var body = request.Body;
        body.ShouldNotBeNullOrWhiteSpace();
        body.ShouldContain("\"externalRef\"");
        body.ShouldContain("\"certificateOfLoss\"");
        body.ShouldContain("\"debtor\"");
        body.ShouldContain("\"claim\"");
        body.ShouldContain("\"responsiblePerson\"");
        body.ShouldContain("\"subsidiaryClaims\"");
        body.ShouldContain("\"sourceRefEmail\"");
        body.ShouldContain("\"sourceRefKey\"");
        body.ShouldContain("\"payeeReference\"");
        body.ShouldContain("\"creditor\"");
        body.ShouldContain("\"phone2\"");
        body.ShouldContain("\"phone3\"");
        body.ShouldContain("\"fax\"");
        body.ShouldContain("\"title\"");
        body.ShouldContain("\"birthName\"");
        body.ShouldContain("\"preferredLanguage\"");

        var sentCase = JsonSerializer.Deserialize<Case>(body);
        sentCase.ShouldNotBeNull();
        AssertNaturalCaseMapped(sentCase, view);
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> deserialised from the response body.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Invalid api_key";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.Unauthorized)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage })));
        var view = TilbagoFakers.NaturalCaseFaker.Generate();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _apiClient.CaseService.CreateNaturalPersonCaseAsync(view));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies the case id from the JSON body of a 200 response is parsed and returned to the caller.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_ReturnsCaseIdFromResponse()
    {
        const string expectedCaseId = "case-legal-99";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new CaseCreateResultView { CaseId = expectedCaseId })));
        var view = TilbagoFakers.LegalCaseFaker.Generate();

        var result = await _apiClient.CaseService.CreateLegalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies every property of the legal-person view — required and optional — is mapped onto the wire payload
    ///     using the camelCase property names defined by <c>[JsonPropertyName]</c>, including <c>NameAddon</c>,
    ///     <c>Fax</c>, <c>Phone2</c>, <c>Phone3</c>, <c>PreferredLanguage</c>, <c>PayeeReference</c>,
    ///     <c>ResponsiblePerson</c>, <c>SubsidiaryClaims</c>, <c>SourceRefEmail</c>, <c>SourceRefKey</c> and
    ///     <c>Creditor</c>.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_SendsPutWithLegalPersonFieldsInBody()
    {
        var view = TilbagoFakers.LegalCaseFaker.Generate();
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" })));

        await _apiClient.CaseService.CreateLegalPersonCaseAsync(view);

        var entries = Server.LogEntries.ToList();
        entries.Count.ShouldBe(1);
        var request = entries[0].RequestMessage!;
        request.Method.ShouldBe("PUT");
        request.AbsolutePath.ShouldBe($"{ApiPathPrefix}/case");

        var body = request.Body;
        body.ShouldNotBeNullOrWhiteSpace();
        body.ShouldContain("\"company\"");
        body.ShouldContain("\"companyUid\"");
        body.ShouldContain("\"legalSeat\"");
        body.ShouldContain("\"contactPerson\"");
        body.ShouldContain("\"isRegistered\"");
        body.ShouldContain("\"nameAddon\"");
        body.ShouldContain("\"responsiblePerson\"");
        body.ShouldContain("\"subsidiaryClaims\"");
        body.ShouldContain("\"sourceRefEmail\"");
        body.ShouldContain("\"sourceRefKey\"");
        body.ShouldContain("\"payeeReference\"");
        body.ShouldContain("\"creditor\"");
        body.ShouldContain("\"phone2\"");
        body.ShouldContain("\"phone3\"");
        body.ShouldContain("\"fax\"");
        body.ShouldContain("\"preferredLanguage\"");

        var sentCase = JsonSerializer.Deserialize<Case>(body);
        sentCase.ShouldNotBeNull();
        AssertLegalCaseMapped(sentCase, view);
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> deserialised from the response body.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Insufficient account balance";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.PaymentRequired)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage })));
        var view = TilbagoFakers.LegalCaseFaker.Generate();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _apiClient.CaseService.CreateLegalPersonCaseAsync(view));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies a GET to <c>/api/v1/case/{id}/status</c> is dispatched and the response body is deserialized into the
    ///     fully-populated <see cref="CaseStatusView" />.
    /// </summary>
    [Test]
    public async Task GetStatusAsync_OnSuccess_DeserializesAndReturnsStatusView()
    {
        const string caseId = "abc-123";
        var expectedStatus = new CaseStatusView
        {
            StatusCode = "002",
            Description = "User has to review the case before submit",
            ESchKgCode = "0001"
        };
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case/{caseId}/status").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedStatus)));

        var status = await _apiClient.CaseService.GetStatusAsync(caseId);

        status.ShouldNotBeNull();
        status.StatusCode.ShouldBe(expectedStatus.StatusCode);
        status.Description.ShouldBe(expectedStatus.Description);
        status.ESchKgCode.ShouldBe(expectedStatus.ESchKgCode);

        var entries = Server.LogEntries.ToList();
        entries.Count.ShouldBe(1);
        var request = entries[0].RequestMessage!;
        request.Method.ShouldBe("GET");
        request.AbsolutePath.ShouldBe($"{ApiPathPrefix}/case/{caseId}/status");
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> deserialised from the response body.
    /// </summary>
    [Test]
    public async Task GetStatusAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Case not found";
        const string caseId = "missing";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case/{caseId}/status").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.InternalServerError)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage })));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _apiClient.CaseService.GetStatusAsync(caseId));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies the attachment id from the JSON body of a 200 response is parsed and returned to the caller and that
    ///     the request reaches WireMock as a PUT to <c>/api/v1/case/{id}/attachment</c>.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_OnSuccess_ReturnsAttachmentIdAndPutsToAttachmentEndpoint()
    {
        const string caseId = "case-77";
        const string expectedAttachmentId = "attachment-7";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case/{caseId}/attachment").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new AddAttachmentResultView
                    { AttachmentId = expectedAttachmentId })));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        var result = await _apiClient.CaseService.AddAttachmentAsync(caseId, "report.pdf", stream);

        result.ShouldBe(expectedAttachmentId);

        var entries = Server.LogEntries.ToList();
        entries.Count.ShouldBe(1);
        var request = entries[0].RequestMessage!;
        request.Method.ShouldBe("PUT");
        request.AbsolutePath.ShouldBe($"{ApiPathPrefix}/case/{caseId}/attachment");
    }

    /// <summary>
    ///     Asserts the documented quirk at the HTTP wire level: <see cref="CaseService.AddAttachmentAsync" /> strips the
    ///     <c>Content-Type</c> header from the multipart payload (Tilbago returns 500 if it is present) while keeping the
    ///     <c>Content-Disposition</c> header that carries the file name.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_DoesNotSendContentTypeHeaderOnTheWire()
    {
        const string caseId = "case-1";
        const string fileName = "report.pdf";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case/{caseId}/attachment").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new AddAttachmentResultView { AttachmentId = "att-1" })));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        await _apiClient.CaseService.AddAttachmentAsync(caseId, fileName, stream);

        var entries = Server.LogEntries.ToList();
        entries.Count.ShouldBe(1);
        var request = entries[0].RequestMessage!;
        var headers = request.Headers;
        headers.ShouldNotBeNull();

        var headerKeys = headers.Keys.Select(k => k.ToLowerInvariant()).ToList();
        headerKeys.ShouldNotContain("content-type",
            "Tilbago returns 500 when the Content-Type header is present on the multipart request — "
            + "CaseService.AddAttachmentAsync must remove it before sending.");

        headerKeys.ShouldContain("content-disposition",
            "the Content-Disposition header carrying the file name must remain on the multipart request.");
        var contentDispositionKey = headers.Keys.First(k => string.Equals(k, "Content-Disposition",
            StringComparison.OrdinalIgnoreCase));
        headers[contentDispositionKey].ShouldContain(v => v.Contains(fileName));
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> deserialised from the response body.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Invalid api_key";
        const string caseId = "case";
        Server
            .Given(Request.Create().WithPath($"{ApiPathPrefix}/case/{caseId}/attachment").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.Unauthorized)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage })));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _apiClient.CaseService.AddAttachmentAsync(caseId, "f.pdf", stream));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Asserts every property of a <see cref="CreateNaturalPersonCaseView" /> is mapped onto the corresponding
    ///     <see cref="Case" /> wire payload.
    /// </summary>
    private static void AssertNaturalCaseMapped(Case sentCase, CreateNaturalPersonCaseView view)
    {
        sentCase.ExternalRef.ShouldBe(view.ExternalRef);
        sentCase.CertificateOfLoss.ShouldBe(view.CertificateOfLoss);
        sentCase.PayeeReference.ShouldBe(view.PayeeReference);
        sentCase.SourceRefKey.ShouldBe(view.SourceRefKey);
        sentCase.SourceRefEmail.ShouldBe(view.SourceRefEmail);

        sentCase.ResponsiblePerson.ShouldNotBeNull();
        sentCase.ResponsiblePerson.Email.ShouldBe(view.ResponsiblePerson!.Email);

        sentCase.SubsidiaryClaims.ShouldNotBeNull();
        sentCase.SubsidiaryClaims.Count.ShouldBe(view.SubsidiaryClaims!.Count);
        for (var i = 0; i < view.SubsidiaryClaims.Count; i++)
            AssertClaimMapped(sentCase.SubsidiaryClaims[i], view.SubsidiaryClaims[i]);

        AssertCreditorMapped(sentCase.Creditor, view.Creditor);
        AssertNaturalDebtorMapped(sentCase.Debtor, view.Debtor);
        AssertClaimMapped(sentCase.Claim, view.Claim);
    }

    /// <summary>
    ///     Asserts every property of a <see cref="CreateLegalPersonCaseView" /> is mapped onto the corresponding
    ///     <see cref="Case" /> wire payload.
    /// </summary>
    private static void AssertLegalCaseMapped(Case sentCase, CreateLegalPersonCaseView view)
    {
        sentCase.ExternalRef.ShouldBe(view.ExternalRef);
        sentCase.CertificateOfLoss.ShouldBe(view.CertificateOfLoss);
        sentCase.PayeeReference.ShouldBe(view.PayeeReference);
        sentCase.SourceRefKey.ShouldBe(view.SourceRefKey);
        sentCase.SourceRefEmail.ShouldBe(view.SourceRefEmail);

        sentCase.ResponsiblePerson.ShouldNotBeNull();
        sentCase.ResponsiblePerson.Email.ShouldBe(view.ResponsiblePerson!.Email);

        sentCase.SubsidiaryClaims.ShouldNotBeNull();
        sentCase.SubsidiaryClaims.Count.ShouldBe(view.SubsidiaryClaims!.Count);
        for (var i = 0; i < view.SubsidiaryClaims.Count; i++)
            AssertClaimMapped(sentCase.SubsidiaryClaims[i], view.SubsidiaryClaims[i]);

        AssertCreditorMapped(sentCase.Creditor, view.Creditor);
        AssertLegalDebtorMapped(sentCase.Debtor, view.Debtor);
        AssertClaimMapped(sentCase.Claim, view.Claim);
    }

    /// <summary>
    ///     Asserts every settable property on a <see cref="DebtorNaturalPersonView" /> is reflected on the wire-side
    ///     <see cref="Debtor" />, including phone numbers, fax, title and birth name.
    /// </summary>
    private static void AssertNaturalDebtorMapped(Debtor? actual, DebtorNaturalPersonView expected)
    {
        actual.ShouldNotBeNull();
        actual.ExternalRef.ShouldBe(expected.ExternalRef);
        actual.Name.ShouldBe(expected.Name);
        actual.Surname.ShouldBe(expected.Surname);
        actual.Sex.ShouldBe(expected.Sex);
        actual.DateOfBirth.ShouldBe(expected.DateOfBirth);
        actual.BirthName.ShouldBe(expected.BirthName);
        actual.Title.ShouldBe(expected.Title);
        actual.Nationality.ShouldBe(expected.Nationality);
        actual.PreferredLanguage.ShouldBe(expected.PreferredLanguage);
        actual.EMail.ShouldBe(expected.EMail);
        actual.Phone1.ShouldBe(expected.Phone1);
        actual.Phone2.ShouldBe(expected.Phone2);
        actual.Phone3.ShouldBe(expected.Phone3);
        actual.Fax.ShouldBe(expected.Fax);
        AssertAddressMapped(actual.Address, expected.Address);
    }

    /// <summary>
    ///     Asserts every settable property on a <see cref="DebtorLegalPersonView" /> is reflected on the wire-side
    ///     <see cref="Debtor" />, including <c>NameAddon</c>, phone numbers and fax.
    /// </summary>
    private static void AssertLegalDebtorMapped(Debtor? actual, DebtorLegalPersonView expected)
    {
        actual.ShouldNotBeNull();
        actual.ExternalRef.ShouldBe(expected.ExternalRef);
        actual.Company.ShouldBe(expected.Company);
        actual.CompanyUid.ShouldBe(expected.CompanyUid);
        actual.NameAddon.ShouldBe(expected.NameAddon);
        actual.LegalSeat.ShouldBe(expected.LegalSeat);
        actual.ContactPerson.ShouldBe(expected.ContactPerson);
        actual.IsRegistered.ShouldBe(expected.IsRegistered);
        actual.PreferredLanguage.ShouldBe(expected.PreferredLanguage);
        actual.EMail.ShouldBe(expected.EMail);
        actual.Phone1.ShouldBe(expected.Phone1);
        actual.Phone2.ShouldBe(expected.Phone2);
        actual.Phone3.ShouldBe(expected.Phone3);
        actual.Fax.ShouldBe(expected.Fax);
        AssertAddressMapped(actual.Address, expected.Address);
    }

    /// <summary>
    ///     Asserts the <see cref="Address" /> on the wire matches the source-of-truth address property-by-property.
    /// </summary>
    private static void AssertAddressMapped(Address? actual, Address? expected)
    {
        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();
        actual.Zip.ShouldBe(expected.Zip);
        actual.Pob.ShouldBe(expected.Pob);
        actual.City.ShouldBe(expected.City);
        actual.Street.ShouldBe(expected.Street);
        actual.StreetNumber.ShouldBe(expected.StreetNumber);
    }

    /// <summary>
    ///     Asserts every property of a <see cref="Claim" /> survives the JSON round trip onto the wire payload.
    /// </summary>
    private static void AssertClaimMapped(Claim? actual, Claim expected)
    {
        actual.ShouldNotBeNull();
        actual.ExternalRef.ShouldBe(expected.ExternalRef);
        actual.Amount.ShouldBe(expected.Amount);
        actual.Reason.ShouldBe(expected.Reason);
        actual.InterestDateFrom.ShouldBe(expected.InterestDateFrom);
        actual.InterestRate.ShouldBe(expected.InterestRate);
        actual.CollocationClass.ShouldBe(expected.CollocationClass);
    }

    /// <summary>
    ///     Asserts every property of a <see cref="Creditor" /> survives the JSON round trip onto the wire payload —
    ///     including <c>TradeRegisterUrl</c> and <c>Iban</c> which are unique to <see cref="Creditor" />.
    /// </summary>
    private static void AssertCreditorMapped(Creditor? actual, Creditor? expected)
    {
        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();
        actual.ExternalRef.ShouldBe(expected.ExternalRef);
        actual.TradeRegisterUrl.ShouldBe(expected.TradeRegisterUrl);
        actual.Iban.ShouldBe(expected.Iban);
        actual.Name.ShouldBe(expected.Name);
        actual.Surname.ShouldBe(expected.Surname);
        actual.NameAddon.ShouldBe(expected.NameAddon);
        actual.Title.ShouldBe(expected.Title);
        actual.BirthName.ShouldBe(expected.BirthName);
        actual.Sex.ShouldBe(expected.Sex);
        actual.DateOfBirth.ShouldBe(expected.DateOfBirth);
        actual.Nationality.ShouldBe(expected.Nationality);
        actual.Company.ShouldBe(expected.Company);
        actual.CompanyUid.ShouldBe(expected.CompanyUid);
        actual.LegalSeat.ShouldBe(expected.LegalSeat);
        actual.ContactPerson.ShouldBe(expected.ContactPerson);
        actual.IsRegistered.ShouldBe(expected.IsRegistered);
        actual.PreferredLanguage.ShouldBe(expected.PreferredLanguage);
        actual.EMail.ShouldBe(expected.EMail);
        actual.Phone1.ShouldBe(expected.Phone1);
        actual.Phone2.ShouldBe(expected.Phone2);
        actual.Phone3.ShouldBe(expected.Phone3);
        actual.Fax.ShouldBe(expected.Fax);
        AssertAddressMapped(actual.Address, expected.Address);
    }
}
