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
    /// <summary>
    /// DbMigrator çalıştığında: roller + admin kullanıcı (İZİN YOK – izinler Application katmanında verilecek).
    /// </summary>
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

                // 2) Admin kullanıcıyı oluştur ve admin rolüne ekle
                const string adminEmail = "admin@student.local";
                const string adminUserName = "admin";
                const string adminPassword = "Admin123*"; // DEV için

                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser(_guid.Create(), adminUserName, adminEmail);
                    (await _userManager.CreateAsync(adminUser, adminPassword)).CheckErrors();
                    _logger.LogInformation("Created admin user {Email}", adminEmail);
                }

                if (!await _userManager.IsInRoleAsync(adminUser, StudentAutomationRoleNames.Admin))
                {
                    (await _userManager.AddToRoleAsync(adminUser, StudentAutomationRoleNames.Admin)).CheckErrors();
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
