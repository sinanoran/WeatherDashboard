using WeatherDashboard.Server.Api.Endpoints;
using WeatherDashboard.Server.Application;
using WeatherDashboard.Server.Infrastructure;
using WeatherDashboard.Server.Infrastructure.ErrorHandling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructureServices();

IServiceCollection services = builder.Services;

services.AddProblemDetails();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddOpenApi();
services.AddApplicationServices();

WebApplication app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapApiEndpoints();
app.MapDefaultEndpoints();
app.UseFileServer();

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"));

app.Run();

public partial class Program;
