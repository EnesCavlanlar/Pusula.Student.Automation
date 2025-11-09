using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Pusula.Student.Automation.Attendances.Dtos;

namespace Pusula.Student.Automation.Attendances
{
    public interface IAttendanceAppService :
        ICrudAppService<
            AttendanceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAttendanceDto>
    {
        // tek gün + tek enrollment için yoksa oluştur, varsa güncelle
        Task<AttendanceDto> UpsertAsync(UpsertAttendanceDto input);

        // admin/teacher ekranı için: bir kursun belli gündeki yoklamaları
        Task<List<AttendanceDto>> GetListByCourseAndDateAsync(Guid courseId, DateTime date);
    }
}
