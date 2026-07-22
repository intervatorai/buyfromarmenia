using BFA.Admin.Application.Commands.Auth;
using BFA.Admin.Application.Commands.Products;
using BFA.Admin.Application.Queries.Products;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Admin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApproveProductCommand).Assembly));

        return services;
    }
}
