using System.Net.Mime;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
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
            if (apiMapped)
            {
                return builder;
            }
            
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
                    {
                        return Results.Unauthorized();
                    }

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
                                        ? await hub.GetNotificationsAsync(
                                            behaviorId,
                                            notification =>
                                                input.Watches?.Any(watch =>
                                                    watch.NotificationName == notification.Name &&
                                                    (
                                                        (
                                                            watch.LastNotificationCheck != null &&
                                                            notification.SentAt >= watch.LastNotificationCheck
                                                        ) ||
                                                        (
                                                            watch.MilisecondsSinceLastNotificationCheck != null &&
                                                            notification.SentAt.AddSeconds(notification.TimeToLive) >= DateTime.Now.AddMilliseconds(- (int)watch.MilisecondsSinceLastNotificationCheck)
                                                        )
                                                    )
                                                ) ?? false
                                        )
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

            return builder;
        }

        private static bool AuthorizeUser(HttpContext context, EventHolder stateflowsEventHolder)
        {
            var authAttribute = stateflowsEventHolder.PayloadType.GetCustomAttribute<AuthorizeAttribute>();
            if (authAttribute != null)
            {
                var policy = authAttribute.Policy;
                var user = context.User;

                return
                    (user != null) &&
                    (
                        policy == null
                            ? user.Identity!.IsAuthenticated
                            : user.Claims.Any(c => c.Value.Equals(policy))
                    );
            }

            return true;
        }
    }
}