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
using System.Text.Json;
using Bogus;
using Shouldly;
using TilbagoApiNet.Abstractions.Enums;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Services;
using TilbagoApiNet.Services.Connectors;
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
        var view = NaturalCaseFaker.Generate();

        var result = await _apiClient.CaseService.CreateNaturalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies the request reaches WireMock as a PUT to <c>/api/v1/case</c>, with the api_key header populated and a
    ///     body whose JSON property names match the <c>[JsonPropertyName]</c> annotations on <see cref="Case" />,
    ///     <see cref="Debtor" /> and <see cref="Claim" />.
    /// </summary>
    [Test]
    public async Task CreateNaturalPersonCaseAsync_OnSuccess_SendsPutWithCamelCaseBodyAndApiKeyHeader()
    {
        var view = NaturalCaseFaker.Generate();
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

        var sentCase = JsonSerializer.Deserialize<Case>(body!);
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
        var view = NaturalCaseFaker.Generate();

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
        var view = LegalCaseFaker.Generate();

        var result = await _apiClient.CaseService.CreateLegalPersonCaseAsync(view);

        result.ShouldBe(expectedCaseId);
    }

    /// <summary>
    ///     Verifies the legal-person view's company-specific fields are mapped onto the wire payload using the camelCase
    ///     property names defined by <c>[JsonPropertyName]</c>.
    /// </summary>
    [Test]
    public async Task CreateLegalPersonCaseAsync_OnSuccess_SendsPutWithLegalPersonFieldsInBody()
    {
        var view = LegalCaseFaker.Generate();
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

        var sentCase = JsonSerializer.Deserialize<Case>(body!);
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
        var view = LegalCaseFaker.Generate();

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

        var headerKeys = headers!.Keys.Select(k => k.ToLowerInvariant()).ToList();
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
}
