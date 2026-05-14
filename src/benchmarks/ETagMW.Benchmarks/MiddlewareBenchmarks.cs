using System.Net.Http;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RzR.Web.Middleware.ETag;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Generators;

namespace ETagMW.Benchmarks;

[MemoryDiagnoser]
public class MiddlewareBenchmarks
{
    private HttpClient _sha256Client = null!;
    private HttpClient _bodyWriterClient = null!;
    private HttpClient _xxHashClient = null!;
    private IHost _sha256Host = null!;
    private IHost _bodyWriterHost = null!;
    private IHost _xxHashHost = null!;
    private byte[] _payloadBytes = null!;
    private string _payloadText = null!;

    [Params(256, 4096, 32768)]
    public int PayloadSize { get; set; }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _payloadText = new string('x', PayloadSize);
        _payloadBytes = Encoding.UTF8.GetBytes(_payloadText);

        _sha256Host = await CreateHostAsync();
        _bodyWriterHost = await CreateHostAsync(useBodyWriter: true);
        _xxHashHost = await CreateHostAsync(services =>
            services.AddSingleton<IETagGenerator, XxHashETagGenerator>());

        _sha256Client = _sha256Host.GetTestClient();
        _bodyWriterClient = _bodyWriterHost.GetTestClient();
        _xxHashClient = _xxHashHost.GetTestClient();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _sha256Client?.Dispose();
        _bodyWriterClient?.Dispose();
        _xxHashClient?.Dispose();
        _sha256Host?.Dispose();
        _bodyWriterHost?.Dispose();
        _xxHashHost?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public Task<int> Get_ResponseWithSha256_Test()
    {
        return ExecuteRequestAsync(_sha256Client);
    }

    [Benchmark]
    public Task<int> Get_ResponseWithBodyWriter_Test()
    {
        return ExecuteRequestAsync(_bodyWriterClient, "/writer");
    }

    [Benchmark]
    public Task<int> Get_ResponseWithXxHash_Test()
    {
        return ExecuteRequestAsync(_xxHashClient);
    }

    private static async Task<int> ExecuteRequestAsync(HttpClient client, string path = "/etag")
    {
        using var response = await client.GetAsync(path);
        await response.Content.ReadAsByteArrayAsync();

        return response.Headers.ETag?.Tag?.Length ?? (int)response.StatusCode;
    }

    private async Task<IHost> CreateHostAsync(Action<IServiceCollection>? configureServices = null,
        bool useBodyWriter = false)
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddETag();
                    configureServices?.Invoke(services);
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseETag();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/etag", async context =>
                        {
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(_payloadText);
                        });

                        endpoints.MapGet("/writer", async context =>
                        {
                            context.Response.ContentType = "text/plain";
                            await context.Response.BodyWriter.WriteAsync(_payloadBytes, context.RequestAborted);
                        });
                    });
                });
            });

        return await hostBuilder.StartAsync();
    }
}