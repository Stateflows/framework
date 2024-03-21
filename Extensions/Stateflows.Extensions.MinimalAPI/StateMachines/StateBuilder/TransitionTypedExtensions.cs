using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TransitionTypedExtensions
    {
        #region AddHttpGetInternalTransition
        public static IStateBuilder AddHttpGetInternalTransition<TTransition>(this IStateBuilder builder, string pattern, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest>
            => builder.AddHttpGetInternalTransition(pattern, b => b.AddTransitionEvents<TTransition, HttpRequest>(), endpointbuildAction);
        #endregion

        #region AddHttpPostTransition
        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TTargetState>(this IStateBuilder builder, string pattern, TransitionBuildAction<HttpRequest<TRequestPayload>> transitionBuildAction = null, Action<IEndpointConventionBuilder> endpointBuildAction = null)
            where TTargetState : BaseState
            => builder.AddHttpPostTransition<TRequestPayload>(pattern, StateInfo<TTargetState>.Name, transitionBuildAction, endpointBuildAction);

        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TTransition, TTargetState>(this IStateBuilder builder, string pattern, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest<TRequestPayload>>
            where TTargetState : BaseState
            => builder.AddHttpPostTransition<TRequestPayload>(pattern, StateInfo<TTargetState>.Name, b => b.AddTransitionEvents<TTransition, HttpRequest<TRequestPayload>>(), endpointbuildAction);

        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TTransition>(this IStateBuilder builder, string pattern, string targetVertexName, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest<TRequestPayload>>
            => builder.AddHttpPostTransition<TRequestPayload>(pattern, targetVertexName, b => b.AddTransitionEvents<TTransition, HttpRequest<TRequestPayload>>(), endpointbuildAction);
        #endregion
    }
}