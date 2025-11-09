using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Pusula.Student.Automation.Messaging
{
    /// <summary>
    /// Uygulamadan RabbitMQ'ya mesaj yollamak için basit publisher.
    /// </summary>
    public class RabbitMqEventPublisher : IRabbitMqEventPublisher, IDisposable
    {
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger)
        {
            _logger = logger;

            // docker-compose'da adı "psa-rabbitmq" ama
            // app container içinde çalışıyorsan "rabbitmq" servisine hostname olarak erişebilirsin.
            // sen local makineden çalıştığın için "localhost" kullanacağız.
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task PublishAsync(string queueName, string message)
        {
            // kuyruk yoksa oluşsun
            _channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body
            );

            _logger.LogInformation("RabbitMQ: {Queue} kuyruğuna mesaj gönderildi. İçerik: {Message}", queueName, message);

            return Task.CompletedTask;
        }

        public Task PublishGradeCreatedAsync(Guid enrollmentId, int score, string? note)
        {
            var evt = new
            {
                Event = "GradeCreated",
                EnrollmentId = enrollmentId,
                Score = score,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(evt);
            return PublishAsync("student-automation.grades", json);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
