using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Enrollments
{
    public interface IEnrollmentAppService :
        ICrudAppService<
            EnrollmentDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateEnrollmentDto>
    {
    }
}
