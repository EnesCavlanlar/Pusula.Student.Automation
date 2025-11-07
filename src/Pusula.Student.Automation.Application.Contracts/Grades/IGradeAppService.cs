using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Grades
{
    public interface IGradeAppService :
        ICrudAppService<
            GradeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateGradeDto>
    {
    }
}
