using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Infrastructure.Options;
using Feezbow.Infrastructure.Options.Validators;
using Feezbow.Infrastructure.Persistence;
using Feezbow.Infrastructure.Persistence.Repositories;
using Feezbow.Infrastructure.Services;
using Feezbow.Infrastructure.Services.Notifications;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Infrastructure.SignalR.Authorization;
using Feezbow.Infrastructure.SignalR.Configuration;
using Feezbow.Infrastructure.SignalR.Filters;
using Feezbow.Infrastructure.SignalR.Hubs.Base;
using Feezbow.Infrastructure.SignalR.Resilience;
using Feezbow.Infrastructure.SignalR.Security;
using Feezbow.Infrastructure.SignalR.Services;

namespace Feezbow.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        
        var dataSource = dataSourceBuilder.Build();
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource, 
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        services.AddIdentity<User, IdentityRole<long>>(options => 
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;

                // Konfiguracija Lockout-a (opciono)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Konfiguracija korisničkog imena i emaila
                options.User.RequireUniqueEmail = true;
               
            })
            .AddEntityFrameworkStores<ApplicationDbContext>() 
            .AddDefaultTokenProviders();

        //--- Registracija Option patterna -- 
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        // --- JWT Bearer Authentication ---
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerHandler>>();

                    logger.LogWarning(context.Exception,
                        "JWT authentication failed: {ExceptionType}",
                        context.Exception.GetType().Name);

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<User>>();
                    
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (long.TryParse(userId, out var id))
                    {
                        var user = await userManager.FindByIdAsync(id.ToString());
                        if (user == null || !user.IsActive)
                        {
                            context.Fail("User is not active");
                        }
                    }
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("EmailConfirmed", policy =>
                policy.RequireClaim("email_confirmed", "true"));

           
            options.AddPolicy("EmailConfirmedOrAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.HasClaim("email_confirmed", "true") ||
                    context.User.IsInRole("Admin"));
            });
        });

        // Post-configure JWT Bearer options with IOptions pattern
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigure>();

        // --- Repozitorijumi ---
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IColumnRepository, ColumnRepository>();
        services.AddScoped<IBoardTaskRepository, BoardTaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();

        //Redis
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? new RedisOptions();

        if (redisOptions.Enabled)
        {

            var redisConnection = ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName + ":";
            });

            // Cache service
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
           
        }

        services.AddScoped<IBoardNotificationService, BoardNotificationService>();
        services.AddScoped<ITaskNotificationService, TaskNotificationService>();
        services.AddScoped<IProjectNotificationService, ProjectNotificationService>();
        services.AddScoped<IColumnNotificationService, ColumnNotificationService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // SignalR
        services.Configure<SignalROptions>(configuration.GetSection(SignalROptions.SectionName));

        var signalROptions = configuration.GetSection(SignalROptions.SectionName).Get<SignalROptions>()
            ?? new SignalROptions();

        services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = signalROptions.MaximumReceiveMessageSize;
            options.StreamBufferCapacity = signalROptions.StreamBufferCapacity;
            options.EnableDetailedErrors = signalROptions.EnableDetailedErrors;
            options.ClientTimeoutInterval = signalROptions.ClientTimeoutInterval;
            options.HandshakeTimeout = signalROptions.HandshakeTimeout;
            options.KeepAliveInterval = signalROptions.KeepAliveInterval;
            options.MaximumParallelInvocationsPerClient = signalROptions.MaximumParallelInvocationsPerClient;
        });

        // Hub filter chain (outer → inner):
        //   HubMethodFilter → HubRateLimitingFilter → HubValidationFilter → HubExceptionFilter
        services.AddSingleton<IHubFilter, HubMethodFilter>();
        services.AddSingleton<IHubFilter, HubRateLimitingFilter>();
        services.AddSingleton<IHubFilter, HubValidationFilter>();
        services.AddSingleton<IHubFilter, HubExceptionFilter>();

        // Phase 2: connection / group / presence services
        services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
        services.AddSingleton<IHubGroupManager, GroupManager>();
        services.AddSingleton<IPresenceTracker, InMemoryPresenceTracker>();

        // Phase 3: authorization handlers + rate limiter
        // IMemoryCache is used by authorization handlers for caching membership checks
        services.AddMemoryCache();
        services.AddSingleton<IAuthorizationHandler, BoardAccessHandler>();
        services.AddSingleton<IAuthorizationHandler, ProjectAccessHandler>();
        services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();

        // Phase 6: resilience pipeline (retry + circuit breaker) for notification services
        services.AddSingleton<ResiliencePipeline>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger("SignalR.Notifications.Resilience");
            return NotificationResiliencePipelineFactory.Create(logger);
        });

        return services;
    }
}