using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.Teachers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
// Alias
using CourseEntity = Pusula.Student.Automation.Courses.Course;

namespace Pusula.Student.Automation.Courses
{
    public class CourseAppService :
        CrudAppService<
            CourseEntity,
            CourseDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateCourseDto>,
        ICourseAppService
    {
        private readonly IRepository<Teacher, Guid> _teacherRepository;

        public CourseAppService(
            IRepository<CourseEntity, Guid> repository,
            IRepository<Teacher, Guid> teacherRepository
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;

            GetPolicyName = AutomationPermissions.Courses.Default;
            GetListPolicyName = AutomationPermissions.Courses.Default;
            CreatePolicyName = AutomationPermissions.Courses.Create;
            UpdatePolicyName = AutomationPermissions.Courses.Edit;
            DeletePolicyName = AutomationPermissions.Courses.Delete;
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

            // EF bağımlılığına girmeden, tüm öğretmenleri çekip
            // UserId/IdentityUserId gibi muhtemel alan adlarını reflection ile kontrol edelim.
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

        private async Task EnsureOwnerAsync(CourseEntity entity)
        {
            if (!IsTeacherUser) return; // admin vb. serbest
            var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
            if (entity.TeacherId != currentTeacherId)
                throw new AbpAuthorizationException("You are not allowed to access this course.");
        }

        // ------------------------------
        // Read operations
        // ------------------------------
        public override async Task<CourseDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);
            return await base.GetAsync(id);
        }

        public override async Task<PagedResultDto<CourseDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                query = query.Where(c => c.TeacherId == currentTeacherId);
            }

            // Basit default sıralama (Dynamic.Core gerektirmez)
            query = query.OrderBy(c => c.Code);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var dtoItems = ObjectMapper.Map<List<CourseEntity>, List<CourseDto>>(page);
            return new PagedResultDto<CourseDto>(total, dtoItems);
        }

        // ------------------------------
        // Write operations
        // ------------------------------
        public override async Task<CourseDto> CreateAsync(CreateUpdateCourseDto input)
        {
            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                input.TeacherId = currentTeacherId; // kendi adına ders açar
            }

            return await base.CreateAsync(input);
        }

        public override async Task<CourseDto> UpdateAsync(Guid id, CreateUpdateCourseDto input)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);

            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                input.TeacherId = currentTeacherId; // başka öğretmene devri engelle
            }

            return await base.UpdateAsync(id, input);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);
            await base.DeleteAsync(id);
        }
    }
}
