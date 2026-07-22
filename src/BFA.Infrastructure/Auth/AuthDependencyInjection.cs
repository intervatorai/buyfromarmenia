using BFA.Modules.Identity.Domain.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Infrastructure.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
