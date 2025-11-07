using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Attendances
{
    /// <summary>
    /// Bir Enrollment için belirli bir tarihte yoklama durumu.
    /// Aynı Enrollment + Date için tek kayıt olması hedeflenir.
    /// </summary>
    public class Attendance : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; protected set; }

        public Guid EnrollmentId { get; protected set; }
        public DateTime Date { get; protected set; } // yalnız gün bazlı kullanacaksan Date.Date
        public bool IsPresent { get; protected set; }

        protected Attendance() { }

        public Attendance(Guid id, Guid enrollmentId, DateTime date, bool isPresent, Guid? tenantId = null)
            : base(id)
        {
            if (enrollmentId == Guid.Empty)
                throw new ArgumentException("EnrollmentId is required.", nameof(enrollmentId));

            EnrollmentId = enrollmentId;
            Date = date.Date; // normalize
            IsPresent = isPresent;
            TenantId = tenantId;
        }

        public void SetPresence(bool present) => IsPresent = present;
        public void SetDate(DateTime date) => Date = date.Date;
    }
}
