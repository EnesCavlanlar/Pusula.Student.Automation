using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// entity alias: isim çakışmasını önler
using StudentEntity = Pusula.Student.Automation.Students.Student;

using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Students
{
    public class StudentAppService :
        CrudAppService<
            StudentEntity,                // Entity
            StudentDto,                   // Return DTO
            Guid,                         // PK
            PagedAndSortedResultRequestDto,
            CreateUpdateStudentDto>,      // Create/Update DTO
        IStudentAppService                 // <-- interface eklendi
    {
        public StudentAppService(IRepository<StudentEntity, Guid> repository)
            : base(repository)
        {
            // İzin politikaları
            GetPolicyName = AutomationPermissions.Students.Default;
            GetListPolicyName = AutomationPermissions.Students.Default;
            CreatePolicyName = AutomationPermissions.Students.Create;
            UpdatePolicyName = AutomationPermissions.Students.Edit;
            DeletePolicyName = AutomationPermissions.Students.Delete;
        }
    }
}
