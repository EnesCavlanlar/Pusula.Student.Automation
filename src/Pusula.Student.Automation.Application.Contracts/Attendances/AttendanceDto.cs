using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Attendances
{
    public class AttendanceDto : EntityDto<Guid>
    {
        public Guid EnrollmentId { get; set; }
        public DateTime Date { get; set; }   // UTC, gün bazlı
        public bool IsPresent { get; set; }
    }
}
