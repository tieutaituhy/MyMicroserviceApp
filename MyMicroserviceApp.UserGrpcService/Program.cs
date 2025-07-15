using Microsoft.EntityFrameworkCore;
using MyMicroserviceApp.SharedContracts.Jagger;
using MyMicroserviceApp.UserGrpcService.Data;
using MyMicroserviceApp.UserGrpcService.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Reflection; // Để đăng ký gRPC service

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

builder.Host.UseSerilog();

builder.Services.AddCustomOpenTelemetry(configuration);

// Cấu hình để gRPC chạy trên HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    // Thiết lập endpoint cho gRPC
    options.ListenLocalhost(5002, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

// Add services to the container.
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddGrpc(); // Thêm gRPC services

var app = builder.Build();

app.UseSerilogRequestLogging();

// Migrate the database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dbContext.Database.Migrate(); // Áp dụng migrations
}

// Configure the HTTP request pipeline.
app.MapGrpcService<UserGrpcServiceImpl>(); // Ánh xạ gRPC service

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086502");

app.Run();