// ... using'ler aynı
using Pusula.Student.Automation.Permissions;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Student.Automation.Teachers
{
    public class TeacherAppService :
        CrudAppService<
            Teacher,                 // Entity
            TeacherDto,              // DTO
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateTeacherDto>,
        ITeacherAppService          // <-- EKLEDİK
    {
        public TeacherAppService(IRepository<Teacher, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Teachers.Default;
            GetListPolicyName = AutomationPermissions.Teachers.Default;
            CreatePolicyName = AutomationPermissions.Teachers.Create;
            UpdatePolicyName = AutomationPermissions.Teachers.Edit;
            DeletePolicyName = AutomationPermissions.Teachers.Delete;
        }
    }
}
