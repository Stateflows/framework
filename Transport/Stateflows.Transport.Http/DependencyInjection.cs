using System.Net.Mime;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Transport.Classes;

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

                        var behaviorId = new BehaviorId(input.BehaviorId.Type, input.BehaviorId.Name, input.BehaviorId.Instance);
                        if (locator.TryLocateBehavior(behaviorId, out var behavior))
                        {
                            EventHolder? response = null;
                            
                            var responses = new Dictionary<object, EventHolder>();
                            ResponseHolder.SetResponses(responses);

                            var result = await behavior.SendAsync(input.Event.BoxedPayload, input.Event.Headers);
                            try
                            {
                                response = result.Status == EventStatus.Consumed
                                    ? input.Event.GetResponseHolder()
                                    : null;
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                            return Results.Text(
                                StateflowsJsonConverter.SerializePolymorphicObject(
                                    new StateflowsResponse()
                                    {
                                        EventStatus = result.Status,
                                        Validation = result.Validation,
                                        Response = response,
                                        Notifications = result.Status != EventStatus.Rejected
                                            ? hub.Notifications.GetPendingNotifications(behaviorId, input.Watches)
                                            : Array.Empty<EventHolder>(),
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
    }
}
