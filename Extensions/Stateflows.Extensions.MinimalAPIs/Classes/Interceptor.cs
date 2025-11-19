using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

internal class Interceptor(IEnumerable<IEndpointDefinitionInterceptor> interceptors) : EndpointDefinitionInterceptor
{
    public override IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        => interceptors.Aggregate(behaviorClasses, (current, interceptor) => interceptor.FilterBehaviorClasses(current));

    public override bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeEventEndpointDefinition<TEvent>(behaviorClass, ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterEventEndpointDefinition<TEvent>(behaviorClass, method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeGetClassesEndpointDefinition(string behaviorType, ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeGetClassesEndpointDefinition(behaviorType, ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterGetClassesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterGetClassesEndpointDefinition(behaviorType, method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeGetAllClassesEndpointDefinition(ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterGetAllClassesEndpointDefinition(method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeGetAllInstancesEndpointDefinition(ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterGetAllInstancesEndpointDefinition(string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterGetAllInstancesEndpointDefinition(method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeGetInstancesEndpointDefinition(behaviorType, ref method, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterGetInstancesEndpointDefinition(behaviorType, method, route, routeHandlerBuilder);
        }
    }

    public override bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, ref string[] methods, ref string route)
    {
        foreach (var interceptor in interceptors)
        {
            if (!interceptor.BeforeCustomEndpointDefinition(behaviorClass, ref methods, ref route))
            {
                return false;
            }
        }

        return true;
    }

    public override void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, string[] methods, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    {
        foreach (var interceptor in interceptors.Reverse())
        {
            interceptor.AfterCustomEndpointDefinition(behaviorClass, methods, route, routeHandlerBuilder);
        }
    }
}