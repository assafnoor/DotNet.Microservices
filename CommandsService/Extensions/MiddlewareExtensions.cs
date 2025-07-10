using Scalar.AspNetCore;

namespace CommandsService.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}