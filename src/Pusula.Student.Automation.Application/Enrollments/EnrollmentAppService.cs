using Pusula.Student.Automation.Courses;
using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.Teachers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

// alias’lar
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
using StudentEntity = Pusula.Student.Automation.Students.Student;
using Pusula.Student.Automation.Enrollments.Dtos;

namespace Pusula.Student.Automation.Enrollments
{
    public class EnrollmentAppService :
        CrudAppService<
            EnrollmentEntity,
            EnrollmentDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateEnrollmentDto>,
        IEnrollmentAppService
    {
        private readonly IRepository<Teacher, Guid> _teacherRepository;
        private readonly IRepository<Course, Guid> _courseRepository;
        private readonly IRepository<StudentEntity, Guid> _studentRepository;

        public EnrollmentAppService(
            IRepository<EnrollmentEntity, Guid> repository,
            IRepository<Teacher, Guid> teacherRepository,
            IRepository<Course, Guid> courseRepository,
            IRepository<StudentEntity, Guid> studentRepository
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;

            GetPolicyName = AutomationPermissions.Enrollments.Default;
            GetListPolicyName = AutomationPermissions.Enrollments.Default;
            CreatePolicyName = AutomationPermissions.Enrollments.Create;
            UpdatePolicyName = AutomationPermissions.Enrollments.Edit;
            DeletePolicyName = AutomationPermissions.Enrollments.Delete;
        }

        private bool IsTeacherUser => CurrentUser?.Roles?.Contains("Teacher") == true;
        private bool IsStudentUser => CurrentUser?.Roles?.Contains("Student") == true;

        private async Task<Guid> GetCurrentTeacherIdOrThrowAsync()
        {
            if (!IsTeacherUser)
                throw new AbpAuthorizationException("Only teacher users can call this helper.");

            var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Current user is not authenticated.");

            var teachers = await _teacherRepository.GetListAsync();

            Guid? GetLinkedUserId(object t)
            {
                var type = t.GetType();
                var prop =
                    type.GetProperty("UserId") ??
                    type.GetProperty("IdentityUserId") ??
                    type.GetProperty("AppUserId");
                return (Guid?)prop?.GetValue(t);
            }

            var teacher = teachers.FirstOrDefault(t => GetLinkedUserId(t) == userId)
                          ?? throw new AbpAuthorizationException("No Teacher record is linked to the current user.");

            return teacher.Id;
        }

        private async Task<Guid> GetCurrentStudentIdOrThrowAsync()
        {
            if (!IsStudentUser)
                throw new AbpAuthorizationException("Only student users can call this helper.");

            var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Current user is not authenticated.");

            var students = await _studentRepository.GetListAsync();

            Guid? GetLinkedUserId(object s)
            {
                var type = s.GetType();
                var prop =
                    type.GetProperty("UserId") ??
                    type.GetProperty("IdentityUserId") ??
                    type.GetProperty("AppUserId");
                return (Guid?)prop?.GetValue(s);
            }

            var student = students.FirstOrDefault(s => GetLinkedUserId(s) == userId)
                          ?? throw new AbpAuthorizationException("No Student record is linked to the current user.");

            return student.Id;
        }

        private async Task<IReadOnlyList<Guid>> GetCurrentTeacherCourseIdsAsync()
        {
            if (!IsTeacherUser) return Array.Empty<Guid>();
            var teacherId = await GetCurrentTeacherIdOrThrowAsync();
            var list = await _courseRepository.GetListAsync(c => c.TeacherId == teacherId);
            return list.Select(c => c.Id).ToList();
        }

        private async Task EnsureOwnerAsync(EnrollmentEntity entity)
        {
            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(entity.CourseId))
                    throw new AbpAuthorizationException("You are not allowed to access this enrollment.");
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                if (entity.StudentId != myStudentId)
                    throw new AbpAuthorizationException("You are not allowed to access other students' enrollments.");
            }
        }

        // READ
        public override async Task<EnrollmentDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);
            return await base.GetAsync(id);
        }

        public override async Task<PagedResultDto<EnrollmentDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                query = query.Where(e => myCourseIds.Contains(e.CourseId));
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                query = query.Where(e => e.StudentId == myStudentId);
            }

            query = query.OrderBy(e => e.EnrollmentDate);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var dtoItems = ObjectMapper.Map<List<EnrollmentEntity>, List<EnrollmentDto>>(page);
            return new PagedResultDto<EnrollmentDto>(total, dtoItems);
        }

        // CREATE
        public override async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input)
        {
            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(input.CourseId))
                    throw new AbpAuthorizationException("You can create enrollments only for your own courses.");
            }

            if (IsStudentUser)
                throw new AbpAuthorizationException("Students cannot create enrollments.");

            var entity = new EnrollmentEntity(
                GuidGenerator.Create(),
                input.StudentId,
                input.CourseId,
                input.EnrollmentDate ?? Clock.Now
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        // UPDATE
        public override async Task<EnrollmentDto> UpdateAsync(Guid id, CreateEnrollmentDto input)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(input.CourseId))
                    throw new AbpAuthorizationException("You can update enrollments only for your own courses.");
            }

            if (IsStudentUser)
                throw new AbpAuthorizationException("Students cannot update enrollments.");

            void SetProp<TVal>(object target, string name, TVal value)
            {
                var p = target.GetType().GetProperty(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var setter = p?.GetSetMethod(true);
                if (setter == null)
                    throw new AbpException($"Property '{name}' is not settable on {target.GetType().Name}.");
                setter.Invoke(target, new object?[] { value });
            }

            SetProp(entity, "StudentId", input.StudentId);
            SetProp(entity, "CourseId", input.CourseId);
            SetProp(entity, "EnrollmentDate", input.EnrollmentDate ?? entity.EnrollmentDate);

            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

        // DELETE
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);

            if (IsStudentUser)
                throw new AbpAuthorizationException("Students cannot delete enrollments.");

            await base.DeleteAsync(id);
        }

        // NEW
        public async Task<List<CourseStudentDto>> GetStudentsByCourseAsync(Guid courseId)
        {
            // rol kontrolü (öğretmense kendi dersini görsün)
            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(courseId))
                    throw new AbpAuthorizationException("You can list students only for your own courses.");
            }

            var enrollments = await Repository.GetListAsync(e => e.CourseId == courseId);

            var studentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();
            var students = await _studentRepository.GetListAsync(s => studentIds.Contains(s.Id));

            var result = (from e in enrollments
                          join s in students on e.StudentId equals s.Id
                          select new CourseStudentDto
                          {
                              Id = e.Id,
                              EnrollmentId = e.Id,
                              StudentId = s.Id,
                              StudentName = $"{s.FirstName} {s.LastName}",
                              StudentNo = s.StudentNo,
                              CourseId = courseId
                          })
                         .OrderBy(x => x.StudentName)
                         .ToList();

            return result;
        }
    }
}
