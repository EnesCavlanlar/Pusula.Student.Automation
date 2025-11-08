using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pusula.Student.Automation.Identity;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Pusula.Student.Automation.Data
{
    public class StudentAutomationDataSeedContributor :
        IDataSeedContributor, ITransientDependency
    {
        private readonly IGuidGenerator _guid;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly ICurrentTenant _currentTenant;
        private readonly ILogger<StudentAutomationDataSeedContributor> _logger;

        public StudentAutomationDataSeedContributor(
            IGuidGenerator guid,
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            ICurrentTenant currentTenant,
            ILogger<StudentAutomationDataSeedContributor> logger)
        {
            _guid = guid;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentTenant = currentTenant;
            _logger = logger;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            // … SeedAsync içinde en başa ekle
            var existingAdminByName = await _userManager.FindByNameAsync("admin");
            if (existingAdminByName != null)
            {
                // Bu sınıf admin üretmeyecek.
                return;
            }

            using (_currentTenant.Change(context?.TenantId))
            {
                // 1️⃣ Rolleri oluştur
                await EnsureRoleAsync(StudentAutomationRoleNames.Admin);
                await EnsureRoleAsync(StudentAutomationRoleNames.Teacher);
                await EnsureRoleAsync(StudentAutomationRoleNames.Student);

                // 2️⃣ Admin kullanıcı oluştur veya güncelle
                const string targetEmail = "admin@student.local";
                const string targetUserName = "admin";
                const string targetPassword = "Admin123!";

                var adminUser =
                    await _userManager.FindByEmailAsync(targetEmail) ??
                    await _userManager.FindByNameAsync(targetUserName);

                if (adminUser == null)
                {
                    adminUser = new IdentityUser(_guid.Create(), targetUserName, targetEmail);
                    (await _userManager.CreateAsync(adminUser)).CheckErrors();
                    _logger.LogInformation("Created admin user {Email}", targetEmail);
                }

                // Email / Username güncelle
                await _userManager.SetEmailAsync(adminUser, targetEmail);
                await _userManager.SetUserNameAsync(adminUser, targetUserName);

                // Şifreyi sıfırla
                if (await _userManager.HasPasswordAsync(adminUser))
                    (await _userManager.RemovePasswordAsync(adminUser)).CheckErrors();

                (await _userManager.AddPasswordAsync(adminUser, targetPassword)).CheckErrors();

                // Email onayını elle ver (token üretmeden)
                adminUser.SetEmailConfirmed(true);

                // Lockout kapat, sayaç sıfırla
                await _userManager.SetLockoutEnabledAsync(adminUser, false);
                await _userManager.ResetAccessFailedCountAsync(adminUser);

                (await _userManager.UpdateAsync(adminUser)).CheckErrors();

                // Admin rolü ata
                if (!await _userManager.IsInRoleAsync(adminUser, StudentAutomationRoleNames.Admin))
                {
                    (await _userManager.AddToRoleAsync(adminUser, StudentAutomationRoleNames.Admin)).CheckErrors();
                    _logger.LogInformation("Added admin role to {Email}", targetEmail);
                }
            }
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                (await _roleManager.CreateAsync(new IdentityRole(_guid.Create(), roleName))).CheckErrors();
                _logger.LogInformation("Created role {Role}", roleName);
            }
        }
    }
}
