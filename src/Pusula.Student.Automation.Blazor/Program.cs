using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch; // ⬅ eklendi
using Pusula.Student.Automation.HttpApi.Rabbit; // ⬅ consumer ve store için

namespace Pusula.Student.Automation.Blazor
{
    public class Program
    {
        public async static Task<int> Main(string[] args)
        {
            // bootstrap logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "psa-blazor-logs-bootstrap-{0:yyyy.MM.dd}"
                })
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host.");
                var builder = WebApplication.CreateBuilder(args);

                // ⬅ RabbitMQ consumer için DI
                builder.Services.AddSingleton<RecentGradesStore>();
                builder.Services.AddHostedService<RabbitMqGradeConsumer>();

                builder.Host
                    .AddAppSettingsSecretsJson()
                    .UseAutofac()
                    .UseSerilog((context, services, loggerConfiguration) =>
                    {
                        loggerConfiguration
#if DEBUG
                            .MinimumLevel.Debug()
#else
                            .MinimumLevel.Information()
#endif
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                            .Enrich.FromLogContext()
                            .WriteTo.Async(c => c.File("Logs/logs.txt"))
                            .WriteTo.Async(c => c.Console())
                            // ABP Studio varsa kalsın
                            .WriteTo.Async(c => c.AbpStudio(services))
                            // ⬇ Elasticsearch'e de yaz
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                            {
                                AutoRegisterTemplate = true,
                                IndexFormat = "psa-blazor-logs-{0:yyyy.MM.dd}"
                            });
                    });

                await builder.AddApplicationAsync<AutomationBlazorModule>();
                var app = builder.Build();
                await app.InitializeApplicationAsync();
                await app.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                if (ex is HostAbortedException)
                {
                    throw;
                }

                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
