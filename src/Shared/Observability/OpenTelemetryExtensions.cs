using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddHappyHeadlinesObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string serviceName)
    {
        var otlpEndpoint =
            configuration["OpenTelemetry:OtlpEndpoint"]
            ?? "http://otel-collector:4317";

        var builder = services.AddOpenTelemetry();

        builder.ConfigureResource(resource => resource
            .AddService(
                serviceName: serviceName,
                serviceVersion: "1.0.0",
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment.EnvironmentName
            }));

        builder.WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                });

            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            }
        });
        
        return services;
    }
}