using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Pusula.Student.Automation.Teachers;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Teachers
{
    public class TeacherAppService :
        CrudAppService<
            Teacher,                 // Entity
            TeacherDto,              // DTO to return
            Guid,                    // PK
            PagedAndSortedResultRequestDto, // paging/sorting input
            CreateUpdateTeacherDto>  // create/update DTO
    {
        public TeacherAppService(IRepository<Teacher, Guid> repository)
            : base(repository)
        {
            // permissions (istersen şimdi açarsın, yoksa sonra)
            GetPolicyName = AutomationPermissions.Teachers.Default;
            GetListPolicyName = AutomationPermissions.Teachers.Default;
            CreatePolicyName = AutomationPermissions.Teachers.Create;
            UpdatePolicyName = AutomationPermissions.Teachers.Edit;
            DeletePolicyName = AutomationPermissions.Teachers.Delete;
        }
    }
}
