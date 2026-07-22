using BFA.Public.Application.Commands.Products;
using BFA.Public.Application.Queries.Products;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Public.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPublicApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

        return services;
    }
}
