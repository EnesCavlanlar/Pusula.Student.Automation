using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// Alias: entity ile namespace çakışmasını önle
using CourseEntity = Pusula.Student.Automation.Courses.Course;

using Pusula.Student.Automation.Courses;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Courses
{
    public class CourseAppService :
        CrudAppService<
            CourseEntity,                 // Entity
            CourseDto,                    // DTO (return)
            Guid,                         // PK
            PagedAndSortedResultRequestDto,
            CreateUpdateCourseDto>        // Create/Update DTO
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
