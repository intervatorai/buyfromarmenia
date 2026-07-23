using BFA.Modules.Identity.Domain.Aggregates;

namespace BFA.Modules.Identity.Domain.Auth;

public record AuthTokenResult(string AccessToken, DateTime ExpiresAt);

public interface IJwtTokenService
{
    AuthTokenResult GenerateToken(AdminUser adminUser);
    AuthTokenResult GenerateCustomerToken(
        User user,
        CustomerProfile profile,
        Guid? impersonatedByAdminId = null,
        int? expirationHoursOverride = null);
    AuthTokenResult GenerateSupplierToken(
        User user,
        Guid supplierId,
        string tradingName,
        string memberRole);
}
