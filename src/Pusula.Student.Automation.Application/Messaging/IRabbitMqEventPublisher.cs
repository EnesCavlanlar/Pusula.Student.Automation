using System;
using System.Threading.Tasks;

namespace Pusula.Student.Automation.Messaging
{
    public interface IRabbitMqEventPublisher
    {
        Task PublishAsync(string queueName, string message);

        // Projemize özel örnek event
        Task PublishGradeCreatedAsync(Guid enrollmentId, int score, string? note);
    }
}
