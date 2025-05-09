﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using HttpRequest = Stateflows.Common.HttpRequest;

namespace Stateflows.StateMachines
{
    public static class TransitionExtensions
    {
        private static readonly List<string> Endpoints = new();

        public static IStateBuilder AddHttpInternalTransition(this IStateBuilder builder, string pattern, InternalTransitionBuildAction<HttpRequest>? transitionbuildAction = null, Action<IEndpointConventionBuilder>? endpointbuildAction = null)
        {
            //DependencyInjection.Endpoints.Add(
            //    pattern,
            //    (IEndpointRouteBuilder applicationBuilder) =>
            //    {
            //        applicationBuilder.Map()
            //    }
            //);

            return builder;
        }

        public static IStateBuilder AddHttpGetInternalTransition(this IStateBuilder builder, string pattern, InternalTransitionBuildAction<HttpRequest>? transitionbuildAction = null, Action<IEndpointConventionBuilder>? endpointbuildAction = null)
        {
            if (builder is IBehaviorBuilder behaviorBuilder)
            {
                var behaviorClass = behaviorBuilder.BehaviorClass;

                var fullPattern = $"/{behaviorClass.Type}/{behaviorClass.Name}/{{instance}}{pattern}";
                var endpoint = $"GET {fullPattern}";

                if (Endpoints.Contains(endpoint))
                {
                    return builder;
                }
                else
                {
                    Endpoints.Add(endpoint);
                }

                HttpEvent.UrlOverride = pattern;
                HttpEvent.MethodOverride = "GET";
                builder.AddInternalTransition<HttpRequest>(transitionbuildAction);
                HttpEvent.UrlOverride = null;
                HttpEvent.MethodOverride = null;

                DependencyInjection.Endpoints.Add(
                    pattern,
                    (IEndpointRouteBuilder applicationBuilder) =>
                    {
                        var endpointBuilder = applicationBuilder.MapGet(
                            fullPattern,
                            async (string instance, HttpContext context, IBehaviorLocator locator) =>
                            {
                                RequestResult<IResponse<IResult>>? result = locator.TryLocateBehavior(new BehaviorId(behaviorClass, instance), out var behavior)
                                    ? await behavior.RequestAsync(new HttpRequest() { Method = "GET", Url = pattern, Request = context.Request })
                                    : null;

                                return result?.Status == EventStatus.Consumed
                                    ? result?.Response?.Payload ?? Results.Ok()
                                    : Results.NotFound();
                            }
                        );

                        endpointbuildAction?.Invoke(endpointBuilder);
                    }
                );
            }

            return builder;
        }

        public static IStateBuilder AddHttpPostTransition<TRequestPayload>(this IStateBuilder builder, string pattern, string targetStateName, TransitionBuildAction<HttpRequest<TRequestPayload>>? transitionbuildAction = null, Action<IEndpointConventionBuilder>? endpointbuildAction = null)
        {
            if (builder is IBehaviorBuilder behaviorBuilder)
            {
                var behaviorClass = behaviorBuilder.BehaviorClass;

                var fullPattern = $"/{behaviorClass.Type}/{behaviorClass.Name}/{{instance}}{pattern}";
                var endpoint = $"POST {fullPattern}";

                if (Endpoints.Contains(endpoint))
                {
                    return builder;
                }
                else
                {
                    Endpoints.Add(endpoint);
                }

                HttpEvent.UrlOverride = pattern;
                HttpEvent.MethodOverride = "POST";
                builder.AddTransition<HttpRequest<TRequestPayload>>(targetStateName, transitionbuildAction);
                HttpEvent.UrlOverride = null;
                HttpEvent.MethodOverride = null;

                DependencyInjection.Endpoints.Add(
                    pattern,
                    (IEndpointRouteBuilder applicationBuilder) =>
                    {
                        var endpointBuilder = applicationBuilder.MapPost(
                            fullPattern,
                            async (string instance, TRequestPayload body, HttpContext context, IBehaviorLocator locator) =>
                            {
                                RequestResult<IResponse<IResult>>? result = locator.TryLocateBehavior(new BehaviorId(behaviorClass, instance), out var behavior)
                                    ? await behavior.RequestAsync(new HttpRequest<TRequestPayload>() { Method = "POST", Url = pattern, Request = context.Request, Payload = body })
                                    : null;

                                return result?.Status == EventStatus.Consumed
                                    ? result?.Response?.Payload ?? Results.Ok()
                                    : Results.NotFound();
                            }
                        );

                        endpointbuildAction?.Invoke(endpointBuilder);
                    }
                );
            }

            return builder;
        }
    }
}