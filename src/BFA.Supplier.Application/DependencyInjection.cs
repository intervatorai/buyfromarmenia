using BFA.Supplier.Application.Commands.Products;
using BFA.Supplier.Application.Queries.Products;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Supplier.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSupplierApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

        return services;
    }
}
