using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Enrollments
{
    public class EnrollmentDto : EntityDto<Guid>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
