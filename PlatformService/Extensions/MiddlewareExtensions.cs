using PlatformService.Data;
using Scalar.AspNetCore;

namespace PlatformService.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        PrepDb.PrepPopulation(app, app.Environment.IsProduction());
        // app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}