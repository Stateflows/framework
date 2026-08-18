using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Tools.Dashboard;

namespace Stateflows
{
    public static class DashboardDependencyInjection
    {
        /// <summary>
        /// Registers Stateflows Dashboard services.
        /// Call this inside <c>AddStateflows(b => b.AddDashboard())</c>.
        /// </summary>
        [DebuggerHidden]
        public static IStateflowsBuilder AddDashboard(
            this IStateflowsBuilder builder,
            Action<DashboardOptions>? configure = null)
        {
            var options = new DashboardOptions();
            configure?.Invoke(options);
            builder.ServiceCollection.AddSingleton(options);

            return builder;
        }

        private static bool _dashboardMapped;

        /// <summary>
        /// Mounts the Stateflows Dashboard Angular SPA.
        /// Must be called after <c>app.UseStaticFiles()</c> and
        /// <c>app.MapStateflowsMinimalAPIsEndpoints()</c>.
        /// </summary>
        [DebuggerHidden]
        public static IEndpointRouteBuilder UseStateflowsDashboard(
            this IEndpointRouteBuilder builder,
            Action<DashboardOptions>? configure = null)
        {
            if (_dashboardMapped) return builder;
            _dashboardMapped = true;
            
            builder.MapStateflowsMinimalAPIsEndpoints(b => b
                .SetApiRoutePrefix("stateflows-dashboard")
            );

            // Resolve options — prefer DI-registered instance so that settings from
            // AddDashboard() are honoured; allow call-site overrides on top of that.
            var options = builder is IApplicationBuilder app
                ? app.ApplicationServices.GetService<DashboardOptions>() ?? new DashboardOptions()
                : new DashboardOptions();

            configure?.Invoke(options);

            var routePrefix = options.RoutePrefix.TrimEnd('/');

            // ── Manifest endpoint ─────────────────────────────────────────────────
            var manifestEndpoint = builder.MapGet(
                $"{routePrefix}/manifest",
                () => Results.Json(new
                {
                    Enabled = true,
                    Url = $"{routePrefix}/index.html",
                })
            );

            if (options.CorsPolicyName != null)
                manifestEndpoint.RequireCors(options.CorsPolicyName);
            else
                manifestEndpoint.RequireCors(b => b.AllowAnyOrigin());

            if (options.AuthorizationPolicyName != null)
                manifestEndpoint.RequireAuthorization(options.AuthorizationPolicyName);

            // ── Resolve dashboard directory next to the dashboard assembly ────────
            var dashboardDir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Dashboard"
            );
            var indexHtmlPath = Path.Combine(dashboardDir, "index.html");

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            var dashboardRootFullPath = Path.GetFullPath(dashboardDir);

            // ── SPA catch-all: only for Angular client-side routes ─────────────
            // Requests WITH a file extension (js, css, etc.) that weren't found by
            // the static-files middleware are left to return 404 naturally.
            // Only extensionless paths (Angular routes) get index.html.
            var fallbackEndpoint = builder.MapGet(
                $"{routePrefix}/{{**slug}}",
                async (string? slug) =>
                {
                    if (!Directory.Exists(dashboardDir))
                        return Results.NotFound();

                    var relativePath = (slug ?? string.Empty)
                        .Replace('\\', '/')
                        .TrimStart('/');

                    if (string.IsNullOrWhiteSpace(relativePath))
                        relativePath = "index.html";

                    var candidatePath = Path.GetFullPath(Path.Combine(dashboardRootFullPath, relativePath));
                    if (!candidatePath.StartsWith(dashboardRootFullPath, StringComparison.OrdinalIgnoreCase))
                        return Results.NotFound();

                    if (File.Exists(candidatePath))
                    {
                        if (!contentTypeProvider.TryGetContentType(candidatePath, out var contentType))
                            contentType = "application/octet-stream";

                        return Results.File(candidatePath, contentType);
                    }

                    if (Path.HasExtension(relativePath))
                        return Results.NotFound();

                    if (!File.Exists(indexHtmlPath))
                        return Results.NotFound();

                    var html = await File.ReadAllTextAsync(indexHtmlPath);
                    return Results.Content(html, "text/html");
                }
            );

            if (options.AuthorizationPolicyName != null)
                fallbackEndpoint.RequireAuthorization(options.AuthorizationPolicyName);

            return builder;
        }
    }
}

