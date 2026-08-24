using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class Utils
{
    internal static string ToResource(this string behaviorType)
        => behaviorType switch
        {
            BehaviorType.StateMachine => "stateMachines",
            BehaviorType.Activity => "activities",
            BehaviorType.Action => "actions",
            _ => throw new ArgumentOutOfRangeException()
        };
    
    internal static IEnumerable<HateoasLink> ToHateoasLinks(this BehaviorInfo behaviorInfo, string method, string path, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> hateoasLinks)
    {
        var links = new List<HateoasLink>();

        if (behaviorInfo is null)
        {
            Debug.WriteLine($"~~~> {method} {path} - ToHateoasLinks(): BehaviorInfo is null, returning empty links.");
            
            return links; 
        }
        
        if (hateoasLinks is null)
        {
            Debug.WriteLine($"~~~> {method} {path} - ToHateoasLinks(): HateoasLinks dictionary is null, returning empty links.");
            
            return links;
        }
        
        links.AddRange(
            behaviorInfo.ExpectedEvents.SelectMany(expectedEvent =>
                hateoasLinks.TryGetValue($"{behaviorInfo.Id.Name}:{(behaviorInfo.Id.Instance == string.Empty ? "default" : "standard")}:event:{expectedEvent.ToShortName().ToCamelCase()}", out var eventLinks)
                    ? eventLinks.ToInstanceLinks(DependencyInjection.ApiRoutePrefix, behaviorInfo)
                    : []
            )
        );
        
        if (hateoasLinks.TryGetValue(behaviorInfo.Id.Name, out var globalLinks))
        {
            links.AddRange(globalLinks.ToInstanceLinks(DependencyInjection.ApiRoutePrefix, behaviorInfo));
        }
        
        if (behaviorInfo is StateMachineInfo { CurrentStates: not null } stateMachineInfo)
        {
            links.AddRange(
                stateMachineInfo.CurrentStates.GetAllNodes().SelectMany(node =>
                    hateoasLinks.TryGetValue($"{behaviorInfo.Id.Name}:{(behaviorInfo.Id.Instance == string.Empty ? "default" : "standard")}:node:{node.Value}", out var stateLinks)
                        ? stateLinks.ToInstanceLinks(DependencyInjection.ApiRoutePrefix, behaviorInfo)
                        : []
                )
            );
        }
        
        if (behaviorInfo is ActivityInfo { ActiveNodes: not null } activityInfo)
        {
            links.AddRange(
                activityInfo.ActiveNodes.GetAllNodes().SelectMany(node =>
                    hateoasLinks.TryGetValue($"{behaviorInfo.Id.Name}:{(behaviorInfo.Id.Instance == string.Empty ? "default" : "standard")}:node:{node.Value}", out var nodeLinks)
                        ? nodeLinks.ToInstanceLinks(DependencyInjection.ApiRoutePrefix, behaviorInfo)
                        : []
                )
            );
        }

        return links;
    }

    internal static IDictionary<string, object> ToMetadata(this BehaviorInfo behaviorInfo)
    {
        var metadata = new Dictionary<string, object>
        {
            { nameof(behaviorInfo.Id).ToCamelCase(), behaviorInfo.Id },
            { nameof(behaviorInfo.BehaviorStatus).ToCamelCase(), behaviorInfo.BehaviorStatus },
            { nameof(behaviorInfo.BehaviorStatusText).ToCamelCase(), behaviorInfo.BehaviorStatusText },
            { nameof(behaviorInfo.ExpectedEvents).ToCamelCase(), behaviorInfo.ExpectedEvents }
        };
        
        switch (behaviorInfo)
        {
            case StateMachineInfo stateMachineInfo:
                if (stateMachineInfo.CurrentStates != null)
                {
                    metadata.Add(nameof(stateMachineInfo.CurrentStates).ToCamelCase(), stateMachineInfo.CurrentStates);
                }

                break;
            case ActivityInfo activityInfo:
                if (activityInfo.ActiveNodes != null)
                {
                    metadata.Add(nameof(activityInfo.ActiveNodes).ToCamelCase(), activityInfo.ActiveNodes);
                }

                break;
        }

        foreach (var metadataKeyValue in behaviorInfo.Metadata)
        {
            metadata.TryAdd(metadataKeyValue.Key, metadataKeyValue.Value);
        }

        return metadata;
    }

    private static IEnumerable<HateoasLink> ToInstanceLinks(this IEnumerable<(HateoasLink, BehaviorStatus[])> links, string routePrefix, BehaviorInfo behaviorInfo)
        => links
            .Where(link => link.Item2.Contains(behaviorInfo.BehaviorStatus))
            .Select(link => link.Item1 with
            {
                Href = $"/{routePrefix}{link.Item1.Href.Replace("{instance}", behaviorInfo.Id.Instance)}"
            });
    
    public static IResult ToResult<TResponse>(this RequestResult<TResponse> result, string method, string path, IEnumerable<EventHolder> notifications, BehaviorInfo behaviorInfo, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
    {
        var response = new ResponseBody<TResponse>(result, notifications, behaviorInfo.ToHateoasLinks(method, path, customHateoasLinks), behaviorInfo.ToMetadata());
        var jsonResult = StateflowsJsonConverter.SerializeObject(response, true);
        return jsonResult.ToResult(result.Status);
    }
    
    public static IResult ToResult(this SendResult result, string method, string path, IEnumerable<EventHolder> notifications, BehaviorInfo behaviorInfo, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
    {
        var response = new ResponseBody(result, notifications, behaviorInfo.ToHateoasLinks(method, path, customHateoasLinks), behaviorInfo.ToMetadata());
        var jsonResult = StateflowsJsonConverter.SerializeObject(response, true);
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
            EventStatus.Failed => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
            EventStatus.Forwarded => Results.Content(jsonResult, "application/json", statusCode: 202), // 202 accepted
            _ => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
        };

    internal static string GetEventName<TEvent>()
        => Event<TEvent>.Name.ToShortName().ToCamelCase();

    internal static bool IsEventEmpty(Type eventType)
    {
        var type = eventType;
        while (type != null && type != typeof(object))
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            if (type.GetFields(flags).Length != 0) return false;
            if (type.GetProperties(flags).Length != 0) return false;
            if (type.GetMethods(flags).Any(m => !m.IsSpecialName)) return false;
            if (type.GetEvents(flags).Length != 0) return false;
            type = type.BaseType;
        }
        return true;
    }

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