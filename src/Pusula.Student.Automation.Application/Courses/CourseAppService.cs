using Pusula.Student.Automation.Permissions;
using Pusula.Student.Automation.Teachers;
using Pusula.Student.Automation.Caching;           // cache item
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
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
        private readonly IDistributedCache<TeacherCoursesCacheItem> _teacherCoursesCache;
        private const int TeacherCoursesCacheSeconds = 60;

        public CourseAppService(
            IRepository<CourseEntity, Guid> repository,
            IRepository<Teacher, Guid> teacherRepository,
            IDistributedCache<TeacherCoursesCacheItem> teacherCoursesCache
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;
            _teacherCoursesCache = teacherCoursesCache;

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

        private static string BuildTeacherCoursesCacheKey(Guid teacherId)
        {
            return $"teacher:{teacherId}:courses";
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

            var created = await base.CreateAsync(input);

            // öğretmen ders açtıysa, kendi cache'i silinsin
            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                await _teacherCoursesCache.RemoveAsync(BuildTeacherCoursesCacheKey(currentTeacherId));
            }

            return created;
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

            var updated = await base.UpdateAsync(id, input);

            // ders güncellendiyse ilgili öğretmenin cache'ini sil
            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                await _teacherCoursesCache.RemoveAsync(BuildTeacherCoursesCacheKey(currentTeacherId));
            }

            return updated;
        }

        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureOwnerAsync(entity);

            await base.DeleteAsync(id);

            // ders silindiyse ilgili öğretmenin cache'i de silinsin
            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                await _teacherCoursesCache.RemoveAsync(BuildTeacherCoursesCacheKey(currentTeacherId));
            }
        }

        // ------------------------------
        // NEW: öğretmenin kendi dersleri (Redis cache'li)
        // ------------------------------
        public async Task<List<CourseDto>> GetMyCoursesAsync()
        {
            if (IsTeacherUser)
            {
                var currentTeacherId = await GetCurrentTeacherIdOrThrowAsync();
                var cacheKey = BuildTeacherCoursesCacheKey(currentTeacherId);

                // 1) önce redis'ten dene
                var cached = await _teacherCoursesCache.GetAsync(cacheKey);
                if (cached != null && cached.CourseIds != null && cached.CourseIds.Count > 0)
                {
                    var ids = cached.CourseIds;

                    var queryable = await Repository.GetQueryableAsync();
                    var filtered = queryable.Where(c => ids.Contains(c.Id));

                    var list = await AsyncExecuter.ToListAsync(filtered);
                    return ObjectMapper.Map<List<CourseEntity>, List<CourseDto>>(list);
                }

                // 2) cache yoksa db'den al
                var dbList = await Repository.GetListAsync(c => c.TeacherId == currentTeacherId);
                var dtoList = ObjectMapper.Map<List<CourseEntity>, List<CourseDto>>(dbList);

                // 3) id'leri cache'e koy
                var toCache = new TeacherCoursesCacheItem
                {
                    TeacherId = currentTeacherId,
                    CourseIds = dtoList.Select(d => d.Id).ToList()
                };

                await _teacherCoursesCache.SetAsync(
                    cacheKey,
                    toCache,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TeacherCoursesCacheSeconds)
                    }
                );

                return dtoList;
            }

            // admin / diğer roller
            var all = await Repository.GetListAsync();
            return ObjectMapper.Map<List<CourseEntity>, List<CourseDto>>(all);
        }
    }
}
