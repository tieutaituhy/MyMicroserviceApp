using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MyMicroserviceApp.SharedContracts.Jagger
{
    /// <summary>
    /// Provides extension methods for configuring OpenTelemetry.
    /// </summary>
    public static class OpenTelemetryExtensions
    {
        /// <summary>
        /// Adds and configures OpenTelemetry tracing with common instrumentations and an OTLP exporter.
        /// Service name, version, and OTLP endpoint are read from IConfiguration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance for reading settings.</param>
        /// <param name="defaultServiceName">A default service name to use if not found in configuration. Defaults to "UnknownService".</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddCustomOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration,
            string defaultServiceName = "UnknownService")
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var NameConfig = configuration.GetValue<string>("AppOptions:Name") ?? defaultServiceName;
            var name = string.IsNullOrEmpty(environment) ? NameConfig : $"{NameConfig}-{environment?.ToLower()}";
            // Read service name from configuration (e.g., AppOptions:Name)
            // You can adjust the configuration key as needed.
            var serviceName = name ?? defaultServiceName;

            // Read service version from configuration (e.g., AppOptions:Version)
            // You can adjust the configuration key as needed.
            var serviceVersion = configuration.GetValue<string>("AppOptions:Version") ?? "1.0.0";

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            // Optionally configure ASP.NET Core instrumentation
                            options.RecordException = true;
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            // Optionally configure HTTP client instrumentation
                            options.RecordException = true;
                        })
                        .AddGrpcClientInstrumentation(options =>
                        {
                            // If additional configuration is needed, use the available properties like 'SuppressDownstreamInstrumentation' or 'EnrichWithHttpRequestMessage'.
                            options.SuppressDownstreamInstrumentation = false;
                        })
                        //.AddConsoleExporter() // For local debugging
                        .AddOtlpExporter(otlpOptions =>
                        {
                            // Read OTLP endpoint from configuration (e.g., Otlp:Endpoint)
                            // Defaults to http://localhost:4317 if not specified.
                            // When running in Docker, this should be set via environment variables
                            // in docker-compose.yml to point to the Jaeger service (e.g., http://jaeger:4317)
                            var otlpEndpoint = configuration.GetValue<string>("Otlp:Endpoint") ?? "http://localhost:4317";
                            otlpOptions.Endpoint = new Uri(otlpEndpoint);

                            // The default protocol is gRPC. You can change it if needed:
                            // otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                        });

                    // If you are using CAP and want to instrument it, you can add:
                    // .AddCapInstrumentation(); 
                    // Make sure to install the OpenTelemetry.Instrumentation.Cap package
                });

            return services;
        }
    }
}
