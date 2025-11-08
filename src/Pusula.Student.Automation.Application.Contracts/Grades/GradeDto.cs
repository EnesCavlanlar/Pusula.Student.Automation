using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Grades
{
    public class GradeDto : AuditedEntityDto<Guid>
    {
        public Guid EnrollmentId { get; set; }

        // UI ile uyumlu isim
        public int Score { get; set; }          // 0–100

        public string? Note { get; set; }
    }
}
