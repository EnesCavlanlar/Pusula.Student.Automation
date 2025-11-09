using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Student.Automation.Grades.Dtos
{
    public class UpsertGradeDto : EntityDto<Guid?>
    {
        public Guid EnrollmentId { get; set; }
        public int Score { get; set; }
        public string? Note { get; set; }
    }
}
