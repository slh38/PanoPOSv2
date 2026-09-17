using Serilog;

namespace PanoPos.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseWebApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseSerilogRequestLogging();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseMiddleware<PanoPos.WebApi.Authentication.OturumMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
