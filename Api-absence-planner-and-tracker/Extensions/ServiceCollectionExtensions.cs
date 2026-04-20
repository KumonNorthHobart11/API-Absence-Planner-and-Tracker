using AbsencePlanner.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace Api_absence_planner_and_tracker.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddInfrastructureServices(config);

   // FluentValidation
        services.AddFluentValidationAutoValidation();
   services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}
