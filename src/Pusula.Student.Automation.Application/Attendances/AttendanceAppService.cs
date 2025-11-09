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

// alias'lar
using AttendanceEntity = Pusula.Student.Automation.Attendances.Attendance;
using CourseEntity = Pusula.Student.Automation.Courses.Course;
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
using StudentEntity = Pusula.Student.Automation.Students.Student;
using TeacherEntity = Pusula.Student.Automation.Teachers.Teacher;
using Pusula.Student.Automation.Attendances.Dtos;

namespace Pusula.Student.Automation.Attendances
{
    public class AttendanceAppService :
        CrudAppService<
            AttendanceEntity,
            AttendanceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAttendanceDto>,
        IAttendanceAppService
    {
        private readonly IRepository<TeacherEntity, Guid> _teacherRepository;
        private readonly IRepository<StudentEntity, Guid> _studentRepository;
        private readonly IRepository<CourseEntity, Guid> _courseRepository;
        private readonly IRepository<EnrollmentEntity, Guid> _enrollmentRepository;

        public AttendanceAppService(
            IRepository<AttendanceEntity, Guid> repository,
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

            GetPolicyName = AutomationPermissions.Attendances.Default;
            GetListPolicyName = AutomationPermissions.Attendances.Default;
            CreatePolicyName = AutomationPermissions.Attendances.Create;
            UpdatePolicyName = AutomationPermissions.Attendances.Edit;
            DeletePolicyName = AutomationPermissions.Attendances.Delete;
        }

        // ---------------- helpers ----------------
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

        private async Task EnsureAttendanceAccessibleAsync(AttendanceEntity attendance)
        {
            var enrollment = await _enrollmentRepository.GetAsync(attendance.EnrollmentId);

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(enrollment.CourseId))
                    throw new AbpAuthorizationException("You are not allowed to access attendances of other teachers' courses.");
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                if (enrollment.StudentId != myStudentId)
                    throw new AbpAuthorizationException("You are not allowed to access attendances of other students.");
            }
        }

        private async Task EnsureEnrollmentAccessibleForWriteAsync(Guid enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetAsync(enrollmentId);

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(enrollment.CourseId))
                    throw new AbpAuthorizationException("You can only manage attendance for your own courses.");
            }

            if (IsStudentUser)
                throw new AbpAuthorizationException("Students are not allowed to modify attendance.");
        }

        // ---------------- READ ----------------
        public override async Task<AttendanceDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureAttendanceAccessibleAsync(entity);
            return await base.GetAsync(id);
        }

        public override async Task<PagedResultDto<AttendanceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var aq = await Repository.GetQueryableAsync();
            var eq = await _enrollmentRepository.GetQueryableAsync();

            var query =
                from a in aq
                join e in eq on a.EnrollmentId equals e.Id
                select new { Attendance = a, Enrollment = e };

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

            query = query.OrderBy(x => x.Attendance.Date).ThenBy(x => x.Enrollment.StudentId);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var list = page.Select(x => x.Attendance).ToList();
            var dto = ObjectMapper.Map<List<AttendanceEntity>, List<AttendanceDto>>(list);
            return new PagedResultDto<AttendanceDto>(total, dto);
        }

        // ---------------- WRITE ----------------
        public override async Task<AttendanceDto> CreateAsync(CreateUpdateAttendanceDto input)
        {
            await EnsureEnrollmentAccessibleForWriteAsync(input.EnrollmentId);

            var entity = new AttendanceEntity(
                GuidGenerator.Create(),
                input.EnrollmentId,
                input.Date.Date,
                input.IsPresent
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        public override async Task<AttendanceDto> UpdateAsync(Guid id, CreateUpdateAttendanceDto input)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureAttendanceAccessibleAsync(entity);
            await EnsureEnrollmentAccessibleForWriteAsync(input.EnrollmentId);

            entity.SetDate(input.Date.Date);
            entity.SetPresence(input.IsPresent);

            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureAttendanceAccessibleAsync(entity);
            await base.DeleteAsync(id);
        }

        // ------------- NEW: Upsert -------------
        public async Task<AttendanceDto> UpsertAsync(UpsertAttendanceDto input)
        {
            // yetki
            await EnsureEnrollmentAccessibleForWriteAsync(input.EnrollmentId);

            // aynı enrollment + aynı gün var mı?
            var existing = await Repository.FirstOrDefaultAsync(a =>
                a.EnrollmentId == input.EnrollmentId &&
                a.Date == input.Date.Date
            );

            if (existing == null)
            {
                var entity = new AttendanceEntity(
                    GuidGenerator.Create(),
                    input.EnrollmentId,
                    input.Date.Date,
                    input.IsPresent
                );

                await Repository.InsertAsync(entity, autoSave: true);
                return ObjectMapper.Map<AttendanceEntity, AttendanceDto>(entity);
            }
            else
            {
                await EnsureAttendanceAccessibleAsync(existing);

                existing.SetDate(input.Date.Date);
                existing.SetPresence(input.IsPresent);

                await Repository.UpdateAsync(existing, autoSave: true);
                return ObjectMapper.Map<AttendanceEntity, AttendanceDto>(existing);
            }
        }

        // ------------- NEW: GetListByCourseAndDateAsync -------------
        public async Task<List<AttendanceDto>> GetListByCourseAndDateAsync(Guid courseId, DateTime date)
        {
            var aq = await Repository.GetQueryableAsync();
            var eq = await _enrollmentRepository.GetQueryableAsync();

            var query =
                from a in aq
                join e in eq on a.EnrollmentId equals e.Id
                where e.CourseId == courseId && a.Date == date.Date
                select new { Attendance = a, Enrollment = e };

            // teacher güvenlik
            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(courseId))
                    throw new AbpAuthorizationException("You are not allowed to see attendance of this course.");
            }

            // student güvenlik
            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                query = query.Where(x => x.Enrollment.StudentId == myStudentId);
            }

            var list = await AsyncExecuter.ToListAsync(query);
            var attendanceList = list.Select(x => x.Attendance).ToList();
            return ObjectMapper.Map<List<AttendanceEntity>, List<AttendanceDto>>(attendanceList);
        }
    }
}
