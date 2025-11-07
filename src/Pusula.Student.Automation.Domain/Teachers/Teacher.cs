using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Teachers
{
    public class Teacher : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; protected set; }

        public string FirstName { get; protected set; } = default!;
        public string LastName { get; protected set; } = default!;
        public string Email { get; protected set; } = default!;
        public string? Department { get; protected set; }

        protected Teacher() { }

        public Teacher(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string? department = null,
            Guid? tenantId = null
        ) : base(id)
        {
            SetFirstName(firstName);
            SetLastName(lastName);
            SetEmail(email);
            Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
            TenantId = tenantId;
        }

        public void SetFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));
            FirstName = firstName.Trim();
        }

        public void SetLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));
            LastName = lastName.Trim();
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Email is invalid.", nameof(email));
            Email = email.Trim();
        }

        public override string ToString() => $"{FirstName} {LastName}";
    }
}
