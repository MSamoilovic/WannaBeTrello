
using Feezbow.Filters;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Feezbow.Application.Features.Tasks.SearchTasks;
using OData.Swagger.Services;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Services;
using Feezbow.Application.Features.Users.SearchUsers;

namespace Feezbow.Extensions;

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

