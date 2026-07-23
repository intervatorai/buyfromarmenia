using BFA.Public.Application.Commands.Products;
using BFA.Public.Application.Services.Payments;
using BFA.Public.Application.Services.Shipping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Public.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPublicApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
        services.AddScoped<IShippingQuoteService, ShippingQuoteService>();
        services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();

        return services;
    }
}
