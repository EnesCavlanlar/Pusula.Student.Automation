using System;
using System.Collections.Generic;

namespace Pusula.Student.Automation.Caching
{
    // Belirli bir derse (course) kayıtlı öğrenci listesini cache'lemek için
    public class CourseStudentsCacheItem
    {
        public Guid CourseId { get; set; }

        // o derse kayıtlı öğrenci (veya enrollment) id'leri
        public List<Guid> StudentIds { get; set; } = new();
    }
}
