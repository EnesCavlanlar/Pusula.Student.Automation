using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Pusula.Student.Automation.HttpApi.Rabbit
{
    // RabbitMQ'dan gelen son notları hafızada tutan basit store
    public class RecentGradesStore
    {
        private readonly ConcurrentQueue<RecentGradeItem> _items = new();
        private const int MaxItems = 50;

        public void Add(RecentGradeItem item)
        {
            _items.Enqueue(item);

            // 50'den fazlasını at
            while (_items.Count > MaxItems && _items.TryDequeue(out _)) { }
        }

        public IReadOnlyCollection<RecentGradeItem> GetAll()
        {
            return _items.ToArray();
        }
    }

    public record RecentGradeItem(Guid EnrollmentId, int Score, string? Note, DateTime CreatedAt);
}
