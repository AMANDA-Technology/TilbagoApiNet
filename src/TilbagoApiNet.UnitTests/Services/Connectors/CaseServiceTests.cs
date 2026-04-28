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
using System.Text;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Services.Connectors;
using TilbagoApiNet.TestHelpers;
using Case = TilbagoApiNet.Abstractions.Models.Case;

namespace TilbagoApiNet.UnitTests.Services.Connectors;

/// <summary>
///     Unit tests for <see cref="CaseService" />, isolating the connector from real HTTP via a stub
///     <see cref="HttpMessageHandler" /> wired into the substituted
///     <see cref="TilbagoApiNet.Interfaces.ITilbagoConnectionHandler" />.
/// </summary>
[TestFixture]
public class CaseServiceTests : ServiceTestBase
{
    private static readonly Uri BaseAddress = new("https://tilbago.test/api/");

    private CaseService _service = null!;
    private StubHttpMessageHandler _handler = null!;
    private HttpClient _httpClient = null!;

    /// <summary>
    ///     Wires a fresh <see cref="StubHttpMessageHandler" /> behind a real <see cref="HttpClient" /> and registers it on
    ///     the substituted <see cref="TilbagoApiNet.Interfaces.ITilbagoConnectionHandler" /> so each test sees a clean
    ///     transport.
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();
        _handler = new StubHttpMessageHandler();
        _httpClient = new HttpClient(_handler) { BaseAddress = BaseAddress };
        ConnectionHandler.Client.Returns(_httpClient);
        _service = new CaseService(ConnectionHandler);
    }

    /// <summary>
    ///     Disposes the per-test <see cref="HttpClient" /> and stub <see cref="HttpMessageHandler" />.
    /// </summary>
    public override void TearDown()
    {
        _httpClient.Dispose();
        _handler.Dispose();
        base.TearDown();
    }

    /// <summary>
    ///     Verifies the case id from the JSON body of a 200 response is returned to the caller.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_ReturnsCaseIdFromResponse()
    {
        const string expectedCaseId = "case-natural-42";
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = expectedCaseId }));
        var view = TilbagoFakers.NaturalCaseFaker.Generate();

        var result = await _service.CreateNaturalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies that the request is a PUT to <c>case</c> and that every property of the natural-person view —
    ///     required and optional — is mapped onto the wire payload, including <c>Fax</c>, <c>Phone2</c>, <c>Phone3</c>,
    ///     <c>Title</c>, <c>BirthName</c>, <c>PreferredLanguage</c>, <c>PayeeReference</c>, <c>ResponsiblePerson</c>,
    ///     <c>SubsidiaryClaims</c>, <c>SourceRefEmail</c>, <c>SourceRefKey</c> and <c>Creditor</c>.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_SendsPutToCaseEndpointWithMappedBody()
    {
        var view = TilbagoFakers.NaturalCaseFaker.Generate();
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" }));

        await _service.CreateNaturalPersonCaseAsync(view);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case");

        _handler.LastRequestBody.ShouldNotBeNullOrWhiteSpace();
        var sentCase = JsonSerializer.Deserialize<Case>(_handler.LastRequestBody!);
        sentCase.ShouldNotBeNull();
        AssertNaturalCaseMapped(sentCase, view);
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> from the response body.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Invalid api_key";
        _handler.SetResponse(HttpStatusCode.Unauthorized,
            JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage }));
        var view = TilbagoFakers.NaturalCaseFaker.Generate();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _service.CreateNaturalPersonCaseAsync(view));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies the case id from the JSON body of a 200 response is returned to the caller.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_ReturnsCaseIdFromResponse()
    {
        const string expectedCaseId = "case-legal-99";
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = expectedCaseId }));
        var view = TilbagoFakers.LegalCaseFaker.Generate();

        var result = await _service.CreateLegalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies that the request is a PUT to <c>case</c> and that every property of the legal-person view —
    ///     required and optional — is mapped onto the wire payload, including <c>NameAddon</c>, <c>Fax</c>,
    ///     <c>Phone2</c>, <c>Phone3</c>, <c>PreferredLanguage</c>, <c>PayeeReference</c>, <c>ResponsiblePerson</c>,
    ///     <c>SubsidiaryClaims</c>, <c>SourceRefEmail</c>, <c>SourceRefKey</c> and <c>Creditor</c>.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_SendsPutToCaseEndpointWithMappedBody()
    {
        var view = TilbagoFakers.LegalCaseFaker.Generate();
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" }));

        await _service.CreateLegalPersonCaseAsync(view);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case");

        _handler.LastRequestBody.ShouldNotBeNullOrWhiteSpace();
        var sentCase = JsonSerializer.Deserialize<Case>(_handler.LastRequestBody!);
        sentCase.ShouldNotBeNull();
        AssertLegalCaseMapped(sentCase, view);
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> from the response body.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Insufficient account balance";
        _handler.SetResponse(HttpStatusCode.PaymentRequired,
            JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage }));
        var view = TilbagoFakers.LegalCaseFaker.Generate();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _service.CreateLegalPersonCaseAsync(view));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies a GET to <c>case/{id}/status</c> is dispatched and the response body is deserialized into
    ///     <see cref="CaseStatusView" />.
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
        _handler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedStatus));

        var status = await _service.GetStatusAsync(caseId);

        status.ShouldNotBeNull();
        status.StatusCode.ShouldBe(expectedStatus.StatusCode);
        status.Description.ShouldBe(expectedStatus.Description);
        status.ESchKgCode.ShouldBe(expectedStatus.ESchKgCode);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case/{caseId}/status");
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> from the response body.
    /// </summary>
    [Test]
    public async Task GetStatusAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Case not found";
        _handler.SetResponse(HttpStatusCode.InternalServerError,
            JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage }));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _service.GetStatusAsync("any"));

        ex.Message.ShouldBe(expectedMessage);
    }

    /// <summary>
    ///     Verifies the attachment id from the JSON body of a 200 response is returned to the caller and the request
    ///     targets the documented <c>case/{id}/attachment</c> endpoint via PUT.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_OnSuccess_ReturnsAttachmentIdAndPutsToAttachmentEndpoint()
    {
        const string caseId = "case-77";
        const string expectedAttachmentId = "attachment-7";
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new AddAttachmentResultView { AttachmentId = expectedAttachmentId }));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        var result = await _service.AddAttachmentAsync(caseId, "report.pdf", stream);

        result.ShouldBe(expectedAttachmentId);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case/{caseId}/attachment");
    }

    /// <summary>
    ///     Asserts the documented quirk that <see cref="CaseService.AddAttachmentAsync" /> strips the <c>Content-Type</c>
    ///     header from the multipart payload — the Tilbago server returns 500 if the header is present. The
    ///     <c>Content-Disposition</c> header carrying the file name must remain.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_RemovesContentTypeHeaderFromMultipartContent()
    {
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new AddAttachmentResultView { AttachmentId = "att-1" }));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        await _service.AddAttachmentAsync("case-1", "report.pdf", stream);

        _handler.LastContentHadContentTypeHeader.ShouldBeFalse(
            "Tilbago returns 500 when the Content-Type header is present on the multipart request — "
            + "CaseService.AddAttachmentAsync must remove it before sending.");
        _handler.LastContentHadContentDispositionHeader.ShouldBeTrue(
            "the Content-Disposition header carrying the file name must remain on the multipart request.");
    }

    /// <summary>
    ///     Verifies a non-success response surfaces an <see cref="InvalidOperationException" /> carrying the
    ///     <see cref="ErrorModel.Message" /> from the response body.
    /// </summary>
    [Test]
    public async Task AddAttachmentAsync_OnErrorResponse_ThrowsInvalidOperationExceptionWithErrorMessage()
    {
        const string expectedMessage = "Invalid api_key";
        _handler.SetResponse(HttpStatusCode.Unauthorized,
            JsonSerializer.Serialize(new ErrorModel { Message = expectedMessage }));
        using var stream = new MemoryStream("dummy file"u8.ToArray());

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _service.AddAttachmentAsync("case", "f.pdf", stream));

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
        sentCase.ResponsiblePerson!.Email.ShouldBe(view.ResponsiblePerson!.Email);

        sentCase.SubsidiaryClaims.ShouldNotBeNull();
        sentCase.SubsidiaryClaims!.Count.ShouldBe(view.SubsidiaryClaims!.Count);
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
        sentCase.ResponsiblePerson!.Email.ShouldBe(view.ResponsiblePerson!.Email);

        sentCase.SubsidiaryClaims.ShouldNotBeNull();
        sentCase.SubsidiaryClaims!.Count.ShouldBe(view.SubsidiaryClaims!.Count);
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
        actual!.ExternalRef.ShouldBe(expected.ExternalRef);
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
        actual!.ExternalRef.ShouldBe(expected.ExternalRef);
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
        actual!.Zip.ShouldBe(expected!.Zip);
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
        actual!.ExternalRef.ShouldBe(expected.ExternalRef);
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
        actual!.ExternalRef.ShouldBe(expected!.ExternalRef);
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

    /// <summary>
    ///     Test double for <see cref="HttpMessageHandler" /> that captures the outgoing request and serves a configured
    ///     response. Captures content-header presence as booleans up-front because the request content may be disposed by
    ///     the caller (for example via a <c>using</c> block on a <see cref="MultipartContent" />) before the test asserts.
    /// </summary>
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        /// <summary>The most recent request observed by <see cref="SendAsync" />, or <c>null</c> when none.</summary>
        public HttpRequestMessage? LastRequest { get; private set; }

        /// <summary>The body of the most recent request, captured before the response is returned.</summary>
        public string? LastRequestBody { get; private set; }

        /// <summary>True when the captured request content carried a <c>Content-Type</c> header.</summary>
        public bool LastContentHadContentTypeHeader { get; private set; }

        /// <summary>True when the captured request content carried a <c>Content-Disposition</c> header.</summary>
        public bool LastContentHadContentDispositionHeader { get; private set; }

        /// <summary>
        ///     Replaces the canned response returned by <see cref="SendAsync" />. The previous response is disposed.
        /// </summary>
        /// <param name="statusCode">HTTP status code for the canned response.</param>
        /// <param name="body">JSON body for the canned response.</param>
        public void SetResponse(HttpStatusCode statusCode, string body)
        {
            _response.Dispose();
            _response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
            {
                LastContentHadContentTypeHeader = request.Content.Headers.Contains("Content-Type");
                LastContentHadContentDispositionHeader = request.Content.Headers.Contains("Content-Disposition");
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return _response;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) _response.Dispose();

            base.Dispose(disposing);
        }
    }
}
