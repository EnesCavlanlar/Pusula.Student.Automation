using System;
using System.Collections.Generic;

namespace Pusula.Student.Automation.Caching
{
    // Bir öğrencinin not listesini cache'lemek için
    public class StudentGradesCacheItem
    {
        public Guid StudentId { get; set; }

        // bu öğrencinin sahip olduğu grade id'leri (veya isterseniz dto'ya çeviririz)
        public List<Guid> GradeIds { get; set; } = new();
    }
}
