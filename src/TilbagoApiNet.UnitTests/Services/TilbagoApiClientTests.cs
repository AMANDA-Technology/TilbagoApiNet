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
using TilbagoApiNet.Interfaces.Connectors;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="TilbagoApiClient" />.
/// </summary>
[TestFixture]
public class TilbagoApiClientTests : ServiceTestBase
{
    /// <summary>
    ///     Verifies the constructor initialises <see cref="TilbagoApiClient.CaseService" /> to a non-null value.
    /// </summary>
    [Test]
    public void Constructor_WithConnectionHandler_InitializesCaseService()
    {
        var client = new TilbagoApiClient(ConnectionHandler);

        client.CaseService.ShouldNotBeNull();
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoApiClient.CaseService" /> exposes an <see cref="ICaseService" /> instance.
    /// </summary>
    [Test]
    public void CaseService_AfterConstruction_ImplementsICaseService()
    {
        var client = new TilbagoApiClient(ConnectionHandler);

        client.CaseService.ShouldBeAssignableTo<ICaseService>();
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoApiClient" /> fulfils the <see cref="ITilbagoApiClient" /> contract.
    /// </summary>
    [Test]
    public void Client_ImplementsITilbagoApiClient()
    {
        var client = new TilbagoApiClient(ConnectionHandler);

        client.ShouldBeAssignableTo<ITilbagoApiClient>();
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoApiClient" /> implements <see cref="IDisposable" />.
    /// </summary>
    [Test]
    public void Client_ImplementsIDisposable()
    {
        var client = new TilbagoApiClient(ConnectionHandler);

        client.ShouldBeAssignableTo<IDisposable>();
    }

    /// <summary>
    ///     Verifies <see cref="TilbagoApiClient.Dispose" /> delegates disposal to the connection handler.
    /// </summary>
    [Test]
    public void Dispose_DelegatesDisposeToConnectionHandler()
    {
        var client = new TilbagoApiClient(ConnectionHandler);

        client.Dispose();

        ConnectionHandler.Received(1).Dispose();
    }
}