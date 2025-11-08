using Microsoft.EntityFrameworkCore;

// --- Aliaslar: isim çakýþmalarýný önlemek için ---
using StudentEntity = Pusula.Student.Automation.Students.Student;
using TeacherEntity = Pusula.Student.Automation.Teachers.Teacher;
using CourseEntity = Pusula.Student.Automation.Courses.Course;
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
using GradeEntity = Pusula.Student.Automation.Grades.Grade;
using AttendanceEntity = Pusula.Student.Automation.Attendances.Attendance;

using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Pusula.Student.Automation.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AutomationDbContext :
    AbpDbContext<AutomationDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    // ---- Our domain entities ----
    public DbSet<StudentEntity> Students { get; set; }
    public DbSet<TeacherEntity> Teachers { get; set; }
    public DbSet<CourseEntity> Courses { get; set; }
    public DbSet<EnrollmentEntity> Enrollments { get; set; }
    public DbSet<GradeEntity> Grades { get; set; }      // <-- eklendi
    public DbSet<AttendanceEntity> Attendances { get; set; }      // <-- eklendi

    #region Entities from the modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public AutomationDbContext(DbContextOptions<AutomationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        // ---- Student ----
        builder.Entity<StudentEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Students", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(64);
            b.Property(x => x.FirstName).IsRequired().HasMaxLength(128);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);

            b.HasIndex(x => x.StudentNo).IsUnique();
            b.HasIndex(x => x.Email).IsUnique();
        });

        // ---- Teacher ----
        builder.Entity<TeacherEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Teachers", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FirstName).IsRequired().HasMaxLength(128);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.Department).HasMaxLength(128);

            b.HasIndex(x => x.Email).IsUnique();
        });

        // ---- Course ----
        builder.Entity<CourseEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Courses", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Code).IsRequired().HasMaxLength(32);
            b.Property(x => x.Credit);

            // Unique Code per tenant
            // Unique Code (single-tenant / no TenantId)
            b.HasIndex(x => x.Code).IsUnique();

            // Relation: Course -> Teacher (required)
            b.HasOne<TeacherEntity>()
             .WithMany()
             .HasForeignKey(x => x.TeacherId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Enrollment (Student-Course junction) ----
        builder.Entity<EnrollmentEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Enrollments", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.EnrollmentDate).IsRequired();

            // Ayný öðrencinin ayný dersi bir kez almasý için unique index
            b.HasIndex(nameof(EnrollmentEntity.TenantId), nameof(EnrollmentEntity.StudentId), nameof(EnrollmentEntity.CourseId))
             .IsUnique();

            // FK: Enrollment -> Student
            b.HasOne<StudentEntity>()
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            // FK: Enrollment -> Course
            b.HasOne<CourseEntity>()
             .WithMany()
             .HasForeignKey(x => x.CourseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Grade ----
        builder.Entity<GradeEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Grades", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.GradeValue).IsRequired();      // 0-100
            b.Property(x => x.Note).HasMaxLength(512);

            // FK: Grade -> Enrollment
            b.HasOne<EnrollmentEntity>()
             .WithMany()
             .HasForeignKey(x => x.EnrollmentId)
             .OnDelete(DeleteBehavior.Cascade);

            // (Opsiyonel) ayný enrollment için sadece tek grade istiyorsan:
            // b.HasIndex(nameof(GradeEntity.TenantId), nameof(GradeEntity.EnrollmentId)).IsUnique();
        });

        // ---- Attendance ----
        builder.Entity<AttendanceEntity>(b =>
        {
            b.ToTable(AutomationConsts.DbTablePrefix + "Attendances", AutomationConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Date).IsRequired();
            b.Property(x => x.IsPresent).IsRequired();

            // Ayný Enrollment + Date için tek kayýt
            b.HasIndex(nameof(AttendanceEntity.TenantId), nameof(AttendanceEntity.EnrollmentId), nameof(AttendanceEntity.Date))
             .IsUnique();

            // FK: Attendance -> Enrollment
            b.HasOne<EnrollmentEntity>()
             .WithMany()
             .HasForeignKey(x => x.EnrollmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
