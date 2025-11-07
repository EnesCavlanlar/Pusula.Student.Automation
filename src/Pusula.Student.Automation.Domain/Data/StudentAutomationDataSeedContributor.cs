using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pusula.Student.Automation.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Pusula.Student.Automation.Data
{
    // ABP, IDataSeedContributor implement eden sınıfları otomatik bulur ve DbMigrator çalışınca tetikler.
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
            using (_currentTenant.Change(context?.TenantId))
            {
                // 1) Rolleri oluştur
                await EnsureRoleAsync(StudentAutomationRoleNames.Admin);
                await EnsureRoleAsync(StudentAutomationRoleNames.Teacher);
                await EnsureRoleAsync(StudentAutomationRoleNames.Student);

                // 2) Admin kullanıcıyı oluştur ve role ata
                var adminEmail = "admin@student.local";
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var id = _guid.Create();
                    adminUser = new IdentityUser(id, adminEmail, adminEmail);
                    var createResult = await _userManager.CreateAsync(adminUser, "Admin123*");
                    createResult.CheckErrors();

                    _logger.LogInformation("Created admin user {Email}", adminEmail);
                }

                // Admin rolünü ver (varsa tekrar vermeye çalışmak sorun olmaz)
                if (!await _userManager.IsInRoleAsync(adminUser, StudentAutomationRoleNames.Admin))
                {
                    var roleResult = await _userManager.AddToRoleAsync(adminUser, StudentAutomationRoleNames.Admin);
                    roleResult.CheckErrors();
                }
            }
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                var create = await _roleManager.CreateAsync(new IdentityRole(_guid.Create(), roleName));
                create.CheckErrors();
                _logger.LogInformation("Created role {Role}", roleName);
            }
        }
    }
}
