using MyMicroserviceApp.SharedContracts;
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

builder.Host.UseSerilog();

builder.Services.AddCustomOpenTelemetry(configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyMicroserviceApp_"; // Tiền tố cho key cache
});

// Add services to the container.

// Đăng ký gRPC client cho Product Service
builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5001"); // Địa chỉ của Product gRPC Service
});

// Đăng ký gRPC client cho User Service
builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5002"); // Địa chỉ của User gRPC Service
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();