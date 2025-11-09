using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Authorization.Permissions;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Data
{
    public class AutomationIdentitySeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPermissionDataSeeder _permissionSeeder;

        public AutomationIdentitySeedContributor(
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            IPermissionDataSeeder permissionSeeder)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionSeeder = permissionSeeder;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            // 1) rolleri yarat
            await EnsureRoleAsync("admin");
            await EnsureRoleAsync("teacher");
            await EnsureRoleAsync("student");

            // 2) 3 kullanıcı
            var admin = await EnsureUserAsync(
                email: "admin@abp.io",
                userName: "admin",
                password: "Admin123!*"
            );
            await EnsureInRoleAsync(admin, "admin");

            var teacher = await EnsureUserAsync(
                email: "teacher1@local",
                userName: "teacher1@local",
                password: "Teacher123!*"
            );
            await EnsureInRoleAsync(teacher, "teacher");

            var student = await EnsureUserAsync(
                email: "student1@local",
                userName: "student1@local",
                password: "Student123!*"
            );
            await EnsureInRoleAsync(student, "student");

            // 3) izinler
            var tenantId = context?.TenantId;
            var provider = RolePermissionValueProvider.ProviderName;

            // admin = hepsi
            await _permissionSeeder.SeedAsync(
                provider, "admin",
                new[]
                {
                    AutomationPermissions.Students.Default,
                    AutomationPermissions.Students.Create,
                    AutomationPermissions.Students.Edit,
                    AutomationPermissions.Students.Delete,

                    AutomationPermissions.Teachers.Default,
                    AutomationPermissions.Teachers.Create,
                    AutomationPermissions.Teachers.Edit,
                    AutomationPermissions.Teachers.Delete,

                    AutomationPermissions.Courses.Default,
                    AutomationPermissions.Courses.Create,
                    AutomationPermissions.Courses.Edit,
                    AutomationPermissions.Courses.Delete,

                    AutomationPermissions.Enrollments.Default,
                    AutomationPermissions.Enrollments.Create,
                    AutomationPermissions.Enrollments.Edit,
                    AutomationPermissions.Enrollments.Delete,

                    AutomationPermissions.Grades.Default,
                    AutomationPermissions.Grades.Create,
                    AutomationPermissions.Grades.Edit,
                    AutomationPermissions.Grades.Delete,

                    AutomationPermissions.Attendances.Default,
                    AutomationPermissions.Attendances.Create,
                    AutomationPermissions.Attendances.Edit,
                    AutomationPermissions.Attendances.Delete,
                },
                tenantId
            );

            // teacher
            await _permissionSeeder.SeedAsync(
                provider, "teacher",
                new[]
                {
                    // öğrencileri listelesin
                    AutomationPermissions.Students.Default,

                    // BURASI YENİ: course sayfası öğretmenleri çekerken patlamasın diye
                    AutomationPermissions.Teachers.Default,

                    // kendi dersleri
                    AutomationPermissions.Courses.Default,
                    AutomationPermissions.Courses.Create,
                    AutomationPermissions.Courses.Edit,
                    AutomationPermissions.Courses.Delete,

                    AutomationPermissions.Enrollments.Default,
                    AutomationPermissions.Enrollments.Create,
                    AutomationPermissions.Enrollments.Edit,
                    AutomationPermissions.Enrollments.Delete,

                    AutomationPermissions.Grades.Default,
                    AutomationPermissions.Grades.Create,
                    AutomationPermissions.Grades.Edit,
                    AutomationPermissions.Grades.Delete,

                    AutomationPermissions.Attendances.Default,
                    AutomationPermissions.Attendances.Create,
                    AutomationPermissions.Attendances.Edit,
                    AutomationPermissions.Attendances.Delete,
                },
                tenantId
            );

            // student
            await _permissionSeeder.SeedAsync(
                provider, "student",
                new[]
                {
                    AutomationPermissions.Students.Default,
                    AutomationPermissions.Enrollments.Default,
                    AutomationPermissions.Grades.Default,
                    AutomationPermissions.Attendances.Default,
                },
                tenantId
            );
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(Guid.NewGuid(), roleName);
                (await _roleManager.CreateAsync(role)).CheckErrors();
            }
        }

        private async Task<IdentityUser> EnsureUserAsync(string email, string userName, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser(Guid.NewGuid(), userName, email);
                (await _userManager.CreateAsync(user)).CheckErrors();
                (await _userManager.AddPasswordAsync(user, password)).CheckErrors();
            }
            else
            {
                if (!string.Equals(user.UserName, userName, StringComparison.Ordinal))
                {
                    (await _userManager.SetUserNameAsync(user, userName)).CheckErrors();
                    (await _userManager.UpdateAsync(user)).CheckErrors();
                }

                if (!await _userManager.HasPasswordAsync(user))
                {
                    (await _userManager.AddPasswordAsync(user, password)).CheckErrors();
                }
            }

            (await _userManager.SetLockoutEnabledAsync(user, false)).CheckErrors();
            (await _userManager.ResetAccessFailedCountAsync(user)).CheckErrors();

            return user;
        }

        private async Task EnsureInRoleAsync(IdentityUser user, string roleName)
        {
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                (await _userManager.AddToRoleAsync(user, roleName)).CheckErrors();
            }
        }
    }
}
