using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Stateflows.Manager
{
    public static class DependencyInjection
    {
        public static WebApplication UseStateflowsManager(this WebApplication app)
        {
            app.MapStateflowsTransportHub();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "StateflowsManager")),
                RequestPath = "/StateflowsManager"
            });

            return app;
        }
    }
}
