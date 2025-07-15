using MyMicroserviceApp.ProductGrpcService.Data;
using MyMicroserviceApp.ProductGrpcService.Services;
using MyMicroserviceApp.SharedContracts.Jagger;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    //.Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Uri"]!))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLower().Replace(".", "-")}-{builder.Environment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 2
    })
    .CreateLogger();

builder.Services.AddCustomOpenTelemetry(configuration);

builder.Host.UseSerilog();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

builder.Services.AddSingleton<ProductDbService>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseSerilogRequestLogging();

// --- Thêm logic khởi tạo dữ liệu vào đây ---
using (var scope = app.Services.CreateScope())
{
    var productDbService = scope.ServiceProvider.GetRequiredService<ProductDbService>();
    await productDbService.SeedDataAsync();
}
// --- Kết thúc logic khởi tạo dữ liệu ---

app.MapGrpcService<ProductGrpcServiceImpl>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086502");

app.Run();