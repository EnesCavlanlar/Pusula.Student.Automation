using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// alias: entity ile namespace çakışmasını önle
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;

using Pusula.Student.Automation.Enrollments;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Enrollments
{
    public class EnrollmentAppService :
        CrudAppService<
            EnrollmentEntity,              // Entity
            EnrollmentDto,                 // Return DTO
            Guid,                          // PK
            PagedAndSortedResultRequestDto,
            CreateEnrollmentDto>           // Create DTO
    {
        public EnrollmentAppService(IRepository<EnrollmentEntity, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Enrollments.Default;
            GetListPolicyName = AutomationPermissions.Enrollments.Default;
            CreatePolicyName = AutomationPermissions.Enrollments.Create;
            UpdatePolicyName = AutomationPermissions.Enrollments.Edit;   // şu an kullanılmıyor ama dursun
            DeletePolicyName = AutomationPermissions.Enrollments.Delete;
        }

        public override async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input)
        {
            // EnrollmentDate boşsa UtcNow ver
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
