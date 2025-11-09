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
using Volo.Abp.Users; // <-- eklendi

namespace Pusula.Student.Automation.Blazor.Menus
{
    public class AutomationMenuContributor : IMenuContributor
    {
        public async Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name != StandardMenus.Main)
                return;

            var sp = context.ServiceProvider;
            var checker = sp.GetRequiredService<IPermissionChecker>();
            var currentUser = sp.GetRequiredService<ICurrentUser>();

            // rol bayrakları
            var isAdmin = currentUser.IsInRole("admin");
            var isTeacher = currentUser.IsInRole("teacher");
            var isStudent = currentUser.IsInRole("student");

            // Home
            context.Menu.Items.Insert(
                0,
                new ApplicationMenuItem(
                    AutomationMenus.Home,
                    "Home",
                    url: "/",
                    icon: "fas fa-home",
                    order: 1
                )
            );

            // ana grup
            var automation = new ApplicationMenuItem(
                name: "Automation",
                displayName: "Öğrenci Otomasyon",
                icon: "fa fa-graduation-cap",
                order: 10
            );

            //
            // 1) ADMIN BLOĞU – sadece admin rolü görsün
            //
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

            //
            // 2) TEACHER BLOĞU – teacher veya admin görsün
            //
            if (isTeacher || isAdmin)
            {
                if (await checker.IsGrantedAsync(AutomationPermissions.Courses.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.TeacherMyCourses",
                        displayName: "Derslerim",
                        url: "/teacher/my-courses",
                        icon: "fa fa-laptop",
                        order: 20
                    ));
                }

                if (await checker.IsGrantedAsync(AutomationPermissions.Enrollments.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.TeacherCourseStudents",
                        displayName: "Ders Öğrencilerim",
                        url: "/teacher/course-students",
                        icon: "fa fa-users",
                        order: 21
                    ));
                }

                if (await checker.IsGrantedAsync(AutomationPermissions.Grades.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.TeacherGrades",
                        displayName: "Not Girişi",
                        url: "/teacher/grades",
                        icon: "fa fa-pen",
                        order: 22
                    ));
                }

                if (await checker.IsGrantedAsync(AutomationPermissions.Attendances.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.TeacherAttendance",
                        displayName: "Bugünün Yoklaması",
                        url: "/teacher/attendance",
                        icon: "fa fa-calendar-day",
                        order: 23
                    ));
                }
            }

            //
            // 3) STUDENT BLOĞU – student veya admin görsün
            //
            if (isStudent || isAdmin)
            {
                if (await checker.IsGrantedAsync(AutomationPermissions.Grades.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.MyGrades",
                        displayName: "Notlarım",
                        url: "/student/my-grades",
                        icon: "fa fa-user-graduate",
                        order: 30
                    ));
                }

                if (await checker.IsGrantedAsync(AutomationPermissions.Attendances.Default))
                {
                    automation.AddItem(new ApplicationMenuItem(
                        name: "Automation.MyAttendance",
                        displayName: "Yoklamalarım",
                        url: "/student/my-attendance",
                        icon: "fa fa-calendar-day",
                        order: 31
                    ));
                }
            }

            if (automation.Items.Count > 0)
            {
                context.Menu.AddItem(automation);
            }

            // ABP default admin menüsü
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
