using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Teachers
{
    public class TeacherDto : EntityDto<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Department { get; set; }
    }
}
