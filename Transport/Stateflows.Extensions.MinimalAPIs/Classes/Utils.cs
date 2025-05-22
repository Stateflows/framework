using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class Utils
{
    internal static IEnumerable<HateoasLink> ToHateoasLinks(this BehaviorInfo behaviorInfo, Dictionary<string, List<HateoasLink>> customHateoasLinks)
    {
        var behaviorTypeResource = behaviorInfo.Id.Type switch
        {
            BehaviorType.StateMachine => "stateMachines",
            BehaviorType.Activity => "activities",
            BehaviorType.Action => "actions",
            _ => throw new ArgumentOutOfRangeException()
        };

        var routePrefix = $"{DependencyInjection.ApiRoutePrefix}/{behaviorTypeResource}";
        
        var links = behaviorInfo.ExpectedEvents
            .Select(ev => ev.ToShortName().ToCamelCase())
            .Select(ev => new HateoasLink()
            {
                Rel = ev,
                Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}/{ev}",
                Method = "POST"
            })
            .ToList();

        links.Add(new HateoasLink()
        {
            Rel = "status",
            Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}/status",
            Method = "GET"
        });

        links.Add(new HateoasLink()
        {
            Rel = "reset",
            Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}",
            Method = "DELETE"
        });

        if (behaviorInfo.BehaviorStatus == BehaviorStatus.Initialized)
        {
            links.Add(new HateoasLink()
            {
                Rel = "notifications",
                Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}/notifications",
                Method = "GET"
            });

            links.Add(new HateoasLink()
            {
                Rel = "finalize",
                Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}/finalize",
                Method = "POST"
            });

            if (customHateoasLinks.TryGetValue("", out var globalLinks))
            {
                links.AddRange(globalLinks.ToInstanceLinks(routePrefix, behaviorInfo));
            }

            if (behaviorInfo is StateMachineInfo stateMachineInfo)
            {
                links.AddRange(
                    stateMachineInfo.CurrentStates.GetAllNodes().SelectMany(node =>
                        customHateoasLinks.TryGetValue(node.Value, out var links)
                            ? links.ToInstanceLinks(routePrefix, behaviorInfo)
                            : []
                    )
                );
            }

            if (behaviorInfo is ActivityInfo activityInfo)
            {
                links.AddRange(
                    activityInfo.ActiveNodes.GetAllNodes().SelectMany(node =>
                        customHateoasLinks.TryGetValue(node.Value, out var links)
                            ? links.ToInstanceLinks(routePrefix, behaviorInfo)
                            : []
                    )
                );
            }
        }

        return links;
    }

    internal static IDictionary<string, object> ToMetadata(this BehaviorInfo behaviorInfo)
    {
        var metadata = new Dictionary<string, object>();
        metadata.Add(nameof(behaviorInfo.Id).ToCamelCase(), behaviorInfo.Id);
        metadata.Add(nameof(behaviorInfo.BehaviorStatus).ToCamelCase(), behaviorInfo.BehaviorStatus);
        metadata.Add(nameof(behaviorInfo.ExpectedEvents).ToCamelCase(), behaviorInfo.ExpectedEvents);
        metadata.Add(nameof(behaviorInfo.BehaviorStatusText).ToCamelCase(), behaviorInfo.BehaviorStatusText);
        switch (behaviorInfo)
        {
            case StateMachineInfo stateMachineInfo:
                metadata.Add(nameof(stateMachineInfo.CurrentStates).ToCamelCase(), stateMachineInfo.CurrentStates);
                break;
            case ActivityInfo activityInfo:
                metadata.Add(nameof(activityInfo.ActiveNodes).ToCamelCase(), activityInfo.ActiveNodes);
                break;
        }

        return metadata;
    }

    private static IEnumerable<HateoasLink> ToInstanceLinks(this IEnumerable<HateoasLink> links, string routePrefix, BehaviorInfo behaviorInfo)
        => links.Select(link => link with { Href = $"{routePrefix}/{behaviorInfo.Id.Name}/{behaviorInfo.Id.Instance}{link.Href}" });
    
    public static IResult ToResult(this SendResult result, IEnumerable<EventHolder> notifications, BehaviorInfo behaviorInfo, Dictionary<string, List<HateoasLink>> customHateoasLinks)
    {
        var response = new ResponseBody(result, notifications, behaviorInfo.ToHateoasLinks(customHateoasLinks), behaviorInfo.ToMetadata());
        var jsonResult = StateflowsJsonConverter.SerializeObject(response, true);
        return jsonResult.ToResult(result.Status);
    }
    
    public static IResult ToResult<TResponse>(this RequestResult<TResponse> result, IEnumerable<EventHolder> notifications, BehaviorInfo behaviorInfo, Dictionary<string, List<HateoasLink>> customHateoasLinks)
    {
        var jsonResult = StateflowsJsonConverter.SerializeObject(new ResponseBody<TResponse>(result, notifications, behaviorInfo.ToHateoasLinks(customHateoasLinks), behaviorInfo.ToMetadata()), true);
        return jsonResult.ToResult(result.Status);
    }

    private static IResult ToResult(this string jsonResult, EventStatus eventStatus)
        => eventStatus switch
        {
            EventStatus.Initialized => Results.Content(jsonResult, "application/json", statusCode: 201), // 201 created
            EventStatus.NotInitialized => Results.Content(jsonResult, "application/json", statusCode: 409),
            EventStatus.Undelivered => Results.Content(jsonResult, "application/json", statusCode: 404),
            EventStatus.Rejected => Results.Content(jsonResult, "application/json", statusCode: 409),
            EventStatus.Invalid => Results.Content(jsonResult, "application/json", statusCode: 400),
            EventStatus.Deferred => Results.Content(jsonResult, "application/json", statusCode: 202), // 202 accepted
            EventStatus.Consumed => Results.Content(jsonResult, "application/json", statusCode: 200),
            EventStatus.NotConsumed => Results.Content(jsonResult, "application/json", statusCode: 409),
            EventStatus.Omitted => Results.Content(jsonResult, "application/json", statusCode: 200),
            EventStatus.Failed => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
            EventStatus.Forwarded => Results.Content(jsonResult, "application/json", statusCode: 202), // 202 accepted
            _ => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
        };

    internal static string GetEventName<TEvent>()
        => Event<TEvent>.Name.ToShortName().ToCamelCase();

    internal static bool IsEventEmpty(Type eventType) =>
        eventType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length == 0 &&
        eventType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length == 0 &&
        eventType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).All(m => m.IsSpecialName) &&
        eventType.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length == 0;

    internal static async Task<(bool Success, IResult? Result)> AuthorizeEventAsync(Type eventType, IServiceProvider serviceProvider, HttpContext context)
    {
        var authorizeAttributes = eventType.GetCustomAttributes<AuthorizeAttribute>().ToArray();
        if (authorizeAttributes.Any())
        {
            try
            {
                var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
                            
                foreach (var authorizeAttribute in authorizeAttributes)
                {
                    if (!await AuthorizationHelper.IsAuthorizedAsync(authorizeAttribute, context.User,
                            authorizationService))
                    {
                        return (false, Results.Unauthorized());
                    }
                }
            }
            catch (Exception e)
            {
                return (false, Results.Problem(e.Message));
            }
        }

        return (true, null);
    }
}