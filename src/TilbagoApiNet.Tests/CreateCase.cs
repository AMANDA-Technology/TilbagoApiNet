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

using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.Tests;

public class Tests
{
    private readonly ITilbagoApiClient _tilbagoApiClient;
    private const string FileName = "dummy.pdf";

    public Tests()
    {
        var apiKey = Environment.GetEnvironmentVariable("TilbagoApiNet__ApiKey") ?? throw new("Missing TilbagoApiNet__ApiKey");
        var baseUri = Environment.GetEnvironmentVariable("TilbagoApiNet__BaseUri") ?? throw new("Missing TilbagoApiNet__BaseUri");

        _tilbagoApiClient = new TilbagoApiClient(new TilbagoConnectionHandler(new TilbagoConfiguration(apiKey, baseUri)));
    }

    [SetUp]
    public void Setup()
    {
        // Nothing to do yet
    }

    [Test]
    public async Task GetCreatedCaseAndAddAttachmentForNaturalPerson()
    {
        Assert.That(_tilbagoApiClient, Is.Not.Null);

        var address = new Address
        {
            City = "Bern",
            Street = "Mainstreet",
            StreetNumber = "12",
            Zip = "3000"
        };

        var debtor = new DebtorNaturalPersonView
        {
            ExternalRef = Helpers.RandomString(12),
            Name = "Hans",
            Surname = "Muster",
            Address = address
        };

        var claim = new Claim
        {
            ExternalRef = Helpers.RandomString(12),
            Amount = 123,
            Reason = "didnt pay his bill",
            InterestDateFrom = "2020-12-12",
            InterestRate = "13",
            CollocationClass = "1"
        };

        var newCase = await _tilbagoApiClient.CaseService.CreateNaturalPersonCaseAsync(new()
        {
            ExternalRef = Helpers.RandomString(12),
            Debtor = debtor,
            Claim = claim,
            CertificateOfLoss = false
        });

        Assert.That(newCase, Is.Not.Null);

        var res = await _tilbagoApiClient.CaseService.GetStatusAsync(newCase ?? throw new());
        Assert.That(res, Is.Not.Null);

        var uploadRes = await _tilbagoApiClient.CaseService.AddAttachmentAsync(newCase, FileName, File.OpenRead(FileName));

        Assert.That(uploadRes, Is.Not.Null);
    }
}
