using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Grades
{
    public class GradeDto : EntityDto<Guid>
    {
        public Guid EnrollmentId { get; set; }
        public int GradeValue { get; set; }    // 0-100
        public string? Note { get; set; }
    }
}
