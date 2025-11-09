using System;
using System.Collections.Generic;

namespace Pusula.Student.Automation.Caching
{
    // Öğretmenin ders listesini Redis'te tutmak için basit cache item
    public class TeacherCoursesCacheItem
    {
        public Guid TeacherId { get; set; }

        // sadece ders ID'leri
        public List<Guid> CourseIds { get; set; } = new();
    }
}
