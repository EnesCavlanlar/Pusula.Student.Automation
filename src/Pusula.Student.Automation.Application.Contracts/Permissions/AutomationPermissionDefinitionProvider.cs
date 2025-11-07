using Pusula.Student.Automation.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Pusula.Student.Automation.Permissions
{
    public class AutomationPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var group = context.AddGroup(AutomationPermissions.GroupName, L("Permission:Automation"));

            // Students
            var students = group.AddPermission(AutomationPermissions.Students.Default, L("Permission:Students"));
            students.AddChild(AutomationPermissions.Students.Create, L("Permission:Students.Create"));
            students.AddChild(AutomationPermissions.Students.Edit, L("Permission:Students.Edit"));
            students.AddChild(AutomationPermissions.Students.Delete, L("Permission:Students.Delete"));

            // Teachers
            var teachers = group.AddPermission(AutomationPermissions.Teachers.Default, L("Permission:Teachers"));
            teachers.AddChild(AutomationPermissions.Teachers.Create, L("Permission:Teachers.Create"));
            teachers.AddChild(AutomationPermissions.Teachers.Edit, L("Permission:Teachers.Edit"));
            teachers.AddChild(AutomationPermissions.Teachers.Delete, L("Permission:Teachers.Delete"));

            // Courses
            var courses = group.AddPermission(AutomationPermissions.Courses.Default, L("Permission:Courses"));
            courses.AddChild(AutomationPermissions.Courses.Create, L("Permission:Courses.Create"));
            courses.AddChild(AutomationPermissions.Courses.Edit, L("Permission:Courses.Edit"));
            courses.AddChild(AutomationPermissions.Courses.Delete, L("Permission:Courses.Delete"));

            // Enrollments
            var enrollments = group.AddPermission(AutomationPermissions.Enrollments.Default, L("Permission:Enrollments"));
            enrollments.AddChild(AutomationPermissions.Enrollments.Create, L("Permission:Enrollments.Create"));
            enrollments.AddChild(AutomationPermissions.Enrollments.Edit, L("Permission:Enrollments.Edit"));
            enrollments.AddChild(AutomationPermissions.Enrollments.Delete, L("Permission:Enrollments.Delete"));

            // Grades
            var grades = group.AddPermission(AutomationPermissions.Grades.Default, L("Permission:Grades"));
            grades.AddChild(AutomationPermissions.Grades.Create, L("Permission:Grades.Create"));
            grades.AddChild(AutomationPermissions.Grades.Edit, L("Permission:Grades.Edit"));
            grades.AddChild(AutomationPermissions.Grades.Delete, L("Permission:Grades.Delete"));

            // Attendances
            var attendances = group.AddPermission(AutomationPermissions.Attendances.Default, L("Permission:Attendances"));
            attendances.AddChild(AutomationPermissions.Attendances.Create, L("Permission:Attendances.Create"));
            attendances.AddChild(AutomationPermissions.Attendances.Edit, L("Permission:Attendances.Edit"));
            attendances.AddChild(AutomationPermissions.Attendances.Delete, L("Permission:Attendances.Delete"));
        }

        private static LocalizableString L(string name)
            => LocalizableString.Create<AutomationResource>(name);
    }
}
