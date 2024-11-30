using System;
using System.Diagnostics;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class OverridenCompositeStateBuilderEventsSyncExtensions
    {
        /// <summary>
        /// Adds synchronous initialization handler to current composite state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOnInitialize(this IOverridenCompositeStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnInitialize(action
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Region.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds synchronous finalization handler to current composite state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOnFinalize(this IOverridenCompositeStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnFinalize(action
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Region.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds synchronous entry handler to current composite state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOnEntry(this IOverridenCompositeStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnEntry(action
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Region.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds synchronous exit handler to current composite state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOnExit(this IOverridenCompositeStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnExit(action
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Region.Graph)
                .ToAsync()
            );
    }
}
