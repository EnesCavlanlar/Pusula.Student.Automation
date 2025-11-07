using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pusula.Student.Automation.Students
{
    // AuditedAggregateRoot: CreatedTime/CreatorId/LastModification vs. otomatik gelir.
    // IMultiTenant opsiyonel; çoklu tenant düşünüyorsak ekleyebiliriz.
    public class Student : AuditedAggregateRoot<Guid>, IMultiTenant
    {
        // IMultiTenant
        public Guid? TenantId { get; protected set; }

        // Business fields
        public string StudentNo { get; protected set; } = default!;
        public string FirstName { get; protected set; } = default!;
        public string LastName { get; protected set; } = default!;
        public string Email { get; protected set; } = default!;
        public DateTime? BirthDate { get; protected set; }

        // ctor for EF
        protected Student() { }

        public Student(
            Guid id,
            string studentNo,
            string firstName,
            string lastName,
            string email,
            DateTime? birthDate = null,
            Guid? tenantId = null
        ) : base(id)
        {
            SetStudentNo(studentNo);
            SetFirstName(firstName);
            SetLastName(lastName);
            SetEmail(email);
            BirthDate = birthDate;
            TenantId = tenantId;
        }

        public void SetStudentNo(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                throw new ArgumentException("Student number is required.", nameof(studentNo));

            StudentNo = studentNo.Trim();
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
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            // basit bir kontrol
            if (!email.Contains("@"))
                throw new ArgumentException("Email is invalid.", nameof(email));

            Email = email.Trim();
        }

        public override string ToString() => $"{StudentNo} - {FirstName} {LastName}";
    }
}
