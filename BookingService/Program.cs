using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingService.Health;
using BookingService.Infrastructure;
using BookingService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProcessRepository, InMemoryProcessRepository>();
builder.Services.AddSingleton<BookingService.Services.BookingService>();

builder.Services.AddControllers();
builder.Services.AddLogging();

builder.Services.AddHealthChecks()
    .AddCheck<ReadinessHealthCheck>("readiness");

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();