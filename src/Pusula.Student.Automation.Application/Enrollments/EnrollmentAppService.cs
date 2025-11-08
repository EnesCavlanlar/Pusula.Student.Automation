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
// Entity alias: isim/namespace çakışmasını önler
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;

namespace Pusula.Student.Automation.Enrollments
{
    public class EnrollmentAppService :
        CrudAppService<
            EnrollmentEntity,               // Entity
            EnrollmentDto,                  // Return DTO
            Guid,                           // PK
            PagedAndSortedResultRequestDto, // Paging input
            CreateEnrollmentDto>,           // Create/Update DTO
        IEnrollmentAppService
    {
        private readonly IRepository<Teacher, Guid> _teacherRepository;
        private readonly IRepository<Course, Guid> _courseRepository;

        public EnrollmentAppService(
            IRepository<EnrollmentEntity, Guid> repository,
            IRepository<Teacher, Guid> teacherRepository,
            IRepository<Course, Guid> courseRepository
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;

            GetPolicyName = AutomationPermissions.Enrollments.Default;
            GetListPolicyName = AutomationPermissions.Enrollments.Default;
            CreatePolicyName = AutomationPermissions.Enrollments.Create;
            UpdatePolicyName = AutomationPermissions.Enrollments.Edit;
            DeletePolicyName = AutomationPermissions.Enrollments.Delete;
        }

        // ------------------------------
        // Helpers
        // ------------------------------
        private bool IsTeacherUser => CurrentUser?.Roles?.Contains("Teacher") == true;

        private async Task<Guid> GetCurrentTeacherIdOrThrowAsync()
        {
            if (!IsTeacherUser)
                throw new AbpAuthorizationException("Only teacher users can call this helper.");

            var userId = CurrentUser.Id;
            if (userId == null)
                throw new AbpAuthorizationException("Current user is not authenticated.");

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

            var teacher = teachers.FirstOrDefault(t => GetLinkedUserId(t) == userId);

            if (teacher == null)
                throw new AbpAuthorizationException("No Teacher record is linked to the current user.");

            return teacher.Id;
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
            if (!IsTeacherUser) return; // admin serbest
            var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
            if (!myCourseIds.Contains(entity.CourseId))
                throw new AbpAuthorizationException("You are not allowed to access this enrollment.");
        }

        // ------------------------------
        // READ
        // ------------------------------
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

            query = query.OrderBy(e => e.EnrollmentDate);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var dtoItems = ObjectMapper.Map<List<EnrollmentEntity>, List<EnrollmentDto>>(page);
            return new PagedResultDto<EnrollmentDto>(total, dtoItems);
        }

        // ------------------------------
        // WRITE
        // ------------------------------
        public override async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto input)
        {
            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(input.CourseId))
                    throw new AbpAuthorizationException("You can create enrollments only for your own courses.");
            }

            var entity = new EnrollmentEntity(
                GuidGenerator.Create(),
                input.StudentId,
                input.CourseId,
                input.EnrollmentDate ?? Clock.Now
            );

            await Repository.InsertAsync(entity, autoSave: true);
            return await MapToGetOutputDtoAsync(entity);
        }

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

            // Entity set'leri private/protected olduğu için reflection ile set edelim
            void SetProp<TVal>(object target, string name, TVal value)
            {
                var p = target.GetType().GetProperty(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                var setter = p?.GetSetMethod(nonPublic: true);
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

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);
            await base.DeleteAsync(id);
        }
    }
}
