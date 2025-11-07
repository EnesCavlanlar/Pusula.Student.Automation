using AutoMapper;

// Alias’lar
using StudentEntity = Pusula.Student.Automation.Students.Student;
using TeacherEntity = Pusula.Student.Automation.Teachers.Teacher;
using CourseEntity = Pusula.Student.Automation.Courses.Course;
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
using GradeEntity = Pusula.Student.Automation.Grades.Grade;
using AttendanceEntity = Pusula.Student.Automation.Attendances.Attendance;

// DTO namespaces
using Pusula.Student.Automation.Students;
using Pusula.Student.Automation.Teachers;
using Pusula.Student.Automation.Courses;
using Pusula.Student.Automation.Enrollments;
using Pusula.Student.Automation.Grades;
using Pusula.Student.Automation.Attendances;

namespace Pusula.Student.Automation
{
    public class AutomationApplicationAutoMapperProfile : Profile
    {
        public AutomationApplicationAutoMapperProfile()
        {
            // Student
            CreateMap<StudentEntity, StudentDto>();
            CreateMap<CreateUpdateStudentDto, StudentEntity>();

            // Teacher
            CreateMap<TeacherEntity, TeacherDto>();
            CreateMap<CreateUpdateTeacherDto, TeacherEntity>();

            // Course
            CreateMap<CourseEntity, CourseDto>();
            CreateMap<CreateUpdateCourseDto, CourseEntity>();

            // Enrollment
            CreateMap(typeof(EnrollmentEntity), typeof(EnrollmentDto));

            // Grade
            CreateMap<GradeEntity, GradeDto>();
            CreateMap<CreateUpdateGradeDto, GradeEntity>();

            // Attendance
            CreateMap<AttendanceEntity, AttendanceDto>();
            CreateMap<CreateUpdateAttendanceDto, AttendanceEntity>();
        }
    }
}
 