using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// Entity alias: isim/namespace çakışmasını önler
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;

using Pusula.Student.Automation.Enrollments;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Enrollments
{
    public class EnrollmentAppService :
        CrudAppService<
            EnrollmentEntity,               // Entity
            EnrollmentDto,                  // Return DTO
            Guid,                           // PK
            PagedAndSortedResultRequestDto, // Paging input
            CreateEnrollmentDto>,           // Create DTO
        IEnrollmentAppService               // <-- arayüz eklendi
    {
        public EnrollmentAppService(IRepository<EnrollmentEntity, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Enrollments.Default;
            GetListPolicyName = AutomationPermissions.Enrollments.Default;
            CreatePolicyName = AutomationPermissions.Enrollments.Create;
            UpdatePolicyName = AutomationPermissions.Enrollments.Edit;   // şu an kullanılmayabilir
            DeletePolicyName = AutomationPermissions.Enrollments.Delete;
        }

        public override async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input)
        {
            // Tarih boş gelirse Clock.Now (UTC) verelim
            var entity = new EnrollmentEntity(
                GuidGenerator.Create(),
                input.StudentId,
                input.CourseId,
                input.EnrollmentDate ?? Clock.Now
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }
    }
}
