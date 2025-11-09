using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Enrollments.Dtos
{
    public class CourseStudentDto : EntityDto<Guid>
    {
        public Guid EnrollmentId { get; set; }
        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;
        public string? StudentNo { get; set; }

        public Guid CourseId { get; set; }
    }
}
