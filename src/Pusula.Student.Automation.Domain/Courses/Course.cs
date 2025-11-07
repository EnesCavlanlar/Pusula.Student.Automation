using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Courses
{
    public class Course : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; protected set; }

        public string Name { get; protected set; } = default!;
        public string Code { get; protected set; } = default!; // benzersiz
        public int? Credit { get; protected set; }

        // Relation: Course -> Teacher (required)
        public Guid TeacherId { get; protected set; }

        protected Course() { }

        public Course(
            Guid id,
            string name,
            string code,
            Guid teacherId,
            int? credit = null,
            Guid? tenantId = null
        ) : base(id)
        {
            SetName(name);
            SetCode(code);
            TeacherId = teacherId;
            Credit = credit;
            TenantId = tenantId;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));
            Name = name.Trim();
        }

        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code is required.", nameof(code));
            Code = code.Trim().ToUpperInvariant();
        }
    }
}
