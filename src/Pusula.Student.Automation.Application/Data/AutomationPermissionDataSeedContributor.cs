using Pusula.Student.Automation.Identity;
using Pusula.Student.Automation.Permissions;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace Pusula.Student.Automation.Data
{
    /// <summary>
    /// Uygulama katmanında: Admin rolüne Automation izinlerini seed eder.
    /// </summary>
    public class AutomationPermissionDataSeedContributor :
        IDataSeedContributor, ITransientDependency
    {
        private readonly IPermissionDataSeeder _permissionDataSeeder;
        private readonly ICurrentTenant _currentTenant;

        public AutomationPermissionDataSeedContributor(
            IPermissionDataSeeder permissionDataSeeder,
            ICurrentTenant currentTenant)
        {
            _permissionDataSeeder = permissionDataSeeder;
            _currentTenant = currentTenant;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            using (_currentTenant.Change(context?.TenantId))
            {
                await _permissionDataSeeder.SeedAsync(
                    RolePermissionValueProvider.ProviderName,          // providerName
                    StudentAutomationRoleNames.Admin,                  // providerKey (rol adı)
                    new[]                                              // grantedPermissions
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
                    context?.TenantId                                  // tenantId
                );
            }
        }
    }
}
