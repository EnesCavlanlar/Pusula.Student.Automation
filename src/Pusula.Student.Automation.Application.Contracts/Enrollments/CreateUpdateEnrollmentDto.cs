using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Enrollments
{
    public class CreateUpdateEnrollmentDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid CourseId { get; set; }
    }
}
