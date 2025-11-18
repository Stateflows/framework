using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

internal enum EndpointKind
{
    Event,
    GetAllBehaviorClasses,
    GetBehaviorClasses,
    GetAllInstances,
    GetInstances,
    Custom,
    All
}

internal class EndpointConfigurationRule
{
    public EndpointKind Kind { get; init; }
    public string? BehaviorType { get; init; }
    public BehaviorClass? BehaviorClass { get; init; }
    public Type? Event { get; init; }

    public bool Disable { get; init; } = false;
    public Func<string, string>? RouteUpdater { get; init; }
    public Action<IEndpointConventionBuilder>? EndpointConfigurator { get; init; }
}

internal class ConfigurationInterceptor(IServiceProvider serviceProvider) : EndpointDefinitionInterceptor
{
    public List<EndpointConfigurationRule> Rules { get; } = new();
    
    public override bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.Event or EndpointKind.All))
        {
            if (rule.BehaviorType == null || rule.BehaviorType == behaviorClass.Type)
            {
                if (rule.BehaviorClass == null || rule.BehaviorClass == behaviorClass)
                {
                    if (rule.Event == null || rule.Event == typeof(TEvent))
                    {
                        if (rule.Disable)
                        {
                            return false;
                        }

                        route = rule.RouteUpdater?.Invoke(route) ?? route;
                    }
                }
            }
        }
        
        return true;
    }

    public override void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.Event or EndpointKind.All))
        {
            if (rule.BehaviorType == null || rule.BehaviorType == behaviorClass.Type)
            {
                if (rule.BehaviorClass == null || rule.BehaviorClass == behaviorClass)
                {
                    if (rule.Event == null || rule.Event == typeof(TEvent))
                    {
                        rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
                    }
                }
            }
        }
    }

    public override bool BeforeGetClassesEndpointDefinition(string behaviorType, ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule => 
                rule.Kind is EndpointKind.GetBehaviorClasses or EndpointKind.All &&
                (
                    rule.BehaviorType == behaviorType ||
                    (rule.BehaviorType == null && rule.BehaviorClass == null)
                )
            )
        )
        {
            if (rule.Disable)
            {
                return false;
            }

            route = rule.RouteUpdater?.Invoke(route) ?? route;
        }
        
        return true;
    }

    public override void AfterGetClassesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => 
                rule.Kind is EndpointKind.GetBehaviorClasses or EndpointKind.All &&
                (
                    rule.BehaviorType == behaviorType ||
                    (rule.BehaviorType == null && rule.BehaviorClass == null)
                )
            )
        )
        {
            rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
        }
    }

    public override bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.GetAllBehaviorClasses or EndpointKind.All && rule.BehaviorType == null && rule.BehaviorClass == null))
        {
            if (rule.Disable)
            {
                return false;
            }

            route = rule.RouteUpdater?.Invoke(route) ?? route;
        }
        
        return true;
    }

    public override void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.GetAllBehaviorClasses or EndpointKind.All && rule.BehaviorType == null && rule.BehaviorClass == null))
        {
            rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
        }
    }

    public override bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule => 
                     rule.Kind is EndpointKind.GetInstances or EndpointKind.All &&
                     (
                         (rule.BehaviorType == null && rule.BehaviorClass == behaviorClass) ||
                         rule.BehaviorType == behaviorClass.Type ||
                         (rule.BehaviorType == null && rule.BehaviorClass == null)
                     )
                )
        )
        {
            if (rule.Disable)
            {
                return false;
            }

            route = rule.RouteUpdater?.Invoke(route) ?? route;
        }
        
        return true;
    }

    public override void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => 
                     rule.Kind is EndpointKind.GetInstances or EndpointKind.All &&
                     (
                         (rule.BehaviorType == null && rule.BehaviorClass == behaviorClass) ||
                         rule.BehaviorType == behaviorClass.Type ||
                         (rule.BehaviorType == null && rule.BehaviorClass == null)
                     )
                )
        )
        {
            rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
        }
    }

    public override bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule =>
                rule.Kind is EndpointKind.GetInstances or EndpointKind.All &&
                (
                    rule.BehaviorType == behaviorType || 
                    (rule.BehaviorType == null && rule.BehaviorClass == null)
                )
            )
        )
        {
            if (rule.Disable)
            {
                return false;
            }

            route = rule.RouteUpdater?.Invoke(route) ?? route;
        }
        
        return true;
    }

    public override void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule =>
                rule.Kind is EndpointKind.GetInstances or EndpointKind.All &&
                (
                    rule.BehaviorType == behaviorType || 
                    (rule.BehaviorType == null && rule.BehaviorClass == null)
                )
            )
        )
        {
            rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
        }
    }

    public override bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.GetAllInstances or EndpointKind.All && rule.BehaviorType == null && rule.BehaviorClass == null))
        {
            if (rule.Disable)
            {
                return false;
            }

            route = rule.RouteUpdater?.Invoke(route) ?? route;
        }
        
        return true;
    }

    public override void AfterGetAllInstancesEndpointDefinition(string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.GetAllInstances or EndpointKind.All && rule.BehaviorType == null && rule.BehaviorClass == null))
        {
            rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
        }
    }

    public override bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, ref string[] methods, ref string route)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.Custom or EndpointKind.All))
        {
            if (rule.BehaviorType == null || rule.BehaviorType == behaviorClass.Type)
            {
                if (rule.BehaviorClass == null || rule.BehaviorClass == behaviorClass)
                {
                    if (rule.Disable)
                    {
                        return false;
                    }

                    route = rule.RouteUpdater?.Invoke(route) ?? route;
                }
            }
        }

        return true;
    }

    public override void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, string[] methods, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var rule in Rules.Where(rule => rule.Kind is EndpointKind.Custom or EndpointKind.All))
        {
            if (rule.BehaviorType == null || rule.BehaviorType == behaviorClass.Type)
            {
                if (rule.BehaviorClass == null || rule.BehaviorClass == behaviorClass)
                {
                    rule.EndpointConfigurator?.Invoke(routeHandlerBuilder);
                }
            }
        }
    }

    public override IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        => behaviorClasses.Where(c => 
            !Rules.Any(rule =>
                rule.Disable &&
                rule.Kind is EndpointKind.All &&
                (
                    rule.BehaviorClass == c ||
                    (rule.BehaviorClass == null && rule.BehaviorType == c.Type)
                )
            )
        ).ToArray();
}