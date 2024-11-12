using System;
using System.Diagnostics;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class OverridenStateBuilderEventsSyncExtensions
    {
        /// <summary>
        /// Adds synchronous entry handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenStateBuilder AddOnEntry(this IOverridenStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnEntry(action
                .AddStateMachineInvocationContext((builder as StateBuilder).Vertex.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds synchronous exit handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public static IOverridenStateBuilder AddOnExit(this IOverridenStateBuilder builder, Action<IStateActionContext> action)
            => builder.AddOnExit(action
                .AddStateMachineInvocationContext((builder as StateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
