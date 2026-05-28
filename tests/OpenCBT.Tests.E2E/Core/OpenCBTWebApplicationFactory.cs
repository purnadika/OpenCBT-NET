using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Tests.E2E.Core;

public class OpenCBTWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;
    private SqliteConnection? _connection;

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    public IServiceProvider KestrelServices
    {
        get
        {
            EnsureServer();
            return _host!.Services;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing ApplicationDbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove Redis Cache
            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (cacheDescriptor != null)
            {
                services.Remove(cacheDescriptor);
            }
            services.AddDistributedMemoryCache();

            // Create open SQLite connection so the in-memory database persists
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // 1. Create the dummy test host that WebApplicationFactory expects
        var testHost = builder.Build();

        // 2. Modify the builder to use Kestrel for our actual E2E server
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("http://127.0.0.1:0");
        });

        // 3. Build and start the real Kestrel host
        _host = builder.Build();
        _host.Start();

        var server = _host.Services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        var address = addressFeature?.Addresses.SingleOrDefault();

        if (address != null)
        {
            ClientOptions.BaseAddress = new Uri(address);
        }

        // Return the dummy test host so CreateDefaultClient() doesn't crash on InvalidCastException
        testHost.Start();
        return testHost;
    }

    private void EnsureServer()
    {
        if (_host == null)
        {
            // Forces CreateHost to be called
            using var _ = CreateDefaultClient();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _host?.Dispose();
            _connection?.Dispose();
        }
    }
}
