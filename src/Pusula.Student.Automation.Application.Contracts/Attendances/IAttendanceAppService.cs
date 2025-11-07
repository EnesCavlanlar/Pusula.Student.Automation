using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Attendances
{
    public interface IAttendanceAppService :
        ICrudAppService<
            AttendanceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAttendanceDto>
    {
    }
}
