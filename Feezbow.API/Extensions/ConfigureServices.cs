
using Asp.Versioning;
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
        
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

