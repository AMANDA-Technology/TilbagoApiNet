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


using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Interfaces.Connectors;
using TilbagoApiNet.Services.Connectors;

namespace TilbagoApiNet.Services;

/// <summary>
/// Connector service to call tilbago REST API
/// </summary>
public sealed class TilbagoApiClient : ITilbagoApiClient
{
    /// <summary>
    /// Instance of connection handler used for all services
    /// </summary>
    private readonly ITilbagoConnectionHandler _tilbagoConnectionHandler;

    /// <summary>
    /// Tilbago REST service that holds a manager for calling the API
    /// </summary>
    public TilbagoApiClient(ITilbagoConnectionHandler tilbagoConnectionHandler)
    {
        _tilbagoConnectionHandler = tilbagoConnectionHandler;
        CaseService = new CaseService(tilbagoConnectionHandler);
    }

    /// <summary>
    /// Tilbago cases service endpoint
    /// </summary>
    public ICaseService CaseService { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        _tilbagoConnectionHandler.Dispose();
    }
}
