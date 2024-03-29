﻿/*
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

using Microsoft.Extensions.DependencyInjection;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.AspNetCore;

/// <summary>
/// Tilbago service collection extension for dependency injection
/// </summary>
public static class TilbagoServiceCollection
{
    /// <summary>
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiKey"></param>
    /// <param name="baseUri"></param>
    /// <returns></returns>
    public static IServiceCollection AddTilbagoServices(this IServiceCollection services, string apiKey, string baseUri)
    {
        return services.AddTilbagoServices(new TilbagoConfiguration(apiKey, baseUri));
    }

    /// <summary>
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="tilbagoConfiguration"></param>
    /// <returns></returns>
    public static IServiceCollection AddTilbagoServices(this IServiceCollection services, ITilbagoConfiguration tilbagoConfiguration)
    {
        services.AddSingleton(tilbagoConfiguration);
        services.AddScoped<ITilbagoConnectionHandler, TilbagoConnectionHandler>();
        services.AddScoped<ITilbagoApiClient, TilbagoApiClient>();

        return services;
    }
}
