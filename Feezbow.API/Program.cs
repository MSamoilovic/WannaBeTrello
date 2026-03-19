using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Feezbow.Application;
using Feezbow.Domain;
using Feezbow.Extensions;
using Feezbow.Infrastructure;
using Feezbow.Infrastructure.Options;
using Feezbow.Infrastructure.Persistence;
using Feezbow.Infrastructure.SignalR.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Feezbow", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    // Global limiter: 100 req/min per user (authenticated) or per IP (anonymous)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // Strict policy for auth endpoints (login, register, forgot-password, etc.)
    options.AddSlidingWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many requests. Please slow down and try again later." },
            cancellationToken: cancellationToken);
    };
});

builder.Services.AddDomainServices();
builder.Services.AddInfrastructure(builder.Configuration, builder);
builder.Services.AddApplication();
builder.Services.AddPresentation();


builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    options.AddPolicy("Default", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Validate configuration options
using (var scope = app.Services.CreateScope())
{
    var emailOptions = scope.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>();
    var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>();
    try
    {
        _ = emailOptions.Value;
        _ = jwtOptions.Value;
    }
    catch (OptionsValidationException ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError("Configuration validation failed: {Errors}",
            string.Join(", ", ex.Failures));
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();

app.UseCors("Default");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<BoardHub>("/hubs/boards").RequireAuthorization();
app.MapHub<ProjectHub>("/hubs/projects").RequireAuthorization();
app.MapHub<NotificationHub>("/hubs/notifications").RequireAuthorization();
app.MapHub<PresenceHub>("/hubs/presence").RequireAuthorization();
app.Run();



