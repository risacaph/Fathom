using Microsoft.Extensions.DependencyInjection;

namespace Fathom.Models.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
    }
}
