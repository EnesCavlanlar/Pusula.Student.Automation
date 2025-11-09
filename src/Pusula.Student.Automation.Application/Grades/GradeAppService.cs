using Pusula.Student.Automation.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;              // cache
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;                                      // cache
using Volo.Abp.Domain.Repositories;
using CourseEntity = Pusula.Student.Automation.Courses.Course;
using EnrollmentEntity = Pusula.Student.Automation.Enrollments.Enrollment;
// ----- Entity alias'ları -----
using GradeEntity = Pusula.Student.Automation.Grades.Grade;
using StudentEntity = Pusula.Student.Automation.Students.Student;
using TeacherEntity = Pusula.Student.Automation.Teachers.Teacher;
using Pusula.Student.Automation.Grades.Dtos;
using Pusula.Student.Automation.Caching;                     // cache item'lar
using Pusula.Student.Automation.Messaging;                   // ⬅ RabbitMQ publisher

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

        private readonly IDistributedCache<StudentGradesCacheItem> _studentGradesCache;
        private readonly IDistributedCache<CourseStudentsCacheItem> _courseStudentsCache;
        private readonly IRabbitMqEventPublisher _rabbit;          // ⬅ yeni

        private const int StudentGradesCacheSeconds = 60;

        public GradeAppService(
            IRepository<GradeEntity, Guid> repository,
            IRepository<TeacherEntity, Guid> teacherRepository,
            IRepository<StudentEntity, Guid> studentRepository,
            IRepository<CourseEntity, Guid> courseRepository,
            IRepository<EnrollmentEntity, Guid> enrollmentRepository,
            IDistributedCache<StudentGradesCacheItem> studentGradesCache,
            IDistributedCache<CourseStudentsCacheItem> courseStudentsCache,
            IRabbitMqEventPublisher rabbitMqEventPublisher          // ⬅ yeni
        ) : base(repository)
        {
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _studentGradesCache = studentGradesCache;
            _courseStudentsCache = courseStudentsCache;
            _rabbit = rabbitMqEventPublisher;                       // ⬅ yeni

            GetPolicyName = AutomationPermissions.Grades.Default;
            GetListPolicyName = AutomationPermissions.Grades.Default;
            CreatePolicyName = AutomationPermissions.Grades.Create;
            UpdatePolicyName = AutomationPermissions.Grades.Edit;
            DeletePolicyName = AutomationPermissions.Grades.Delete;
        }

        // cache key helpers
        private static string BuildStudentGradesCacheKey(Guid studentId)
            => $"student:{studentId}:grades";

        private static string BuildCourseStudentsCacheKey(Guid courseId)
            => $"course:{courseId}:students";

        // role helpers
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
                throw new AbpAuthorizationException("Students are not allowed to modify grades.");
            }
        }

        // READ
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

            query = query.OrderBy(x => x.Enrollment.CourseId).ThenBy(x => x.Grade.Id);

            var total = await AsyncExecuter.CountAsync(query);
            var page = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var gradeList = page.Select(x => x.Grade).ToList();
            var dtoItems = ObjectMapper.Map<List<GradeEntity>, List<GradeDto>>(gradeList);
            return new PagedResultDto<GradeDto>(total, dtoItems);
        }

        // Öğrencinin notlarını cache'li getir
        public async Task<List<GradeDto>> GetByStudentAsync(Guid studentId)
        {
            var cacheKey = BuildStudentGradesCacheKey(studentId);

            var cached = await _studentGradesCache.GetAsync(cacheKey);
            if (cached != null && cached.GradeIds is { Count: > 0 })
            {
                var gq = await Repository.GetQueryableAsync();
                var eq = await _enrollmentRepository.GetQueryableAsync();

                var fromDb = from g in gq
                             join e in eq on g.EnrollmentId equals e.Id
                             where e.StudentId == studentId && cached.GradeIds.Contains(g.Id)
                             select g;

                var list = await AsyncExecuter.ToListAsync(fromDb);
                return ObjectMapper.Map<List<GradeEntity>, List<GradeDto>>(list);
            }

            // cache yoksa
            var gq2 = await Repository.GetQueryableAsync();
            var eq2 = await _enrollmentRepository.GetQueryableAsync();

            var query2 =
                from g in gq2
                join e in eq2 on g.EnrollmentId equals e.Id
                where e.StudentId == studentId
                select g;

            var allGrades = await AsyncExecuter.ToListAsync(query2);
            var dtos = ObjectMapper.Map<List<GradeEntity>, List<GradeDto>>(allGrades);

            var toCache = new StudentGradesCacheItem
            {
                StudentId = studentId,
                GradeIds = dtos.Select(d => d.Id).ToList()
            };

            await _studentGradesCache.SetAsync(
                cacheKey,
                toCache,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(StudentGradesCacheSeconds)
                });

            return dtos;
        }

        // CREATE
        public override async Task<GradeDto> CreateAsync(CreateUpdateGradeDto input)
        {
            await EnsureEnrollmentAccessibleAsync(input.EnrollmentId);

            var enrollment = await _enrollmentRepository.GetAsync(input.EnrollmentId);

            var dto = await base.CreateAsync(input);

            // cache temizle
            if (enrollment.StudentId != Guid.Empty)
                await _studentGradesCache.RemoveAsync(BuildStudentGradesCacheKey(enrollment.StudentId));

            if (enrollment.CourseId != Guid.Empty)
                await _courseStudentsCache.RemoveAsync(BuildCourseStudentsCacheKey(enrollment.CourseId));

            // RabbitMQ'ya da gönder
            await _rabbit.PublishGradeCreatedAsync(enrollment.Id, input.Score, input.Note);

            return dto;
        }

        // UPDATE
        public override async Task<GradeDto> UpdateAsync(Guid id, CreateUpdateGradeDto input)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureGradeAccessibleAsync(entity);
            await EnsureEnrollmentAccessibleAsync(input.EnrollmentId);

            var oldEnrollment = await _enrollmentRepository.GetAsync(entity.EnrollmentId);
            var newEnrollment = await _enrollmentRepository.GetAsync(input.EnrollmentId);

            var dto = await base.UpdateAsync(id, input);

            if (oldEnrollment.StudentId != Guid.Empty)
                await _studentGradesCache.RemoveAsync(BuildStudentGradesCacheKey(oldEnrollment.StudentId));
            if (newEnrollment.StudentId != Guid.Empty && newEnrollment.StudentId != oldEnrollment.StudentId)
                await _studentGradesCache.RemoveAsync(BuildStudentGradesCacheKey(newEnrollment.StudentId));

            if (oldEnrollment.CourseId != Guid.Empty)
                await _courseStudentsCache.RemoveAsync(BuildCourseStudentsCacheKey(oldEnrollment.CourseId));
            if (newEnrollment.CourseId != Guid.Empty && newEnrollment.CourseId != oldEnrollment.CourseId)
                await _courseStudentsCache.RemoveAsync(BuildCourseStudentsCacheKey(newEnrollment.CourseId));

            // event
            await _rabbit.PublishGradeCreatedAsync(newEnrollment.Id, input.Score, input.Note);

            return dto;
        }

        // DELETE
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await EnsureGradeAccessibleAsync(entity);

            var enrollment = await _enrollmentRepository.GetAsync(entity.EnrollmentId);

            await base.DeleteAsync(id);

            if (enrollment.StudentId != Guid.Empty)
                await _studentGradesCache.RemoveAsync(BuildStudentGradesCacheKey(enrollment.StudentId));

            if (enrollment.CourseId != Guid.Empty)
                await _courseStudentsCache.RemoveAsync(BuildCourseStudentsCacheKey(enrollment.CourseId));
        }

        // UPSERT
        public async Task<GradeDto> UpsertAsync(UpsertGradeDto input)
        {
            await EnsureEnrollmentAccessibleAsync(input.EnrollmentId);

            var enrollment = await _enrollmentRepository.GetAsync(input.EnrollmentId);

            var existing = await Repository.FirstOrDefaultAsync(g => g.EnrollmentId == input.EnrollmentId);
            if (existing != null)
            {
                await EnsureGradeAccessibleAsync(existing);
                await Repository.DeleteAsync(existing);
            }

            var grade = new GradeEntity(
                GuidGenerator.Create(),
                input.EnrollmentId,
                input.Score,
                input.Note
            );

            await Repository.InsertAsync(grade, autoSave: true);

            if (enrollment.StudentId != Guid.Empty)
                await _studentGradesCache.RemoveAsync(BuildStudentGradesCacheKey(enrollment.StudentId));
            if (enrollment.CourseId != Guid.Empty)
                await _courseStudentsCache.RemoveAsync(BuildCourseStudentsCacheKey(enrollment.CourseId));

            // ⬅ RabbitMQ
            await _rabbit.PublishGradeCreatedAsync(enrollment.Id, input.Score, input.Note);

            return ObjectMapper.Map<GradeEntity, GradeDto>(grade);
        }

        // ders bazlı liste
        public async Task<List<GradeDto>> GetListByCourseAsync(Guid courseId)
        {
            var gq = await Repository.GetQueryableAsync();
            var eq = await _enrollmentRepository.GetQueryableAsync();

            var query =
                from g in gq
                join e in eq on g.EnrollmentId equals e.Id
                where e.CourseId == courseId
                select new { Grade = g, Enrollment = e };

            if (IsTeacherUser)
            {
                var myCourseIds = await GetCurrentTeacherCourseIdsAsync();
                if (!myCourseIds.Contains(courseId))
                    throw new AbpAuthorizationException("You are not allowed to see grades of this course.");
            }

            if (IsStudentUser)
            {
                var myStudentId = await GetCurrentStudentIdOrThrowAsync();
                query = query.Where(x => x.Enrollment.StudentId == myStudentId);
            }

            var list = await AsyncExecuter.ToListAsync(query);
            var gradeList = list.Select(x => x.Grade).ToList();
            return ObjectMapper.Map<List<GradeEntity>, List<GradeDto>>(gradeList);
        }
    }
}
