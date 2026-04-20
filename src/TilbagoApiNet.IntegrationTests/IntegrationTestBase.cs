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

using WireMock.Server;

namespace TilbagoApiNet.IntegrationTests;

/// <summary>
///     Base class for integration tests that spins up and tears down a <see cref="WireMockServer" /> per test.
/// </summary>
public abstract class IntegrationTestBase
{
    /// <summary>
    ///     Running WireMock server for the current test.
    /// </summary>
    protected WireMockServer Server { get; private set; } = null!;

    /// <summary>
    ///     Starts a WireMock server before each test.
    /// </summary>
    [SetUp]
    public virtual void SetUp()
    {
        Server = WireMockServer.Start();
    }

    /// <summary>
    ///     Stops and disposes the WireMock server after each test.
    /// </summary>
    [TearDown]
    public virtual void TearDown()
    {
        Server?.Dispose();
    }
}