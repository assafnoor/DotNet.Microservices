using CommandsService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseApplicationMiddleware();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();