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

using Shouldly;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="TilbagoConfiguration" />.
/// </summary>
[TestFixture]
public class TilbagoConfigurationTests
{
    /// <summary>
    ///     Verifies the constructor stores the API key correctly.
    /// </summary>
    [Test]
    public void Constructor_WithValidApiKey_StoresApiKey()
    {
        const string expectedApiKey = "test-api-key";

        var config = new TilbagoConfiguration(expectedApiKey, "https://example.com/");

        config.ApiKey.ShouldBe(expectedApiKey);
    }

    /// <summary>
    ///     Verifies the constructor stores the base URI correctly.
    /// </summary>
    [Test]
    public void Constructor_WithValidBaseUri_StoresBaseUri()
    {
        const string expectedBaseUri = "https://example.com/api/";

        var config = new TilbagoConfiguration("test-key", expectedBaseUri);

        config.BaseUri.ShouldBe(expectedBaseUri);
    }

    /// <summary>
    ///     Verifies the ApiKey property can be updated after construction.
    /// </summary>
    [Test]
    public void ApiKey_SetNewValue_UpdatesProperty()
    {
        var config = new TilbagoConfiguration("initial-key", "https://example.com/");

        config.ApiKey = "updated-key";

        config.ApiKey.ShouldBe("updated-key");
    }

    /// <summary>
    ///     Verifies the BaseUri property can be updated after construction.
    /// </summary>
    [Test]
    public void BaseUri_SetNewValue_UpdatesProperty()
    {
        var config = new TilbagoConfiguration("test-key", "https://initial.com/");

        config.BaseUri = "https://updated.com/";

        config.BaseUri.ShouldBe("https://updated.com/");
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoConfiguration" /> implements <see cref="ITilbagoConfiguration" />.
    /// </summary>
    [Test]
    public void Configuration_ImplementsITilbagoConfiguration()
    {
        var config = new TilbagoConfiguration("test-key", "https://example.com/");

        config.ShouldBeAssignableTo<ITilbagoConfiguration>();
    }
}