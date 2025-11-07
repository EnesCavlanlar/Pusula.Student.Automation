using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Grades
{
    /// <summary>
    /// Bir Enrollment (öğrencinin derse kaydı) için verilen not.
    /// İster tek not, ister birden fazla not (vize/final) olarak genişletilebilir.
    /// </summary>
    public class Grade : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; protected set; }

        public Guid EnrollmentId { get; protected set; }
        public int GradeValue { get; protected set; } // 0-100
        public string? Note { get; protected set; } // açıklama

        protected Grade() { }

        public Grade(Guid id, Guid enrollmentId, int gradeValue, string? note = null, Guid? tenantId = null)
            : base(id)
        {
            if (enrollmentId == Guid.Empty)
                throw new ArgumentException("EnrollmentId is required.", nameof(enrollmentId));

            SetGradeValue(gradeValue);
            EnrollmentId = enrollmentId;
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            TenantId = tenantId;
        }

        public void SetGradeValue(int value)
        {
            if (value is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Grade must be between 0 and 100.");
            GradeValue = value;
        }
    }
}
