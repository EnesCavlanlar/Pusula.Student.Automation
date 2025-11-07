using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Courses
{
    public class CourseDto : EntityDto<Guid>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public int? Credit { get; set; }

        public Guid TeacherId { get; set; }   // ilişki
    }
}
