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
///     Base class for end-to-end tests requiring live Tilbago API credentials. Provides a configured
///     <see cref="TilbagoApiClient" /> per test and a LIFO cleanup stack for resources created during the test body.
/// </summary>
public abstract class E2eTestBase
{
    private readonly Stack<Func<Task>> _cleanupStack = new();

    /// <summary>
    ///     Initialised API client for use in E2E tests.
    /// </summary>
    protected TilbagoApiClient TilbagoApiClient { get; private set; } = null!;

    /// <summary>
    ///     Reads credentials from environment variables and initialises the API client.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a required environment variable is absent.</exception>
    [SetUp]
    public void SetUpBase()
    {
        var apiKey = Environment.GetEnvironmentVariable("TilbagoApiNet__ApiKey")
                     ?? throw new InvalidOperationException("Missing environment variable: TilbagoApiNet__ApiKey");
        var baseUri = Environment.GetEnvironmentVariable("TilbagoApiNet__BaseUri")
                      ?? throw new InvalidOperationException("Missing environment variable: TilbagoApiNet__BaseUri");

        TilbagoApiClient =
            new TilbagoApiClient(new TilbagoConnectionHandler(new TilbagoConfiguration(apiKey, baseUri)));
    }

    /// <summary>
    ///     Registers a cleanup callback to run during <see cref="TearDownBase" />. Callbacks execute in LIFO order so
    ///     dependencies created later in the test are torn down before the resources they depend on.
    /// </summary>
    /// <param name="cleanup">Asynchronous cleanup action.</param>
    protected void RegisterCleanup(Func<Task> cleanup)
    {
        _cleanupStack.Push(cleanup);
    }

    /// <summary>
    ///     Runs all registered cleanup callbacks in LIFO order (best-effort; exceptions are swallowed so a failing
    ///     cleanup does not mask earlier failures or block subsequent cleanups), then disposes the API client.
    /// </summary>
    [TearDown]
    public async Task TearDownBase()
    {
        while (_cleanupStack.TryPop(out var cleanup))
            try
            {
                await cleanup();
            }
            catch
            {
                // Cleanup is best-effort; swallow to allow remaining cleanups to run.
            }

        // Guard against SetUpBase having failed before assigning the client (e.g. missing env vars).
        TilbagoApiClient?.Dispose();
    }
}