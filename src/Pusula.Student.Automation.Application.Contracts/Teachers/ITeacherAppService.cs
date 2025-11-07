using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Teachers
{
    public interface ITeacherAppService :
        ICrudAppService<
            TeacherDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateTeacherDto>
    {
    }
}
