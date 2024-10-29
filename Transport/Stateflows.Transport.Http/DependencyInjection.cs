using System.Net.Mime;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Transport.Classes;
using Microsoft.AspNetCore.Authorization;

namespace Stateflows.Transport.Http
{
    public static class DependencyInjection
    {
        private static bool apiMapped = false;

        [DebuggerHidden]
        public static IEndpointRouteBuilder MapStateflowsHttpTransport(this IEndpointRouteBuilder builder, Action<RouteHandlerBuilder>? routeHandlerBuilderAction = null)
        {
            if (!apiMapped)
            {
                var routeBuilder = builder.MapPost(
                    "/stateflows/send",
                    async (
                        HttpContext context,
                        IBehaviorLocator locator,
                        INotificationsHub hub
                    ) =>
                    {

                        var responseTime = DateTime.Now;
                        using var reader = new StreamReader(context.Request.Body);
                        var body = await reader.ReadToEndAsync();
                        var input = StateflowsJsonConverter.DeserializeObject<StateflowsRequest>(body);
                        //temporary authorization solution
                        if (!AuthorizeUser(context, input.Event))
                            return Results.Unauthorized();
                        var behaviorId = new BehaviorId(input.BehaviorId.Type, input.BehaviorId.Name, input.BehaviorId.Instance);
                        if (locator.TryLocateBehavior(behaviorId, out var behavior))
                        {
                            var result = await behavior.SendAsync(input.Event);
                            return Results.Text(
                                StateflowsJsonConverter.SerializePolymorphicObject(
                                    new StateflowsResponse()
                                    {
                                        EventStatus = result.Status,
                                        Validation = result.Validation,
                                        Response = result.Status == EventStatus.Consumed
                                            ? result.Event.GetResponse()
                                            : null,
                                        Notifications = result.Status != EventStatus.Rejected
                                            ? hub.Notifications.GetPendingNotifications(behaviorId, input.Watches)
                                            : Array.Empty<Notification>(),
                                        ResponseTime = responseTime,
                                    },
                                    true
                                ),
                                MediaTypeNames.Application.Json
                            );
                        }
                        else
                        {
                            return Results.NotFound();
                        }
                    }
                );

                routeHandlerBuilderAction?.Invoke(routeBuilder);

                routeBuilder = builder.MapGet(
                    "/stateflows/availableClasses",
                    (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses)
                );

                routeHandlerBuilderAction?.Invoke(routeBuilder);

                apiMapped = true;
            }

            return builder;
        }

        private static bool AuthorizeUser(HttpContext context, Event stateflowsEvent)
        {
            AuthorizeAttribute? authAttribute = (AuthorizeAttribute?)Attribute.GetCustomAttribute(stateflowsEvent.GetType(),
                                                                                                  typeof(AuthorizeAttribute));
            if (authAttribute != null)
            {
                var policy = authAttribute.Policy;
                var user = context.User;
                if (policy == null && user != null && user.Identity!.IsAuthenticated)
                    return true;
                if (user != null && user.Claims.Any(c => c.Value.Equals(policy)))
                {
                    return true;
                }
                return false;
            }
            return true;

        }
    }
}
