using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Pusula.Student.Automation.Grades.Dtos;

namespace Pusula.Student.Automation.Grades
{
    public interface IGradeAppService :
        ICrudAppService<
            GradeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateGradeDto>
    {
        // tek enrollment için upsert
        Task<GradeDto> UpsertAsync(UpsertGradeDto input);

        // course bazlı liste
        Task<List<GradeDto>> GetListByCourseAsync(Guid courseId);
    }
}
