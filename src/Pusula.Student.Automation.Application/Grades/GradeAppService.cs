using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// alias: entity ile namespace çakışmasını önle
using GradeEntity = Pusula.Student.Automation.Grades.Grade;

using Pusula.Student.Automation.Grades;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Grades
{
    public class GradeAppService :
        CrudAppService<
            GradeEntity,
            GradeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateGradeDto>
    {
        public GradeAppService(IRepository<GradeEntity, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Grades.Default;
            GetListPolicyName = AutomationPermissions.Grades.Default;
            CreatePolicyName = AutomationPermissions.Grades.Create;
            UpdatePolicyName = AutomationPermissions.Grades.Edit;
            DeletePolicyName = AutomationPermissions.Grades.Delete;
        }
    }
}
