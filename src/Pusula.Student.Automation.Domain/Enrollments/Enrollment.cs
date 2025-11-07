using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Enrollments
{
    /// <summary>
    /// Student - Course ilişki tablosu.
    /// Aynı öğrencinin aynı dersi bir kez alabilmesi için (TenantId, StudentId, CourseId) üzerinde unique index koyacağız.
    /// </summary>
    public class Enrollment : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; protected set; }

        public Guid StudentId { get; protected set; }
        public Guid CourseId { get; protected set; }

        public DateTime EnrollmentDate { get; protected set; }

        protected Enrollment() { }

        public Enrollment(
            Guid id,
            Guid studentId,
            Guid courseId,
            DateTime? enrollmentDate = null,
            Guid? tenantId = null
        ) : base(id)
        {
            if (studentId == Guid.Empty) throw new ArgumentException("StudentId is required.", nameof(studentId));
            if (courseId == Guid.Empty) throw new ArgumentException("CourseId is required.", nameof(courseId));

            StudentId = studentId;
            CourseId = courseId;
            EnrollmentDate = enrollmentDate ?? DateTime.UtcNow;
            TenantId = tenantId;
        }
    }
}
