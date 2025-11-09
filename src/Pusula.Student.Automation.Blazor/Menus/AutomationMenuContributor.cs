using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pusula.Student.Automation.Localization;
using Pusula.Student.Automation.MultiTenancy;
using Pusula.Student.Automation.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;

namespace Pusula.Student.Automation.Blazor.Menus
{
    public class AutomationMenuContributor : IMenuContributor
    {
        public async Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name != StandardMenus.Main)
                return;

            var l = context.GetLocalizer<AutomationResource>();
            var checker = context.ServiceProvider.GetRequiredService<IPermissionChecker>();
            var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();

            // ---------- Home ----------
            context.Menu.Items.Insert(
                0,
                new ApplicationMenuItem(
                    AutomationMenus.Home,
                    l["Menu:Home"],
                    url: "/",
                    icon: "fas fa-home",
                    order: 1
                )
            );

            // ortak grup
            var automation = new ApplicationMenuItem(
                name: "Automation",
                displayName: l["Menu:Automation"] ?? "Automation",
                icon: "fa fa-graduation-cap",
                order: 10
            );

            // =============== ADMIN MENÜSÜ ===============
            if (currentUser.IsInRole("admin"))
            {
                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Students.Default,
                    name: "Automation.Students", display: l["Menu:Students"] ?? "Students",
                    url: "/admin/students", icon: "fa fa-users", order: 1);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Teachers.Default,
                    name: "Automation.Teachers", display: l["Menu:Teachers"] ?? "Teachers",
                    url: "/admin/teachers", icon: "fa fa-chalkboard-teacher", order: 2);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Courses.Default,
                    name: "Automation.Courses", display: l["Menu:Courses"] ?? "Courses",
                    url: "/admin/courses", icon: "fa fa-book", order: 3);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Enrollments.Default,
                    name: "Automation.Enrollments", display: l["Menu:Enrollments"] ?? "Enrollments",
                    url: "/admin/enrollments", icon: "fa fa-link", order: 4);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Grades.Default,
                    name: "Automation.Grades", display: l["Menu:Grades"] ?? "Grades",
                    url: "/admin/grades", icon: "fa fa-percent", order: 5);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Attendances.Default,
                    name: "Automation.Attendance", display: l["Menu:Attendance"] ?? "Attendance",
                    url: "/admin/attendance", icon: "fa fa-calendar-check", order: 6);
            }
            // =============== TEACHER MENÜSÜ ===============
            else if (currentUser.IsInRole("teacher"))
            {
                // kendi dersleri
                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.TeacherMyCourses",
                    displayName: l["Menu:TeacherMyCourses"] ?? "My Courses",
                    url: "/teacher/my-courses",
                    icon: "fa fa-chalkboard",
                    order: 1
                ));

                // not girişi ekranı
                if (await checker.IsGrantedAsync(AutomationPermissions.Grades.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.Grades",
                        displayName: l["Menu:Grades"] ?? "Grades",
                        url: "/admin/grades",
                        icon: "fa fa-percent",
                        order: 2
                    ));
                }

                // yoklama girişi
                if (await checker.IsGrantedAsync(AutomationPermissions.Attendances.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.Attendance",
                        displayName: l["Menu:Attendance"] ?? "Attendance",
                        url: "/admin/attendance",
                        icon: "fa fa-calendar-check",
                        order: 3
                    ));
                }
            }
            // =============== STUDENT MENÜSÜ ===============
            else if (currentUser.IsInRole("student"))
            {
                if (await checker.IsGrantedAsync(AutomationPermissions.Grades.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.MyGrades",
                        displayName: l["Menu:MyGrades"] ?? "My Grades",
                        url: "/student/my-grades",
                        icon: "fa fa-user-graduate",
                        order: 1
                    ));
                }

                if (await checker.IsGrantedAsync(AutomationPermissions.Attendances.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.MyAttendance",
                        displayName: l["Menu:MyAttendance"] ?? "My Attendance",
                        url: "/student/my-attendance",
                        icon: "fa fa-calendar-day",
                        order: 2
                    ));
                }
            }

            // boş değilse ekle
            if (automation.Items.Count > 0)
                context.Menu.AddItem(automation);

            // ---------- Administration (ABP default) ----------
            var administration = context.Menu.GetAdministration();
            administration.Order = 99;

            if (MultiTenancyConsts.IsEnabled)
                administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
            else
                administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);

            administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
            administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);
        }

        private static async Task AddIfGrantedAsync(
            ApplicationMenuItem parent,
            IPermissionChecker checker,
            string permission,
            string name,
            string display,
            string url,
            string icon,
            int order)
        {
            if (await checker.IsGrantedAsync(permission))
            {
                parent.AddItem(new ApplicationMenuItem(
                    name: name,
                    displayName: display,
                    url: url,
                    icon: icon,
                    order: order
                ));
            }
        }
    }
}
