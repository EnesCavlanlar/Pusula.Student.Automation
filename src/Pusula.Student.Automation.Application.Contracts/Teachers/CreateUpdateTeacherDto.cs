using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Teachers
{
    public class CreateUpdateTeacherDto
    {
        [Required, StringLength(128)]
        public string FirstName { get; set; } = default!;

        [Required, StringLength(128)]
        public string LastName { get; set; } = default!;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = default!;

        [StringLength(128)]
        public string? Department { get; set; }
    }
}
