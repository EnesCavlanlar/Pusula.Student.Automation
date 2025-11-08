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
            // Students
            CreateMap<StudentEntity, StudentDto>();
            CreateMap<CreateUpdateStudentDto, StudentEntity>();

            // Teachers
            CreateMap<TeacherEntity, TeacherDto>();
            CreateMap<CreateUpdateTeacherDto, TeacherEntity>();

            // Courses
            CreateMap<CourseEntity, CourseDto>();
            CreateMap<CreateUpdateCourseDto, CourseEntity>();

            // Enrollments
            CreateMap<EnrollmentEntity, EnrollmentDto>();
            CreateMap<CreateEnrollmentDto, EnrollmentEntity>();

            // Grades  (Entity.GradeValue  <-> DTO.Score)
            CreateMap<GradeEntity, GradeDto>()
                .ForMember(d => d.Score, opt => opt.MapFrom(s => s.GradeValue));

            CreateMap<CreateUpdateGradeDto, GradeEntity>()
                .ForMember(d => d.GradeValue, opt => opt.MapFrom(s => s.Score));

            // Attendance
            CreateMap<AttendanceEntity, AttendanceDto>();
            CreateMap<CreateUpdateAttendanceDto, AttendanceEntity>();
        }
    }
}
