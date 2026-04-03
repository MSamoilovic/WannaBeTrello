using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Feezbow.Integration.Tests.Infrastructure;

public class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private RedisContainer _redis = null!;
    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("feezbow_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        _redis = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Override connection strings with test containers
                    services.Configure<Microsoft.Extensions.Options.IOptions<object>>(_ => { });
                });

                builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
                builder.UseSetting("Redis:Enabled", "true");
                builder.UseSetting("Redis:ConnectionString", _redis.GetConnectionString());
                builder.UseSetting("Redis:InstanceName", "feezbow_test_");
                builder.UseSetting("Jwt:Key", "test-secret-key-at-least-32-characters-long!");
                builder.UseSetting("Jwt:Issuer", "feezbow-test");
                builder.UseSetting("Jwt:Audience", "feezbow-test");
                builder.UseSetting("Jwt:ExpiryMinutes", "60");
                builder.UseSetting("Email:Host", "localhost");
                builder.UseSetting("Email:Port", "25");
                builder.UseSetting("Email:Username", "test");
                builder.UseSetting("Email:Password", "test");
                builder.UseSetting("Email:From", "test@feezbow.test");
            });

        Client = _factory.CreateClient();
        Services = _factory.Services;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    protected T GetService<T>() where T : notnull =>
        Services.CreateScope().ServiceProvider.GetRequiredService<T>();
}
