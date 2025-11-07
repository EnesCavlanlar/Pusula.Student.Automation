using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Students
{
    public class StudentDto : EntityDto<Guid>
    {
        public string StudentNo { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime? BirthDate { get; set; }
    }
}
