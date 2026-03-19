using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Feezbow.Application.Common.Behaviors;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Services;

namespace Feezbow.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // --- MediatR ---
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        //Domenski Servisi
        services.AddScoped<BoardService>();
        services.AddScoped<BoardTaskService>();
        services.AddScoped<ProjectService>();

        return services;
    }
}