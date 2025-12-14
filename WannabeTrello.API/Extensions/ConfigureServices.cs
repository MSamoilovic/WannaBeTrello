
using WannabeTrello.Filters;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.ModelBuilder;
using WannabeTrello.Application.Features.Tasks.SearchTasks;
using OData.Swagger.Services;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Services;
using WannabeTrello.Application.Features.Users.SearchUsers;

namespace WannabeTrello.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        
        services.Configure<RouteOptions>(options =>
        {
            if (!options.ConstraintMap.ContainsKey("long"))
            {
                options.ConstraintMap.Add("long", typeof(LongRouteConstraint));
            }
        });

        services.AddControllers(options =>
        {
            options.Filters.Add<CustomExceptionFilter>();

        }).AddOData(options =>
        {
            
            var modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<SearchTaskQueryResponse>("Tasks");

            modelBuilder.EntitySet<SearchUsersQueryResponse>("Users");

            options.AddRouteComponents("odata", modelBuilder.GetEdmModel())
                   .Select()
                   .Expand()
                   .Filter()
                   .OrderBy()
                   .SetMaxTop(100)
                   .Count();
        });

        services.AddOdataSwaggerSupport();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

