using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Pusula.Student.Automation.HttpApi.Rabbit;

namespace Pusula.Student.Automation.HttpApi.Rabbit
{
    /// <summary>
    /// "student-automation.grades" kuyruğunu dinler.
    /// Gelen mesajı loglar ve RecentGradesStore'a ekler.
    /// </summary>
    public class RabbitMqGradeConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMqGradeConsumer> _logger;
        private readonly RecentGradesStore _store;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqGradeConsumer(
            ILogger<RabbitMqGradeConsumer> logger,
            RecentGradesStore store)
        {
            _logger = logger;
            _store = store;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // bağlantıyı burada açıyoruz
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // dinleyeceğimiz kuyruğu garantiye al
            _channel.QueueDeclare(
                queue: "student-automation.grades",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _logger.LogInformation("RabbitMQ grade consumer started.");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogWarning("RabbitMQ channel not initialized.");
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var doc = JsonDocument.Parse(message);

                    var evt = doc.RootElement.GetProperty("Event").GetString();
                    var enrollmentId = doc.RootElement.GetProperty("EnrollmentId").GetGuid();
                    var score = doc.RootElement.GetProperty("Score").GetInt32();

                    string? note = null;
                    if (doc.RootElement.TryGetProperty("Note", out var noteProp) && noteProp.ValueKind != JsonValueKind.Null)
                    {
                        note = noteProp.GetString();
                    }

                    _logger.LogInformation("📩 RabbitMQ event received: {Event} {EnrollmentId} {Score}",
                        evt, enrollmentId, score);

                    _store.Add(new RecentGradeItem(enrollmentId, score, note, DateTime.UtcNow));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ message could not be processed. Raw: {Message}", message);
                }
                finally
                {
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsume(
                queue: "student-automation.grades",
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
