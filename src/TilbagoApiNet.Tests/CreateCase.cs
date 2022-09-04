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
    private readonly IRestService _restService;

    public Tests()
    {
        var apiKey = Environment.GetEnvironmentVariable("TilbagoApiNet__ApiKey") ??
                     throw new("Missing TilbagoApiNet__ApiKey");
        var baseUri = Environment.GetEnvironmentVariable("TilbagoApiNet__BaseUri") ??
                      throw new("Missing TilbagoApiNet__BaseUri");

        _restService = new RestService(new ConnectionHandler(new Configuration(apiKey, baseUri)));
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GetCreatedCase()
    {
        Assert.That(_restService, Is.Not.Null);

        var address = new Address
        {
            City = "Bern",
            Street = "Mainstreet",
            StreetNumber = "12",
            Zip = "3000"
        };

        var debtor = new DebtorNaturalPersonView(
            Helpers.RandomString(12),
            "Hans",
            "Muster",
            address);

        var claim = new Claim(
            Helpers.RandomString(12),
            123,
            "didnt pay his bill",
            "2020-12-12",
            "13",
            "1");

        var newCase = await _restService.CaseService.CreateNaturalPersonCaseAsync(
            new(
                Helpers.RandomString(12),
                false,
                debtor,
                claim
            ));

        Assert.That(newCase, Is.Not.Null);

        var res = await _restService.CaseService.GetStatusAsync(newCase ?? throw new());
        Assert.That(res, Is.Not.Null);
    }
}
