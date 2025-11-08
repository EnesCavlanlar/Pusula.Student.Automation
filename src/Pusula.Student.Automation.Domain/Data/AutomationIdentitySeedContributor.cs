using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Pusula.Student.Automation.Data;

public class AutomationIdentitySeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    public AutomationIdentitySeedContributor(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        // Roller
        await EnsureRoleAsync("admin");
        await EnsureRoleAsync("teacher");
        await EnsureRoleAsync("student");

        // 1) Student admin
        var u1 = await EnsureUserAsync(
            email: "admin@student.local",
            userName: "admin@student.local",
            password: "Admin123!*");
        await EnsureInRoleAsync(u1, "admin");

        // 2) Global admin
        var u2 = await EnsureUserAsync(
            email: "admin@admin.local",
            userName: "admin@admin.local",
            password: "Admin123!*");
        await EnsureInRoleAsync(u2, "admin");

        // 3) Teacher admin
        var u3 = await EnsureUserAsync(
            email: "admin@teacher.local",
            userName: "admin@teacher.local",
            password: "Admin123!*");
        await EnsureInRoleAsync(u3, "admin");
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
            // GuidGenerator yerine Framework'ün kendi Guid üreticisi
            user = new IdentityUser(Guid.NewGuid(), userName, email);
            (await _userManager.CreateAsync(user)).CheckErrors();
        }
        else
        {
            // UserName değiştiyse güvenli şekilde güncelle
            if (!string.Equals(user.UserName, userName, StringComparison.Ordinal))
            {
                (await _userManager.SetUserNameAsync(user, userName)).CheckErrors();
                (await _userManager.UpdateAsync(user)).CheckErrors();
            }

            // Email’i normalize ederek güncelle
            (await _userManager.SetEmailAsync(user, email)).CheckErrors();
        }

        // Lockout kapalı + sayaç sıfır
        (await _userManager.SetLockoutEnabledAsync(user, false)).CheckErrors();
        (await _userManager.ResetAccessFailedCountAsync(user)).CheckErrors();

        // Parolayı tazele
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
