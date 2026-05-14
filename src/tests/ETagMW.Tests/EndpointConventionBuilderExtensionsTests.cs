// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW.Tests
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:18
//  ***********************************************************************
//  <copyright file="EndpointConventionBuilderExtensionsTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RzR.Web.Middleware.ETag;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Attributes;
using RzR.Web.Middleware.ETag.Options;
using Xunit;

#endregion

namespace ETagMW.Tests
{
    public class EndpointConventionBuilderExtensionsTests
    {
        [Fact]
        public async Task Endpoint_HelpersWorkWithoutLibraryNamespaceUsing_Test()
        {
            using var host = await CreateHostAsync(options => options.ApplyETagByDefault = false);
            var client = host.GetTestClient();

            var optInResponse = await client.GetAsync("/no-using-opt-in");
            var customTagResponse = await client.GetAsync("/no-using-custom-tag");
            var optOutResponse = await client.GetAsync("/no-using-opt-out");

            Assert.Equal(HttpStatusCode.OK, optInResponse.StatusCode);
            Assert.NotNull(optInResponse.Headers.ETag);
            Assert.Equal("no-using-opt-in-payload", await optInResponse.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, customTagResponse.StatusCode);
            Assert.NotNull(customTagResponse.Headers.ETag);
            Assert.Equal("\"no-using-route-tag\"", customTagResponse.Headers.ETag.ToString());
            Assert.Equal("no-using-custom-tag-payload", await customTagResponse.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, optOutResponse.StatusCode);
            Assert.Null(optOutResponse.Headers.ETag);
            Assert.Equal("no-using-opt-out-payload", await optOutResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Endpoint_ETagOptionsAttribute_UsesDIConfigurator_Test()
        {
            using var host = await CreateHostAsync(
                options => options.ApplyETagByDefault = false,
                services => services.AddSingleton<RouteConfigurator>());

            var client = host.GetTestClient();

            var response = await client.GetAsync("/no-using-attr-configured");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"di-configurator-tag\"", response.Headers.ETag.ToString());
            Assert.Equal("attr-configured-payload", await response.Content.ReadAsStringAsync());
        }

        private static void MapEndpointsWithoutLibraryUsing(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/no-using-opt-in", async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("no-using-opt-in-payload");
            }).WithETag();

            endpoints.MapGet("/no-using-custom-tag", async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("no-using-custom-tag-payload");
            }).WithETag(options => options.ETagFactory = _ => "no-using-route-tag");

            endpoints.MapGet("/no-using-opt-out", async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("no-using-opt-out-payload");
            }).WithoutETag();

            endpoints.MapGet("/no-using-attr-configured", async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("attr-configured-payload");
            }).WithMetadata(new ETagOptionsAttribute(typeof(RouteConfigurator)));
        }

        private static async Task<IHost> CreateHostAsync(
            Action<ETagOption>? configureOptions = null,
            Action<IServiceCollection>? configureServices = null)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddRouting();

                        if (configureOptions == null)
                            DependencyInjection.AddETag(services);
                        else
                            DependencyInjection.AddETag(services, configureOptions);

                        configureServices?.Invoke(services);
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        DependencyInjection.UseETag(app);
                        app.UseEndpoints(MapEndpointsWithoutLibraryUsing);
                    });
                });

            return await hostBuilder.StartAsync();
        }

        private sealed class RouteConfigurator : IETagOptionsConfigurator
        {
            public void Configure(ETagOption options)
            {
                options.ETagFactory = _ => "di-configurator-tag";
            }
        }
    }
}