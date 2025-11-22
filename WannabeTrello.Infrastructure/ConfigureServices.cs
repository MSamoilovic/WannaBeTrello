using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Infrastructure.Options;
using WannabeTrello.Infrastructure.Options.Validators;
using WannabeTrello.Infrastructure.Persistence;
using WannabeTrello.Infrastructure.Persistence.Repositories;
using WannabeTrello.Infrastructure.Services;
using WannabeTrello.Infrastructure.Services.Notifications;

namespace WannabeTrello.Infrastructure;

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
                // Identity će koristiti UserName za login, ali i Email se može koristiti
            })
            .AddEntityFrameworkStores<ApplicationDbContext>() // Povezuje Identity sa vašim DbContext-om
            .AddDefaultTokenProviders(); // Omogućava generisanje tokena za reset lozinke, email potvrdu itd.

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
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    Console.WriteLine($"Error Type: {context.Exception.GetType().Name}");
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
        services.AddScoped<IActivityTrackerRepository, ActivityTrackerRepository>();
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IBoardNotificationService, BoardNotificationService>();
        services.AddScoped<ITaskNotificationService, TaskNotificationService>();
        services.AddScoped<IProjectNotificationService, ProjectNotificationService>();
        services.AddScoped<IColumnNotificationService, ColumnNotificationService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSignalR();

        return services;
    }
}