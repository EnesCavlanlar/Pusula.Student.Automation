using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Enrollments
{
    public class CreateEnrollmentDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        public DateTime? EnrollmentDate { get; set; } // boş gelirse AppService tarafında UtcNow veriyoruz
    }
}
