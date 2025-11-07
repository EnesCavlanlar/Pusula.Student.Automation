using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Pusula.Student.Automation.Courses
{
    public interface ICourseAppService :
        ICrudAppService<
            CourseDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateCourseDto>
    {
    }
}
