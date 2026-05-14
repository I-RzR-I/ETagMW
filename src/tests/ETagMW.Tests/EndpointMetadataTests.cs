// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW.Tests
//  Author            : RzR
//  Created           : 07-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:18
//  ***********************************************************************
//  <copyright file="EndpointMetadataTests.cs" company="RzR SOFT & TECH">
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
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RzR.Web.Middleware.ETag;
using RzR.Web.Middleware.ETag.Options;
using Xunit;

#endregion

namespace ETagMW.Tests
{
    /// <summary>
    ///     Behavior tests for endpoint-level ETag metadata
    ///     (<c>WithETag()</c> / <c>WithETag(Action&lt;ETagOption&gt;)</c> / <c>WithoutETag()</c>).
    ///     Split out of <see cref="ETagMiddlewareTests" /> so the middleware suite stays focused on
    ///     middleware behavior and this suite stays focused on endpoint-metadata behavior.
    /// </summary>
    public class EndpointMetadataTests
    {
        [Fact]
        public async Task Endpoint_WithoutETag_DisablesHeader_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();

            var response = await client.GetAsync("/opt-out");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.Equal("opt-out-payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Endpoint_WithETag_EnablesHeader_WhenDisabledByDefault_Test()
        {
            using var host = await CreateHostAsync(options => options.ApplyETagByDefault = false);
            var client = host.GetTestClient();

            var regularResponse = await client.GetAsync("/plain");
            var optedInResponse = await client.GetAsync("/opt-in");

            Assert.Equal(HttpStatusCode.OK, regularResponse.StatusCode);
            Assert.Null(regularResponse.Headers.ETag);
            Assert.Equal("plain-payload", await regularResponse.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, optedInResponse.StatusCode);
            Assert.NotNull(optedInResponse.Headers.ETag);
            Assert.Equal("opt-in-payload", await optedInResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Endpoint_WithETag_UsesPerRouteFactory_Test()
        {
            using var host = await CreateHostAsync(options => options.ApplyETagByDefault = false);
            var client = host.GetTestClient();

            var response = await client.GetAsync("/opt-in-custom-tag");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"route-tag\"", response.Headers.ETag.ToString());
            Assert.Equal("opt-in-custom-tag-payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Endpoint_WithETag_UsesPerRouteSupportedMethods_Test()
        {
            using var host = await CreateHostAsync(options =>
            {
                options.ApplyETagByDefault = false;
                options.SupportedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    HttpMethods.Get
                };
            });

            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Put, "/put-opt-in")
            {
                Content = new StringContent("input")
            };

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"put-route-tag\"", response.Headers.ETag.ToString());
            Assert.Equal("put-opt-in-payload", await response.Content.ReadAsStringAsync());
        }

        private static async Task<IHost> CreateHostAsync(Action<ETagOption>? configureOptions = null)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddRouting();

                        if (configureOptions == null)
                            services.AddETag();
                        else
                            services.AddETag(configureOptions);
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseETag();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/plain", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("plain-payload");
                            });

                            endpoints.MapGet("/opt-in", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("opt-in-payload");
                            }).WithETag();

                            endpoints.MapGet("/opt-in-custom-tag", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("opt-in-custom-tag-payload");
                            }).WithETag(options => options.ETagFactory = _ => "route-tag");

                            endpoints.MapGet("/opt-out", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("opt-out-payload");
                            }).WithoutETag();

                            endpoints.MapPut("/put-opt-in", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("put-opt-in-payload");
                            }).WithETag(options =>
                            {
                                options.ETagFactory = _ => "put-route-tag";
                                options.SupportedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    HttpMethods.Put
                                };
                            });
                        });
                    });
                });

            return await hostBuilder.StartAsync();
        }
    }
}