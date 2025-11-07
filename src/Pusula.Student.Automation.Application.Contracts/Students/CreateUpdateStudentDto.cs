using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Student.Automation.Students
{
    public class CreateUpdateStudentDto
    {
        [Required]
        [StringLength(64)]
        public string StudentNo { get; set; } = default!;

        [Required]
        [StringLength(128)]
        public string FirstName { get; set; } = default!;

        [Required]
        [StringLength(128)]
        public string LastName { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = default!;

        public DateTime? BirthDate { get; set; }
    }
}
