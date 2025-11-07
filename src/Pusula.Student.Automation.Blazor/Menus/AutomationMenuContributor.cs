using System.Threading.Tasks;
using Pusula.Student.Automation.Localization;
using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.MultiTenancy;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.Identity.Blazor;

namespace Pusula.Student.Automation.Blazor.Menus;

public class AutomationMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<AutomationResource>();

        // Home
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                AutomationMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 1
            )
        );

        // Students
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Students",
                l["Menu:Students"],
                url: "/admin/students",
                icon: "fa fa-users",
                order: 2,
                requiredPermissionName: AutomationPermissions.Students.Default
            )
        );

        // Teachers
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Teachers",
                l["Menu:Teachers"],
                url: "/admin/teachers",
                icon: "fa fa-chalkboard-teacher",
                order: 3,
                requiredPermissionName: AutomationPermissions.Teachers.Default
            )
        );

        // Courses
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Courses",
                l["Menu:Courses"],
                url: "/admin/courses",
                icon: "fa fa-book",
                order: 4,
                requiredPermissionName: AutomationPermissions.Courses.Default
            )
        );

        // Enrollments
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Enrollments",
                l["Menu:Enrollments"],
                url: "/admin/enrollments",
                icon: "fa fa-link",
                order: 5,
                requiredPermissionName: AutomationPermissions.Enrollments.Default
            )
        );

        // Grades
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Grades",
                l["Menu:Grades"],
                url: "/admin/grades",
                icon: "fa fa-percent",
                order: 6,
                requiredPermissionName: AutomationPermissions.Grades.Default
            )
        );

        // Attendance
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Automation.Attendance",
                l["Menu:Attendance"],
                url: "/admin/attendance",
                icon: "fa fa-calendar-check",
                order: 7,
                requiredPermissionName: AutomationPermissions.Attendances.Default
            )
        );

        // Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 8;

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        return Task.CompletedTask;
    }
}
