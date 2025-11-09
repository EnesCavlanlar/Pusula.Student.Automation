using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        // öğretmenin sadece kendi derslerini çeksin
        Task<List<CourseDto>> GetMyCoursesAsync();
    }
}
