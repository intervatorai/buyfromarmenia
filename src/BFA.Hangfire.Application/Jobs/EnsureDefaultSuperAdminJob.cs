using BFA.Hangfire.Application;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BFA.Hangfire.Application.Jobs;

public class EnsureDefaultSuperAdminJob
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<EnsureDefaultSuperAdminJob> _logger;

    public EnsureDefaultSuperAdminJob(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher,
        ILogger<EnsureDefaultSuperAdminJob> logger)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _adminUserRepository.ExistsByEmailAndRoleAsync(
            DefaultAdminSeed.Login,
            AdminRole.SuperAdmin,
            cancellationToken);

        if (exists)
        {
            return;
        }

        var adminUser = AdminUser.Create(
            DefaultAdminSeed.Login,
            _passwordHasher.Hash(DefaultAdminSeed.Password),
            DefaultAdminSeed.FullName,
            AdminRole.SuperAdmin);

        await _adminUserRepository.AddAsync(adminUser, cancellationToken);

        _logger.LogInformation(
            "Created default super admin user with login {Login}.",
            DefaultAdminSeed.Login);
    }
}
