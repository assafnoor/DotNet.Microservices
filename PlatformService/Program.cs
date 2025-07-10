using PlatformService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add application services
builder.Services.AddApplicationServices(builder.Configuration,
    builder.Environment);
Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");

var app = builder.Build();

app.UseApplicationMiddleware();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();