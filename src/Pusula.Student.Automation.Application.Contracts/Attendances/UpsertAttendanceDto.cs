using System;

namespace Pusula.Student.Automation.Attendances.Dtos
{
    public class UpsertAttendanceDto
    {
        public Guid EnrollmentId { get; set; }

        // sadece gün bilgisi yeterli, saat önemli değil
        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
    }
}
