using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TransitionTypedExtensions
    {
        #region AddHttpGetInternalTransition
        public static IStateBuilder AddHttpGetInternalTransition<TResponsePayload, TTransition>(this IStateBuilder builder, string pattern, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest<TResponsePayload>>
            => builder.AddHttpGetInternalTransition<TResponsePayload>(pattern, b => b.AddTransitionEvents<TTransition, HttpRequest<TResponsePayload>>(), endpointbuildAction);
        #endregion

        #region AddHttpPostTransition
        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TResponsePayload, TTargetState>(this IStateBuilder builder, string pattern, TransitionBuildAction<HttpRequest<TRequestPayload, TResponsePayload>> transitionbuildAction = null, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTargetState : BaseState
            => builder.AddHttpPostTransition<TRequestPayload, TResponsePayload>(pattern, StateInfo<TTargetState>.Name, transitionbuildAction, endpointbuildAction);

        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TResponsePayload, TTransition, TTargetState>(this IStateBuilder builder, string pattern, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest<TRequestPayload, TResponsePayload>>
            where TTargetState : BaseState
            => builder.AddHttpPostTransition<TRequestPayload, TResponsePayload>(pattern, StateInfo<TTargetState>.Name, b => b.AddTransitionEvents<TTransition, HttpRequest<TRequestPayload, TResponsePayload>>(), endpointbuildAction);

        public static IStateBuilder AddHttpPostTransition<TRequestPayload, TResponsePayload, TTransition>(this IStateBuilder builder, string pattern, string targetVertexName, Action<IEndpointConventionBuilder> endpointbuildAction = null)
            where TTransition : Transition<HttpRequest<TRequestPayload, TResponsePayload>>
            => builder.AddHttpPostTransition<TRequestPayload, TResponsePayload>(pattern, targetVertexName, b => b.AddTransitionEvents<TTransition, HttpRequest<TRequestPayload, TResponsePayload>>(), endpointbuildAction);
        #endregion
    }
}