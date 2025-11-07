using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Pusula.Student.Automation.Permissions;

// 🔴 ALIAS: Entity ismi ile namespace çakışmasını önler
using CourseEntity = Pusula.Student.Automation.Courses.Course;

namespace Pusula.Student.Automation.Courses
{
    public class CourseAppService :
        CrudAppService<
            CourseEntity,                 // Entity (alias)
            CourseDto,                    // DTO
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateCourseDto>,
        ICourseAppService
    {
        public CourseAppService(IRepository<CourseEntity, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Courses.Default;
            GetListPolicyName = AutomationPermissions.Courses.Default;
            CreatePolicyName = AutomationPermissions.Courses.Create;
            UpdatePolicyName = AutomationPermissions.Courses.Edit;
            DeletePolicyName = AutomationPermissions.Courses.Delete;
        }
    }
}
