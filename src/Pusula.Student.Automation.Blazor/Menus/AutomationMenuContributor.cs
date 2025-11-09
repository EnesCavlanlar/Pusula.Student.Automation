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

            // ---------- Home ----------
            context.Menu.Items.Insert(
                0,
                new ApplicationMenuItem(
                    AutomationMenus.Home,
                    "Ana Sayfa",
                    url: "/",
                    icon: "fas fa-home",
                    order: 1
                )
            );

            // Kullanıcının rolünü izinlerden yakalayacağız
            var isAdmin = await checker.IsGrantedAsync(AutomationPermissions.Students.Default)
                          && await checker.IsGrantedAsync(AutomationPermissions.Teachers.Default);
            // Çok kaba bir ayrım ama bizim senaryoya yeter.
            var isTeacher = !isAdmin && await checker.IsGrantedAsync(AutomationPermissions.Courses.Default);
            var isStudent = !isAdmin && !isTeacher;

            var automation = new ApplicationMenuItem(
                name: "Automation",
                displayName: "Öğrenci Otomasyonu",
                icon: "fa fa-graduation-cap",
                order: 10
            );

            if (isAdmin)
            {
                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Students.Default,
                    name: "Automation.Students", display: "Öğrenciler",
                    url: "/admin/students", icon: "fa fa-users", order: 1);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Teachers.Default,
                    name: "Automation.Teachers", display: "Öğretmenler",
                    url: "/admin/teachers", icon: "fa fa-chalkboard-teacher", order: 2);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Courses.Default,
                    name: "Automation.Courses", display: "Dersler",
                    url: "/admin/courses", icon: "fa fa-book", order: 3);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Enrollments.Default,
                    name: "Automation.Enrollments", display: "Ders Kayıtları",
                    url: "/admin/enrollments", icon: "fa fa-link", order: 4);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Grades.Default,
                    name: "Automation.Grades", display: "Notlar",
                    url: "/admin/grades", icon: "fa fa-percent", order: 5);

                await AddIfGrantedAsync(
                    automation, checker, AutomationPermissions.Attendances.Default,
                    name: "Automation.Attendance", display: "Yoklama",
                    url: "/admin/attendance", icon: "fa fa-calendar-check", order: 6);
            }
            else if (isTeacher)
            {
                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.TeacherMyCourses",
                    displayName: "Derslerim",
                    url: "/teacher/my-courses",
                    icon: "fa fa-laptop-code",
                    order: 1
                ));

                // yeni not girişi sayfamız
                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.TeacherGradeEntry",
                    displayName: "Not Girişi",
                    url: "/teacher/grade-entry",
                    icon: "fa fa-percent",
                    order: 2
                ));

                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.TeacherAttendance",
                    displayName: "Yoklama",
                    url: "/admin/attendance",      // aynı ekranı kullanıyoruz
                    icon: "fa fa-calendar-check",
                    order: 3
                ));
            }
            else if (isStudent)
            {
                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.MyGrades",
                    displayName: "Notlarım",
                    url: "/student/my-grades",
                    icon: "fa fa-user-graduate",
                    order: 1
                ));

                automation.AddItem(new ApplicationMenuItem(
                    name: "Automation.MyAttendance",
                    displayName: "Yoklamalarım",
                    url: "/student/my-attendance",
                    icon: "fa fa-calendar-day",
                    order: 2
                ));
            }

            if (automation.Items.Count > 0)
                context.Menu.AddItem(automation);

            // ----- Administration -----
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
