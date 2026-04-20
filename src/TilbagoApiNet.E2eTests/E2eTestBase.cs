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

using TilbagoApiNet.Services;

namespace TilbagoApiNet.E2eTests;

/// <summary>
///     Base class for end-to-end tests requiring live Tilbago API credentials.
/// </summary>
public abstract class E2eTestBase
{
    /// <summary>
    ///     Initialised API client for use in E2E tests.
    /// </summary>
    protected TilbagoApiClient ApiClient { get; private set; } = null!;

    /// <summary>
    ///     Reads credentials from environment variables and initialises the API client.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a required environment variable is absent.</exception>
    [SetUp]
    public virtual void SetUp()
    {
        var apiKey = Environment.GetEnvironmentVariable("TilbagoApiNet__ApiKey")
                     ?? throw new InvalidOperationException("Missing environment variable: TilbagoApiNet__ApiKey");
        var baseUri = Environment.GetEnvironmentVariable("TilbagoApiNet__BaseUri")
                      ?? throw new InvalidOperationException("Missing environment variable: TilbagoApiNet__BaseUri");

        ApiClient = new TilbagoApiClient(new TilbagoConnectionHandler(new TilbagoConfiguration(apiKey, baseUri)));
    }

    /// <summary>
    ///     Disposes the API client after each test.
    /// </summary>
    [TearDown]
    public virtual void TearDown()
    {
        ApiClient?.Dispose();
    }
}