using DotNet.Testcontainers.Builders;
using Feezbow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
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
                    // Replace real email service with no-op to prevent SMTP calls in tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddScoped<IEmailService, FakeEmailService>();
                });

                builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
                builder.UseSetting("Redis:Enabled", "true");
                builder.UseSetting("Redis:ConnectionString", _redis.GetConnectionString());
                builder.UseSetting("Redis:InstanceName", "feezbow_test_");
                builder.UseSetting("Jwt:Key", "test-secret-key-at-least-32-characters-long!");
                builder.UseSetting("Jwt:Issuer", "feezbow-test");
                builder.UseSetting("Jwt:Audience", "feezbow-test");
                builder.UseSetting("Jwt:ExpiryMinutes", "60");
                builder.UseSetting("Email:SmtpHost", "localhost");
                builder.UseSetting("Email:SmtpPort", "25");
                builder.UseSetting("Email:SmtpUsername", "test");
                builder.UseSetting("Email:SmtpPassword", "test");
                builder.UseSetting("Email:FromEmail", "test@feezbow.test");
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
