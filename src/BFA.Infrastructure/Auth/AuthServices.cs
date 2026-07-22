using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BFA.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokenResult GenerateToken(AdminUser adminUser)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT secret is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "BFA.Admin.Api";
        var audience = jwtSettings["Audience"] ?? "BFA.Admin.UI";
        var expirationHours = int.TryParse(jwtSettings["ExpirationHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, adminUser.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, adminUser.Email),
            new(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new(ClaimTypes.Name, adminUser.FullName),
            new(ClaimTypes.Role, adminUser.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public AuthTokenResult GenerateCustomerToken(User user, CustomerProfile profile)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT secret is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "BFA.Public.Api";
        var audience = jwtSettings["Audience"] ?? "BFA.Public.UI";
        var expirationHours = int.TryParse(jwtSettings["ExpirationHours"], out var hours) ? hours : 168;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, profile.FullName),
            new(ClaimTypes.Role, "Customer")
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public AuthTokenResult GenerateSupplierToken(
        User user,
        Guid supplierId,
        string tradingName,
        string memberRole)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT secret is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "BFA.Supplier.Api";
        var audience = jwtSettings["Audience"] ?? "BFA.Supplier.UI";
        var expirationHours = int.TryParse(jwtSettings["ExpirationHours"], out var hours) ? hours : 168;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, tradingName),
            new(ClaimTypes.Role, memberRole),
            new("supplier_id", supplierId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
