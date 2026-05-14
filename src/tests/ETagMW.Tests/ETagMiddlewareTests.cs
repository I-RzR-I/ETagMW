// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW.Tests
//  Author            : RzR
//  Created           : 07-05-2026 17:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:18
//  ***********************************************************************
//  <copyright file="ETagMiddlewareTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Globalization;
using System.Net;
using System.Text;
using ETagMW.Tests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RzR.Web.Middleware.ETag;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Options;
using Xunit;

#endregion

namespace ETagMW.Tests
{
    public class ETagMiddlewareTests
    {
        [Fact]
        public async Task Get_ResponseContainsEtag_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();

            var response = await client.GetAsync("/etag");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task BodyWriter_ResponseContainsEtag_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();

            var response = await client.GetAsync("/writer");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("writer-payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MatchingIfNoneMatch_Returns304WithoutBodyHeaders_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();

            var initialResponse = await client.GetAsync("/etag");
            var request = new HttpRequestMessage(HttpMethod.Get, "/etag");
            request.Headers.TryAddWithoutValidation("If-None-Match", $"W/{initialResponse.Headers.ETag}");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.True(response.Content.Headers.ContentLength == null || response.Content.Headers.ContentLength == 0);
            Assert.Null(response.Content.Headers.ContentType);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_RequestDoesNotAddEtag_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();

            var response = await client.PostAsync("/etag", new StringContent("input"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.Equal("posted", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Large_ResponseBypassesEtag_Test()
        {
            using var host = await CreateHostAsync(options => options.MaxBodySize = 32);
            var client = host.GetTestClient();

            var response = await client.GetAsync("/large");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.Equal(new string('x', 128), await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Range_RequestBypassesEtagAndPreservesBody_Test()
        {
            using var host = await CreateHostAsync();
            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "/etag");
            request.Headers.TryAddWithoutValidation("Range", "bytes=0-3");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.Equal("payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task AddETag_UsesConfiguredFactory_Test()
        {
            using var host = await CreateHostAsync(options => options.ETagFactory = _ => "factory-tag");
            var client = host.GetTestClient();

            var response = await client.GetAsync("/etag");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"factory-tag\"", response.Headers.ETag.ToString());
        }

        [Fact]
        public async Task AddETag_UsesRegisteredGenerator_Test()
        {
            using var host = await CreateHostAsync(
                configureServices: services =>
                    services.AddSingleton<IETagGenerator>(new TestETagGenerator("generator-tag")));

            var client = host.GetTestClient();
            var response = await client.GetAsync("/etag");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"generator-tag\"", response.Headers.ETag.ToString());
        }

        [Fact]
        public async Task AddETag_BindsConfiguration_Test()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ETag:ApplyETagByDefault"] = "true",
                    ["ETag:MaxBodySize"] = "32",
                    ["ETag:SupportedMethods:0"] = HttpMethods.Get
                })
                .Build();

            using var host = await CreateHostAsync(configuration: configuration.GetSection("ETag"));
            var client = host.GetTestClient();

            var response = await client.GetAsync("/large");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.Equal(new string('x', 128), await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_RequestWithMatchingIfMatch_ReturnsBody_Test()
        {
            using var host = await CreateHostAsync(options =>
            {
                options.ETagFactory = _ => "writer-tag";
                options.SupportedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    HttpMethods.Get,
                    HttpMethods.Put
                };
            });

            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Put, "/etag")
            {
                Content = new StringContent("input")
            };

            request.Headers.TryAddWithoutValidation("If-Match", "\"writer-tag\"");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("updated", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_RequestWithNonMatchingIfMatch_Returns412_Test()
        {
            using var host = await CreateHostAsync(options =>
            {
                options.ETagFactory = _ => "writer-tag";
                options.SupportedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    HttpMethods.Get,
                    HttpMethods.Put
                };
            });

            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Put, "/etag")
            {
                Content = new StringContent("input")
            };

            request.Headers.TryAddWithoutValidation("If-Match", "\"other-tag\"");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.True(response.Content.Headers.ContentLength == null || response.Content.Headers.ContentLength == 0);
            Assert.Null(response.Content.Headers.ContentType);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Put_RequestWithMatchingIfNoneMatch_Returns412_Test()
        {
            using var host = await CreateHostAsync(options =>
            {
                options.ETagFactory = _ => "writer-tag";
                options.SupportedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    HttpMethods.Get,
                    HttpMethods.Put
                };
            });

            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Put, "/etag")
            {
                Content = new StringContent("input")
            };

            request.Headers.TryAddWithoutValidation("If-None-Match", "\"writer-tag\"");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Get_RequestWithIfModifiedSince_Returns304AndLastModified_Test()
        {
            var lastModified = new DateTimeOffset(2026, 05, 07, 12, 15, 10, TimeSpan.Zero);

            using var host = await CreateHostAsync(options => { options.LastModifiedFactory = _ => lastModified; });
            var client = host.GetTestClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "/etag");

            request.Headers.TryAddWithoutValidation(
                "If-Modified-Since",
                lastModified.AddMinutes(10).ToUniversalTime().ToString("R", CultureInfo.InvariantCulture));

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.True(response.Content.Headers.LastModified.HasValue);
            Assert.Equal(lastModified, response.Content.Headers.LastModified.Value);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        }

        private static async Task<IHost> CreateHostAsync(
            Action<ETagOption>? configureOptions = null,
            Action<IServiceCollection>? configureServices = null,
            IConfiguration? configuration = null,
            bool useLegacyRegistration = false)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddRouting();

                        if (!useLegacyRegistration)
                        {
                            if (configuration != null)
                                services.AddETag(configuration);
                            else if (configureOptions == null)
                                services.AddETag();
                            else
                                services.AddETag(configureOptions);
                        }

                        configureServices?.Invoke(services);
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        if (useLegacyRegistration)
                        {
                            if (configureOptions == null)
                            {
                                app.UseETag();
                            }
                            else
                            {
#pragma warning disable CS0618
                                app.UseETag(configureOptions);
#pragma warning restore CS0618
                            }
                        }
                        else
                        {
                            app.UseETag();
                        }

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/etag", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("payload");
                            });

                            endpoints.MapGet("/writer", async context =>
                            {
                                context.Response.ContentType = "text/plain";

                                await context.Response.BodyWriter.WriteAsync(
                                    Encoding.UTF8.GetBytes("writer-payload"),
                                    context.RequestAborted);
                            });

                            endpoints.MapPost("/etag", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("posted");
                            });

                            endpoints.MapPut("/etag", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("updated");
                            });

                            endpoints.MapGet("/large", async context =>
                            {
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync(new string('x', 128));
                            });
                        });
                    });
                });

            return await hostBuilder.StartAsync();
        }
    }
}