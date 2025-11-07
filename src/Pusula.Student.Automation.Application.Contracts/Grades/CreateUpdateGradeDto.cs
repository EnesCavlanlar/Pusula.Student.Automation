using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Grades
{
    public class CreateUpdateGradeDto
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [Range(0, 100)]
        public int GradeValue { get; set; }

        [StringLength(512)]
        public string? Note { get; set; }
    }
}
