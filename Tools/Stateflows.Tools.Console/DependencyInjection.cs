using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class DependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddConsole(this IStateflowsBuilder builder)
        {
            // builder.ServiceCollection.AddHostedService<DebuggerWatcher>();
            
            return builder;
        }
        
        private static bool apiMapped = false;

        [DebuggerHidden]
        public static IEndpointRouteBuilder UseStateflowsConsole(this IEndpointRouteBuilder builder)
        {
            if (!apiMapped)
            {
                builder.MapGet(
                    "/stateflows/console/manifest",
                    () => Results.Json(new
                    {
                        Enabled = true,
                        Url = "/stateflows/console/index.html",
                    })
                ).RequireCors(b => b.AllowAnyOrigin());

                if (builder is WebApplication app)
                {
                    app.UseStaticFiles(new StaticFileOptions 
                    {
                        FileProvider = new PhysicalFileProvider(
                            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Console")
                        ),
                        RequestPath = "/stateflows/console"
                    });   
                }

                apiMapped = true;
            }

            return builder;
        }
    }
}