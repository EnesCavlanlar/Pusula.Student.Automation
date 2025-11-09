using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Pusula.Student.Automation.Enrollments.Dtos;

namespace Pusula.Student.Automation.Enrollments
{
    public interface IEnrollmentAppService :
        ICrudAppService<
            EnrollmentDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateEnrollmentDto>
    {
        // öğretmenin seçtiği dersteki öğrencileri getir
        Task<List<CourseStudentDto>> GetStudentsByCourseAsync(Guid courseId);
    }
}
