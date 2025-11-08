using System;
using Volo.Abp.Domain.Entities;

namespace Pusula.Student.Automation.Courses;

public class Course : Entity<Guid>
{
    // ABP default ctor
    protected Course() { }

    public Course(Guid id, string code, string name, int credit, Guid teacherId)
        : base(id)
    {
        Code = code;
        Name = name;
        Credit = credit;
        TeacherId = teacherId;
        Status = CourseStatus.Planned; // default
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public int Credit { get; private set; }
    public Guid TeacherId { get; private set; }

    // NEW: durum
    public CourseStatus Status { get; private set; } = CourseStatus.Planned;

    // setters
    public void SetCode(string code) => Code = code;
    public void SetName(string name) => Name = name;
    public void SetCredit(int credit) => Credit = credit;
    public void SetTeacher(Guid teacherId) => TeacherId = teacherId;

    public void SetStatus(CourseStatus status) => Status = status;
}
