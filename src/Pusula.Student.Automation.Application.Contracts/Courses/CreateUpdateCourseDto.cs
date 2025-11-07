using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Courses
{
    public class CreateUpdateCourseDto
    {
        [Required, StringLength(128)]
        public string Name { get; set; } = default!;

        [Required, StringLength(32)]
        public string Code { get; set; } = default!;

        public int? Credit { get; set; }

        [Required]
        public Guid TeacherId { get; set; }
    }
}
