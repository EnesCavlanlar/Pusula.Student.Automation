using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Attendances
{
    public class CreateUpdateAttendanceDto
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }   // Saat gönderilse de AppService'te .Date'e normalize edeceğiz.

        public bool IsPresent { get; set; }
    }
}
