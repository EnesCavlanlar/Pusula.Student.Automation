using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

// alias
using AttendanceEntity = Pusula.Student.Automation.Attendances.Attendance;

using Pusula.Student.Automation.Attendances;
using Pusula.Student.Automation.Permissions;

namespace Pusula.Student.Automation.Attendances
{
    public class AttendanceAppService :
        CrudAppService<
            AttendanceEntity,
            AttendanceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAttendanceDto>
    {
        public AttendanceAppService(IRepository<AttendanceEntity, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AutomationPermissions.Attendances.Default;
            GetListPolicyName = AutomationPermissions.Attendances.Default;
            CreatePolicyName = AutomationPermissions.Attendances.Create;
            UpdatePolicyName = AutomationPermissions.Attendances.Edit;
            DeletePolicyName = AutomationPermissions.Attendances.Delete;
        }

        public override async Task<AttendanceDto> CreateAsync(CreateUpdateAttendanceDto input)
        {
            var entity = new AttendanceEntity(
                GuidGenerator.Create(),
                input.EnrollmentId,
                input.Date.Date,      // normalize: günü baz al
                input.IsPresent
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        public override async Task<AttendanceDto> UpdateAsync(Guid id, CreateUpdateAttendanceDto input)
        {
            var entity = await Repository.GetAsync(id);
            entity.SetDate(input.Date.Date);
            entity.SetPresence(input.IsPresent);

            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }
    }
}
