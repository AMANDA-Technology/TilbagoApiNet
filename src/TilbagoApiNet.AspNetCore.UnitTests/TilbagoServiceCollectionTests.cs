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

using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using TilbagoApiNet.Interfaces;
using TilbagoApiNet.Services;

namespace TilbagoApiNet.AspNetCore.UnitTests;

/// <summary>
///     Unit tests for <see cref="TilbagoServiceCollection" />, exercising both <c>AddTilbagoServices</c> overloads via
///     a real <see cref="ServiceCollection" /> and asserting the registered descriptors and resolution behaviour.
/// </summary>
[TestFixture]
public class TilbagoServiceCollectionTests
{
    private const string ApiKey = "test-api-key";
    private const string BaseUri = "https://tilbago.test/api/v1";

    /// <summary>
    ///     Verifies the (apiKey, baseUri) overload registers an <see cref="ITilbagoConfiguration" /> singleton whose
    ///     properties carry the values supplied to the call.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_RegistersConfigurationSingletonWithSuppliedValues()
    {
        var services = new ServiceCollection();

        services.AddTilbagoServices(ApiKey, BaseUri);

        using var provider = services.BuildServiceProvider();
        var configuration = provider.GetRequiredService<ITilbagoConfiguration>();
        configuration.ApiKey.ShouldBe(ApiKey);
        configuration.BaseUri.ShouldBe(BaseUri);
    }

    /// <summary>
    ///     Verifies the (apiKey, baseUri) overload registers all three services with their expected lifetimes:
    ///     singleton configuration, scoped connection handler and scoped API client.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_RegistersExpectedLifetimes()
    {
        var services = new ServiceCollection();

        services.AddTilbagoServices(ApiKey, BaseUri);

        AssertExpectedDescriptors(services);
    }

    /// <summary>
    ///     Verifies the configuration overload stores the exact instance supplied by the caller as a singleton.
    /// </summary>
    [Test]
    public void AddTilbagoServices_ConfigurationOverload_RegistersSuppliedConfigurationInstance()
    {
        var services = new ServiceCollection();
        var configuration = Substitute.For<ITilbagoConfiguration>();
        configuration.ApiKey.Returns(ApiKey);
        configuration.BaseUri.Returns(BaseUri);

        services.AddTilbagoServices(configuration);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ITilbagoConfiguration>().ShouldBeSameAs(configuration);
    }

    /// <summary>
    ///     Verifies the configuration overload registers all three services with their expected lifetimes:
    ///     singleton configuration, scoped connection handler and scoped API client.
    /// </summary>
    [Test]
    public void AddTilbagoServices_ConfigurationOverload_RegistersExpectedLifetimes()
    {
        var services = new ServiceCollection();

        services.AddTilbagoServices(new TilbagoConfiguration(ApiKey, BaseUri));

        AssertExpectedDescriptors(services);
    }

    /// <summary>
    ///     Verifies the configuration overload returns the same <see cref="IServiceCollection" /> to support fluent
    ///     chaining, matching the convention of other Microsoft <c>Add*</c> extensions.
    /// </summary>
    [Test]
    public void AddTilbagoServices_ConfigurationOverload_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddTilbagoServices(new TilbagoConfiguration(ApiKey, BaseUri));

        result.ShouldBeSameAs(services);
    }

    /// <summary>
    ///     Verifies the (apiKey, baseUri) overload returns the same <see cref="IServiceCollection" /> to support fluent
    ///     chaining.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddTilbagoServices(ApiKey, BaseUri);

        result.ShouldBeSameAs(services);
    }

    /// <summary>
    ///     Verifies <see cref="ITilbagoApiClient" /> resolves to a non-null, fully-wired
    ///     <see cref="TilbagoApiClient" /> with its <c>CaseService</c> dependency populated by the DI graph.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_ResolvesNonNullApiClientWithCaseService()
    {
        var services = new ServiceCollection();
        services.AddTilbagoServices(ApiKey, BaseUri);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<ITilbagoApiClient>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<TilbagoApiClient>();
        client.CaseService.ShouldNotBeNull();
    }

    /// <summary>
    ///     Verifies the connection handler is resolved as a scoped service: two resolutions inside the same scope return
    ///     the same instance, while resolutions across different scopes return different instances.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_ConnectionHandlerIsScoped()
    {
        var services = new ServiceCollection();
        services.AddTilbagoServices(ApiKey, BaseUri);
        using var provider = services.BuildServiceProvider();

        ITilbagoConnectionHandler firstFromScopeA;
        ITilbagoConnectionHandler secondFromScopeA;
        using (var scopeA = provider.CreateScope())
        {
            firstFromScopeA = scopeA.ServiceProvider.GetRequiredService<ITilbagoConnectionHandler>();
            secondFromScopeA = scopeA.ServiceProvider.GetRequiredService<ITilbagoConnectionHandler>();
        }

        ITilbagoConnectionHandler fromScopeB;
        using (var scopeB = provider.CreateScope())
        {
            fromScopeB = scopeB.ServiceProvider.GetRequiredService<ITilbagoConnectionHandler>();
        }

        firstFromScopeA.ShouldBeSameAs(secondFromScopeA);
        firstFromScopeA.ShouldNotBeSameAs(fromScopeB);
    }

    /// <summary>
    ///     Verifies the API client is resolved as a scoped service: two resolutions inside the same scope return the same
    ///     instance, while resolutions across different scopes return different instances.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_ApiClientIsScoped()
    {
        var services = new ServiceCollection();
        services.AddTilbagoServices(ApiKey, BaseUri);
        using var provider = services.BuildServiceProvider();

        ITilbagoApiClient firstFromScopeA;
        ITilbagoApiClient secondFromScopeA;
        using (var scopeA = provider.CreateScope())
        {
            firstFromScopeA = scopeA.ServiceProvider.GetRequiredService<ITilbagoApiClient>();
            secondFromScopeA = scopeA.ServiceProvider.GetRequiredService<ITilbagoApiClient>();
        }

        ITilbagoApiClient fromScopeB;
        using (var scopeB = provider.CreateScope())
        {
            fromScopeB = scopeB.ServiceProvider.GetRequiredService<ITilbagoApiClient>();
        }

        firstFromScopeA.ShouldBeSameAs(secondFromScopeA);
        firstFromScopeA.ShouldNotBeSameAs(fromScopeB);
    }

    /// <summary>
    ///     Verifies the configuration is resolved as a singleton: every resolution — across the root provider and any
    ///     scope — returns the same instance.
    /// </summary>
    [Test]
    public void AddTilbagoServices_StringOverload_ConfigurationIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddTilbagoServices(ApiKey, BaseUri);
        using var provider = services.BuildServiceProvider();

        var fromRoot = provider.GetRequiredService<ITilbagoConfiguration>();
        using var scope = provider.CreateScope();
        var fromScope = scope.ServiceProvider.GetRequiredService<ITilbagoConfiguration>();

        fromRoot.ShouldBeSameAs(fromScope);
    }

    /// <summary>
    ///     Verifies the descriptors registered by <see cref="TilbagoServiceCollection.AddTilbagoServices(IServiceCollection,ITilbagoConfiguration)" />
    ///     match the documented contract: a singleton <see cref="ITilbagoConfiguration" />, a scoped
    ///     <see cref="ITilbagoConnectionHandler" /> and a scoped <see cref="ITilbagoApiClient" />.
    /// </summary>
    private static void AssertExpectedDescriptors(IServiceCollection services)
    {
        var configurationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITilbagoConfiguration));
        configurationDescriptor.ShouldNotBeNull();
        configurationDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);

        var connectionHandlerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITilbagoConnectionHandler));
        connectionHandlerDescriptor.ShouldNotBeNull();
        connectionHandlerDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        connectionHandlerDescriptor.ImplementationType.ShouldBe(typeof(TilbagoConnectionHandler));

        var apiClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITilbagoApiClient));
        apiClientDescriptor.ShouldNotBeNull();
        apiClientDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        apiClientDescriptor.ImplementationType.ShouldBe(typeof(TilbagoApiClient));
    }
}
