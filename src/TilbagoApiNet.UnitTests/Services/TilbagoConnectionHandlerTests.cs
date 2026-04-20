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

using NSubstitute;
using Shouldly;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="TilbagoConnectionHandler" />.
/// </summary>
[TestFixture]
public class TilbagoConnectionHandlerTests
{
    /// <summary>
    ///     Creates a fresh mocked <see cref="ITilbagoConfiguration" /> before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _configuration = Substitute.For<ITilbagoConfiguration>();
    }

    private ITilbagoConfiguration _configuration = null!;

    /// <summary>
    ///     Verifies the constructor creates a non-null <see cref="HttpClient" />.
    /// </summary>
    [Test]
    public void Constructor_WithValidConfiguration_CreatesHttpClient()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("https://example.com/api");

        using var handler = new TilbagoConnectionHandler(_configuration);

        handler.Client.ShouldNotBeNull();
    }

    /// <summary>
    ///     Verifies the constructor sets <see cref="HttpClient.BaseAddress" /> from configuration, appending a trailing slash
    ///     when absent.
    /// </summary>
    [Test]
    public void Constructor_WithUriWithoutTrailingSlash_SetsBaseAddressWithTrailingSlash()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("https://example.com/api");

        using var handler = new TilbagoConnectionHandler(_configuration);

        handler.Client.BaseAddress.ShouldNotBeNull();
        handler.Client.BaseAddress!.ToString().ShouldBe("https://example.com/api/");
    }

    /// <summary>
    ///     Verifies the constructor does not double the trailing slash when already present in the URI.
    /// </summary>
    [Test]
    public void Constructor_WithUriAlreadyHavingTrailingSlash_DoesNotDoubleSlash()
    {
        const string baseUri = "https://example.com/api/";
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns(baseUri);

        using var handler = new TilbagoConnectionHandler(_configuration);

        handler.Client.BaseAddress!.ToString().ShouldBe(baseUri);
    }

    /// <summary>
    ///     Verifies the <c>api_key</c> default request header is set from configuration.
    /// </summary>
    [Test]
    public void Constructor_WithValidApiKey_AddsApiKeyRequestHeader()
    {
        const string apiKey = "my-secret-api-key";
        _configuration.ApiKey.Returns(apiKey);
        _configuration.BaseUri.Returns("https://example.com/api");

        using var handler = new TilbagoConnectionHandler(_configuration);

        handler.Client.DefaultRequestHeaders.TryGetValues("api_key", out var values).ShouldBeTrue();
        values!.ShouldContain(apiKey);
    }

    /// <summary>
    ///     Verifies the constructor throws when <see cref="ITilbagoConfiguration.BaseUri" /> is empty.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyBaseUri_ThrowsArgumentException()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns(string.Empty);

        Should.Throw<ArgumentException>(() => _ = new TilbagoConnectionHandler(_configuration));
    }

    /// <summary>
    ///     Verifies the constructor throws when <see cref="ITilbagoConfiguration.BaseUri" /> is whitespace.
    /// </summary>
    [Test]
    public void Constructor_WithWhitespaceBaseUri_ThrowsArgumentException()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("   ");

        Should.Throw<ArgumentException>(() => _ = new TilbagoConnectionHandler(_configuration));
    }

    /// <summary>
    ///     Verifies the constructor throws when <see cref="ITilbagoConfiguration.ApiKey" /> is empty.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
    {
        _configuration.ApiKey.Returns(string.Empty);
        _configuration.BaseUri.Returns("https://example.com/api");

        Should.Throw<ArgumentException>(() => _ = new TilbagoConnectionHandler(_configuration));
    }

    /// <summary>
    ///     Verifies the constructor throws when <see cref="ITilbagoConfiguration.ApiKey" /> is whitespace.
    /// </summary>
    [Test]
    public void Constructor_WithWhitespaceApiKey_ThrowsArgumentException()
    {
        _configuration.ApiKey.Returns("   ");
        _configuration.BaseUri.Returns("https://example.com/api");

        Should.Throw<ArgumentException>(() => _ = new TilbagoConnectionHandler(_configuration));
    }

    /// <summary>
    ///     Verifies the <see cref="TilbagoConnectionHandler.Client" /> property returns the same instance on repeated access.
    /// </summary>
    [Test]
    public void Client_AccessedMultipleTimes_ReturnsSameInstance()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("https://example.com/api");

        using var handler = new TilbagoConnectionHandler(_configuration);

        var first = handler.Client;
        var second = handler.Client;

        first.ShouldBeSameAs(second);
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoConnectionHandler" /> implements <see cref="ITilbagoConnectionHandler" />.
    /// </summary>
    [Test]
    public void Handler_ImplementsITilbagoConnectionHandler()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("https://example.com/api");

        using var handler = new TilbagoConnectionHandler(_configuration);

        handler.ShouldBeAssignableTo<ITilbagoConnectionHandler>();
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoConnectionHandler.Dispose" /> can be called without throwing.
    /// </summary>
    [Test]
    public void Dispose_CanBeCalledWithoutException()
    {
        _configuration.ApiKey.Returns("valid-api-key");
        _configuration.BaseUri.Returns("https://example.com/api");

        var handler = new TilbagoConnectionHandler(_configuration);

        Should.NotThrow(handler.Dispose);
    }
}