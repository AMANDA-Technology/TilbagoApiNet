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

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Bogus;
using NSubstitute;
using Shouldly;
using TilbagoApiNet.Abstractions.Enums;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Services.Connectors;
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

    private static readonly Faker<Address> AddressFaker = new Faker<Address>()
        .RuleFor(x => x.Zip, f => f.Address.ZipCode("####"))
        .RuleFor(x => x.City, f => f.Address.City())
        .RuleFor(x => x.Street, f => f.Address.StreetName())
        .RuleFor(x => x.StreetNumber, f => f.Address.BuildingNumber())
        .RuleFor(x => x.Pob, f => f.Random.Replace("####"));

    private static readonly Faker<Claim> ClaimFaker = new Faker<Claim>()
        .CustomInstantiator(f => new Claim
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Amount = f.Random.Int(100, 100_000_00),
            Reason = f.Lorem.Sentence(),
            InterestDateFrom = f.Date.Past().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            InterestRate = f.Random.Decimal(0, 10).ToString("F2", CultureInfo.InvariantCulture),
            CollocationClass = f.PickRandom("1", "2", "3")
        });

    private static readonly Faker<DebtorNaturalPersonView> DebtorNaturalFaker = new Faker<DebtorNaturalPersonView>()
        .CustomInstantiator(f => new DebtorNaturalPersonView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Name = f.Name.FirstName(),
            Surname = f.Name.LastName(),
            Sex = f.PickRandom<Sex>(),
            DateOfBirth = f.Date.Past(60).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Address = AddressFaker.Generate(),
            EMail = f.Internet.Email(),
            Phone1 = f.Phone.PhoneNumber(),
            Nationality = "CH",
            PreferredLanguage = Language.de
        });

    private static readonly Faker<DebtorLegalPersonView> DebtorLegalFaker = new Faker<DebtorLegalPersonView>()
        .CustomInstantiator(f => new DebtorLegalPersonView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            Company = f.Company.CompanyName(),
            CompanyUid = $"CHE-{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}.{f.Random.Int(100, 999)}",
            ContactPerson = f.Name.FullName(),
            IsRegistered = true,
            LegalSeat = f.Address.City(),
            Address = AddressFaker.Generate(),
            EMail = f.Internet.Email(),
            Phone1 = f.Phone.PhoneNumber(),
            PreferredLanguage = Language.de
        });

    private static readonly Faker<CreateNaturalPersonCaseView> NaturalCaseFaker =
        new Faker<CreateNaturalPersonCaseView>()
            .CustomInstantiator(f => new CreateNaturalPersonCaseView
            {
                ExternalRef = f.Random.AlphaNumeric(12),
                CertificateOfLoss = false,
                Debtor = DebtorNaturalFaker.Generate(),
                Claim = ClaimFaker.Generate()
            });

    private static readonly Faker<CreateLegalPersonCaseView> LegalCaseFaker = new Faker<CreateLegalPersonCaseView>()
        .CustomInstantiator(f => new CreateLegalPersonCaseView
        {
            ExternalRef = f.Random.AlphaNumeric(12),
            CertificateOfLoss = false,
            Debtor = DebtorLegalFaker.Generate(),
            Claim = ClaimFaker.Generate()
        });

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
        var view = NaturalCaseFaker.Generate();

        var result = await _service.CreateNaturalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies that the request is a PUT to <c>case</c> and that the natural-person view's fields are mapped onto the
    ///     wire payload.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_SendsPutToCaseEndpointWithMappedBody()
    {
        var view = NaturalCaseFaker.Generate();
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" }));

        await _service.CreateNaturalPersonCaseAsync(view);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case");

        _handler.LastRequestBody.ShouldNotBeNullOrWhiteSpace();
        var sentCase = JsonSerializer.Deserialize<Case>(_handler.LastRequestBody!);
        sentCase.ShouldNotBeNull();
        sentCase.ExternalRef.ShouldBe(view.ExternalRef);
        sentCase.CertificateOfLoss.ShouldBe(view.CertificateOfLoss);
        sentCase.Debtor.ShouldNotBeNull();
        sentCase.Debtor!.ExternalRef.ShouldBe(view.Debtor.ExternalRef);
        sentCase.Debtor.Name.ShouldBe(view.Debtor.Name);
        sentCase.Debtor.Surname.ShouldBe(view.Debtor.Surname);
        sentCase.Debtor.DateOfBirth.ShouldBe(view.Debtor.DateOfBirth);
        sentCase.Debtor.Sex.ShouldBe(view.Debtor.Sex);
        sentCase.Debtor.Nationality.ShouldBe(view.Debtor.Nationality);
        sentCase.Debtor.EMail.ShouldBe(view.Debtor.EMail);
        sentCase.Debtor.Phone1.ShouldBe(view.Debtor.Phone1);
        sentCase.Debtor.Address.ShouldNotBeNull();
        sentCase.Debtor.Address!.City.ShouldBe(view.Debtor.Address.City);
        sentCase.Claim.ShouldNotBeNull();
        sentCase.Claim!.ExternalRef.ShouldBe(view.Claim.ExternalRef);
        sentCase.Claim.Amount.ShouldBe(view.Claim.Amount);
        sentCase.Claim.Reason.ShouldBe(view.Claim.Reason);
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
        var view = NaturalCaseFaker.Generate();

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
        var view = LegalCaseFaker.Generate();

        var result = await _service.CreateLegalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies that the request is a PUT to <c>case</c> and that the legal-person view's company fields are mapped
    ///     onto the wire payload.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_SendsPutToCaseEndpointWithMappedBody()
    {
        var view = LegalCaseFaker.Generate();
        _handler.SetResponse(HttpStatusCode.OK,
            JsonSerializer.Serialize(new CaseCreateResultView { CaseId = "ignored" }));

        await _service.CreateLegalPersonCaseAsync(view);

        _handler.LastRequest.ShouldNotBeNull();
        _handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        _handler.LastRequest.RequestUri!.AbsoluteUri.ShouldBe($"{BaseAddress}case");

        _handler.LastRequestBody.ShouldNotBeNullOrWhiteSpace();
        var sentCase = JsonSerializer.Deserialize<Case>(_handler.LastRequestBody!);
        sentCase.ShouldNotBeNull();
        sentCase.ExternalRef.ShouldBe(view.ExternalRef);
        sentCase.CertificateOfLoss.ShouldBe(view.CertificateOfLoss);
        sentCase.Debtor.ShouldNotBeNull();
        sentCase.Debtor!.ExternalRef.ShouldBe(view.Debtor.ExternalRef);
        sentCase.Debtor.Company.ShouldBe(view.Debtor.Company);
        sentCase.Debtor.CompanyUid.ShouldBe(view.Debtor.CompanyUid);
        sentCase.Debtor.LegalSeat.ShouldBe(view.Debtor.LegalSeat);
        sentCase.Debtor.ContactPerson.ShouldBe(view.Debtor.ContactPerson);
        sentCase.Debtor.IsRegistered.ShouldBe(view.Debtor.IsRegistered);
        sentCase.Debtor.EMail.ShouldBe(view.Debtor.EMail);
        sentCase.Debtor.Phone1.ShouldBe(view.Debtor.Phone1);
        sentCase.Debtor.Address.ShouldNotBeNull();
        sentCase.Debtor.Address!.City.ShouldBe(view.Debtor.Address.City);
        sentCase.Claim.ShouldNotBeNull();
        sentCase.Claim!.ExternalRef.ShouldBe(view.Claim.ExternalRef);
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
        var view = LegalCaseFaker.Generate();

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
