using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using Pusula.Student.Automation.Courses;
using Pusula.Student.Automation.Enrollments;

namespace Pusula.Student.Automation.Data;

/// <summary>
/// Demo amaçlı tohum veriler. AppService/DTO üzerinden çalışır; domain entity setter korumalarına takılmaz.
/// Idempotent: aynı verileri tekrar oluşturmaz.
/// </summary>
public class AutomationDemoAppServiceSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IStudentAppService _students;
    private readonly ITeacherAppService _teachers;
    private readonly ICourseAppService _courses;
    private readonly IEnrollmentAppService _enrollments;

    public AutomationDemoAppServiceSeedContributor(
        IStudentAppService students,
        ITeacherAppService teachers,
        ICourseAppService courses,
        IEnrollmentAppService enrollments)
    {
        _students = students;
        _teachers = teachers;
        _courses = courses;
        _enrollments = enrollments;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // --- Students ---
        var sAyseId = await EnsureStudentAsync("Ayşe", "Yılmaz", "S1001", "ayse@example.com");
        var sMehmetId = await EnsureStudentAsync("Mehmet", "Demir", "S1002", "mehmet@example.com");
        var sElifId = await EnsureStudentAsync("Elif", "Kaya", "S1003", "elif@example.com");

        // --- Teachers ---
        var tAhmetId = await EnsureTeacherAsync("Ahmet", "Hoca", "ahmet.hoca@example.com", "CS");
        var tZeynepId = await EnsureTeacherAsync("Zeynep", "Hoca", "zeynep.hoca@example.com", "Math");

        // --- Courses ---
        var cCs101Id = await EnsureCourseAsync("CS101", "Intro to CS", 3, tAhmetId);
        var cMath201Id = await EnsureCourseAsync("MATH201", "Calculus", 4, tZeynepId);

        // --- Enrollments (öğrenci-ders tekil kombinasyonu) ---
        await EnsureEnrollmentAsync(sAyseId, cCs101Id);
        await EnsureEnrollmentAsync(sMehmetId, cCs101Id);
        await EnsureEnrollmentAsync(sElifId, cMath201Id);
    }

    // ---------- helpers ----------

    private async Task<Guid> EnsureStudentAsync(string first, string last, string studentNo, string email)
    {
        var all = await _students.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 5000 });

        var hit = all.Items.FirstOrDefault(s =>
            (s.StudentNo ?? "").Equals(studentNo, StringComparison.OrdinalIgnoreCase) ||
            (s.Email ?? "").Equals(email, StringComparison.OrdinalIgnoreCase));

        if (hit != null)
            return hit.Id;

        var created = await _students.CreateAsync(new CreateUpdateStudentDto
        {
            FirstName = first,
            LastName = last,
            StudentNo = studentNo,
            Email = email
        });

        return created.Id;
    }

    private async Task<Guid> EnsureTeacherAsync(string first, string last, string email, string? department)
    {
        var all = await _teachers.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 5000 });

        var hit = all.Items.FirstOrDefault(t =>
            (t.Email ?? "").Equals(email, StringComparison.OrdinalIgnoreCase));

        if (hit != null)
            return hit.Id;

        var created = await _teachers.CreateAsync(new CreateUpdateTeacherDto
        {
            FirstName = first,
            LastName = last,
            Email = email,
            Department = department
        });

        return created.Id;
    }

    // NOTE: teacherId artık non-nullable Guid (hata düzeltildi)
    private async Task<Guid> EnsureCourseAsync(string code, string name, int credit, Guid teacherId)
    {
        var all = await _courses.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 5000 });

        var hit = all.Items.FirstOrDefault(c =>
            (c.Code ?? "").Equals(code, StringComparison.OrdinalIgnoreCase));

        if (hit != null)
            return hit.Id;

        var created = await _courses.CreateAsync(new CreateUpdateCourseDto
        {
            Code = code,
            Name = name,
            Credit = credit,
            TeacherId = teacherId
        });

        return created.Id;
    }

    private async Task EnsureEnrollmentAsync(Guid studentId, Guid courseId)
    {
        var all = await _enrollments.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 5000 });

        var exists = all.Items.Any(e => e.StudentId == studentId && e.CourseId == courseId);
        if (exists) return;

        await _enrollments.CreateAsync(new CreateEnrollmentDto
        {
            StudentId = studentId,
            CourseId = courseId
        });
    }
}
