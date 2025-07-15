using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Infrastructure.Persistence;
using WannabeTrello.Infrastructure.Persistence.Repositories;
using WannabeTrello.Infrastructure.Services;
using WannabeTrello.Infrastructure.Services.Notifications;

namespace WannabeTrello.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        services.AddIdentity<User, IdentityRole<long>>(options => // Koristi vaš User entitet i long za ključeve
            {
                // Konfiguracija lozinki (za razvoj/testiranje)
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
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
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty)),
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
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Authentication succeeded!");
                        return Task.CompletedTask;
                    }
                };
            });
        // --- Repozitorijumi ---
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IColumnRepository, ColumnRepository>();
        services.AddScoped<IBoardTaskRepository, BoardTaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IBoardNotificationService, BoardNotificationService>();
        services.AddScoped<ITaskNotificationService, TaskNotificationService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSignalR();

        return services;
    }
}