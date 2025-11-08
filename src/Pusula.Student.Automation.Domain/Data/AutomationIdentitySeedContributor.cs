using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

// İzin seeding
using Volo.Abp.PermissionManagement;                 // IPermissionDataSeeder
using Volo.Abp.Authorization.Permissions;            // RolePermissionValueProvider
using Pusula.Student.Automation.Permissions;         // AutomationPermissions

namespace Pusula.Student.Automation.Data;

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
        // 1) Roller
        await EnsureRoleAsync("admin");
        await EnsureRoleAsync("teacher");
        await EnsureRoleAsync("student");

        // 2) Admin kullanıcılar (mevcut kurgu)
        var u1 = await EnsureUserAsync("admin@student.local", "admin@student.local", "Admin123!*");
        await EnsureInRoleAsync(u1, "admin");

        var u2 = await EnsureUserAsync("admin@admin.local", "admin@admin.local", "Admin123!*");
        await EnsureInRoleAsync(u2, "admin");

        var u3 = await EnsureUserAsync("admin@teacher.local", "admin@teacher.local", "Admin123!*");
        await EnsureInRoleAsync(u3, "admin");

        // 3) Test öğretmen ve öğrenci kullanıcıları
        var t1 = await EnsureUserAsync("teacher1@local", "teacher1@local", "Teacher123!*");
        await EnsureInRoleAsync(t1, "teacher");

        var s1 = await EnsureUserAsync("student1@local", "student1@local", "Student123!*");
        await EnsureInRoleAsync(s1, "student");

        // 4) Rol → İzin seeding
        var tenantId = context?.TenantId;
        var providerName = RolePermissionValueProvider.ProviderName;

        // Admin = tüm Automation.*
        await _permissionSeeder.SeedAsync(
            providerName, "admin",
            new[]
            {
                // Students
                AutomationPermissions.Students.Default,
                AutomationPermissions.Students.Create,
                AutomationPermissions.Students.Edit,
                AutomationPermissions.Students.Delete,

                // Teachers
                AutomationPermissions.Teachers.Default,
                AutomationPermissions.Teachers.Create,
                AutomationPermissions.Teachers.Edit,
                AutomationPermissions.Teachers.Delete,

                // Courses
                AutomationPermissions.Courses.Default,
                AutomationPermissions.Courses.Create,
                AutomationPermissions.Courses.Edit,
                AutomationPermissions.Courses.Delete,

                // Enrollments
                AutomationPermissions.Enrollments.Default,
                AutomationPermissions.Enrollments.Create,
                AutomationPermissions.Enrollments.Edit,
                AutomationPermissions.Enrollments.Delete,

                // Grades
                AutomationPermissions.Grades.Default,
                AutomationPermissions.Grades.Create,
                AutomationPermissions.Grades.Edit,
                AutomationPermissions.Grades.Delete,

                // Attendances
                AutomationPermissions.Attendances.Default,
                AutomationPermissions.Attendances.Create,
                AutomationPermissions.Attendances.Edit,
                AutomationPermissions.Attendances.Delete
            },
            tenantId
        );

        // Teacher = Courses/Enrollments/Grades/Attendances → Full
        await _permissionSeeder.SeedAsync(
            providerName, "teacher",
            new[]
            {
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
                AutomationPermissions.Attendances.Delete
            },
            tenantId
        );

        // Student = Enrollments/Grades/Attendances → sadece Default
        await _permissionSeeder.SeedAsync(
            providerName, "student",
            new[]
            {
                AutomationPermissions.Enrollments.Default,
                AutomationPermissions.Grades.Default,
                AutomationPermissions.Attendances.Default
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
        }
        else
        {
            if (!string.Equals(user.UserName, userName, StringComparison.Ordinal))
            {
                (await _userManager.SetUserNameAsync(user, userName)).CheckErrors();
                (await _userManager.UpdateAsync(user)).CheckErrors();
            }
            (await _userManager.SetEmailAsync(user, email)).CheckErrors();
        }

        (await _userManager.SetLockoutEnabledAsync(user, false)).CheckErrors();
        (await _userManager.ResetAccessFailedCountAsync(user)).CheckErrors();

        if (await _userManager.HasPasswordAsync(user))
            (await _userManager.RemovePasswordAsync(user)).CheckErrors();

        (await _userManager.AddPasswordAsync(user, password)).CheckErrors();

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
