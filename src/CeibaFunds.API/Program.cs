using CeibaFunds.Infrastructure.Configuration;
using CeibaFunds.Application.Handlers.Commands;
using FluentValidation;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ceibafunds-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SubscribeToFundHandler).Assembly);
});

builder.Services.AddAutoMapper(typeof(CeibaFunds.Application.Mappings.MappingProfile));

builder.Services.AddValidatorsFromAssembly(typeof(CeibaFunds.Application.Validators.SubscribeToFundCommandValidator).Assembly);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CeibaFunds API",
        Version = "v1",
        Description = "Investment Funds Management System",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "CeibaFunds Team",
            Email = "support@ceibafunds.com"
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CeibaFunds API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI as root
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

try
{
    Log.Information("Starting CeibaFunds API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
