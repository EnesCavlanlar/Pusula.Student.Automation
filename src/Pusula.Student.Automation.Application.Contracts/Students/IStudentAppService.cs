using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Students
{
    public interface IStudentAppService :
        ICrudAppService<
            StudentDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateStudentDto>
    {
    }
}
