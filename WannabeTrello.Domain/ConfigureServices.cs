using Microsoft.Extensions.DependencyInjection;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Domain;

public static class ConfigureServices
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IActivityTrackerService, ActivityTrackerService>();
        services.AddScoped<IColumnService, ColumnService>();
        services.AddScoped<IBoardTaskService, BoardTaskService>();
        
        return services;
    }
}