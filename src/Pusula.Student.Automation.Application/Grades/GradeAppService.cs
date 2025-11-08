using Pusula.Student.Automation.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using CourseEntity = Pusula.Student.Automation.Courses.Course;
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
// ----- Entity alias'ları (namespace/isim çakışmasını önler) -----
using GradeEntity = Pusula.Student.Automation.Grades.Grade;
using StudentEntity = Pusula.Student.Automation.Students.Student;
using TeacherEntity = Pusula.Student.Automation.Teachers.Teacher;

namespace Pusula.Student.Automation.Grades
{
    public class GradeAppService :
        CrudAppService<
            GradeEntity,
            GradeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateGradeDto>,
        IGradeAppService
    {
        private readonly IRepository<TeacherEntity, Guid> _teacherRepository;
        private readonly IRepository<StudentEntity, Guid> _studentRepository;
        private readonly IRepository<CourseEntity, Guid> _courseRepository;
        private readonly IRepository<EnrollmentEntity, Guid> _enrollmentRepository;

        public GradeAppService(
            IRepository<GradeEntity, Guid> repository,
            IRepository<TeacherEntity, Guid> teacherRepository,
            IRepository<StudentEntity, Guid> studentRepository,
            IRepository<CourseEntity, Guid> courseRepository,
            IRepository<EnrollmentEntity, Guid> enrollmentRepository
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;

            GetPolicyName = AutomationPermissions.Grades.Default;
            GetListPolicyName = AutomationPermissions.Grades.Default;
            CreatePolicyName = AutomationPermissions.Grades.Create;
            UpdatePolicyName = AutomationPermissions.Grades.Edit;
            DeletePolicyName = AutomationPermissions.Grades.Delete;
        }

        // ------------------------------
        // Role/ownership helpers
        // ------------------------------
        private bool IsTeacherUser => CurrentUser?.Roles?.Contains("Teacher") == true;
        private bool IsStudentUser => CurrentUser?.Roles?.Contains("Student") == true;

        private Guid? TryGetLinkedUserId(object obj, params string[] propNames)
        {
            var t = obj.GetType();
            foreach (var name in propNames)
            {
                var p = t.GetProperty(name);
                if (p != null) return (Guid?)p.GetValue(obj);
            }
            return null;
        }

        private async Task<Guid> GetCurrentTeacherIdOrThrowAsync()
        {
            if (!IsTeacherUser)
                throw new AbpAuthorizationException("Only teacher users can call this helper.");

            var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Current user is not authenticated.");

            var teachers = await _teacherRepository.GetListAsync();
            var teacher = teachers.FirstOrDefault(t => TryGetLinkedUserId(t, "UserId", "IdentityUserId", "AppUserId") == userId);
            if (teacher == null)
                throw new AbpAuthorizationException("No Teacher record is linked to the current user.");

            return teacher.Id;
        }

        private async Task<Guid> GetCurrentStudentIdOrThrowAsync()
        {
            if (!IsStudentUser)
                throw new AbpAuthorizationException("Only student users can call this helper.");

            var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Current user is not authenticated.");

            var students = await _studentRepository.GetListAsync();
            var student = students.FirstOrDefault(s => TryGetLinkedUserId(s, "UserId", "IdentityUserId", "AppUserId") == userId);
            if (student == null)
                throw new AbpAuthorizationException("No Student record is linked to the current user.");

            return student.Id;
        }

        private async Task<IReadOnlyList<Guid>> GetCurrentTeacherCourseIdsAsync()
        {
            if (!IsTeacherUser) return Array.Empty<Guid>();
            var teacherId = await GetCurrentTeacherIdOrThrowAsync();
            var list = await _courseRepository.GetListAsync(c => c.TeacherId == teacherId);
            return list.Select(c => c.Id).ToList();
        }

        private async Task EnsureGradeAccessibleAsync(GradeEntity grade)
        {
            // Grade -> Enrollment -> (CourseId, StudentId)
            var enrollment = await _enrollmentRepository.GetAsync(grade.EnrollmentId);

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(enrollment.CourseId))
                    throw new AbpAuthorizationException("You are not allowed to access grades of other teachers' courses.");
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                if (enrollment.StudentId != myStudentId)
                    throw new AbpAuthorizationException("You are not allowed to access grades of other students.");
            }
        }

        private async Task EnsureEnrollmentAccessibleAsync(Guid enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetAsync(enrollmentId);

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(enrollment.CourseId))
                    throw new AbpAuthorizationException("You can only manage grades for your own courses.");
            }

            if (IsStudentUser)
            {
                // Öğrenciler yazma işlemi yapamaz; yine de savunmacı kontrol
                throw new AbpAuthorizationException("Students are not allowed to modify grades.");
            }
        }

        // ------------------------------
        // READ
        // ------------------------------
        public override async Task<GradeDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureGradeAccessibleAsync(entity);
            return await base.GetAsync(id);
        }

        public override async Task<PagedResultDto<GradeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var gq = await Repository.GetQueryableAsync();
            var eq = await _enrollmentRepository.GetQueryableAsync();

            var query =
                from g in gq
                join e in eq on g.EnrollmentId equals e.Id
                select new { Grade = g, Enrollment = e };

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                query = query.Where(x => myCourseIds.Contains(x.Enrollment.CourseId));
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                query = query.Where(x => x.Enrollment.StudentId == myStudentId);
            }

            // Basit default sıralama
            query = query.OrderBy(x => x.Enrollment.CourseId).ThenBy(x => x.Grade.Id);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var gradeList = page.Select(x => x.Grade).ToList();
            var dtoItems = ObjectMapper.Map<List<GradeEntity>, List<GradeDto>>(gradeList);
            return new PagedResultDto<GradeDto>(total, dtoItems);
        }

        // ------------------------------
        // WRITE
        // ------------------------------
        public override async Task<GradeDto> CreateAsync(CreateUpdateGradeDto input)
        {
            await EnsureEnrollmentAccessibleAsync(input.EnrollmentId);
            return await base.CreateAsync(input);
        }

        public override async Task<GradeDto> UpdateAsync(Guid id, CreateUpdateGradeDto input)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureGradeAccessibleAsync(entity);                 // mevcut kayda erişim
            await EnsureEnrollmentAccessibleAsync(input.EnrollmentId); // hedefe yetki
            return await base.UpdateAsync(id, input);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureGradeAccessibleAsync(entity);
            await base.DeleteAsync(id);
        }
    }
}
