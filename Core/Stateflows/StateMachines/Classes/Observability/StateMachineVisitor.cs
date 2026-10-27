using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Base definition visitor with no-op implementations, allowing individual metadata callbacks to be overridden.</summary>
    /// <remarks>This visits registrations, not runtime executions.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public abstract class StateMachineVisitor : IStateMachineVisitor
    {
        /// <inheritdoc/>
        public virtual Task TransitionAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
            string sourceVertexName, string targetVertexName = null, bool isElse = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task DoBehaviorAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
            string stateName, string behaviorName, string behaviorType) => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task StateMachineAddingAsync(string stateMachineName, int stateMachineVersion, bool hasDefaultInstance = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
            where TStateMachine : class, IStateMachine
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task DefaultInitializerAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task FinalizerAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task VertexAddedAsync(string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string parentVertexName = null)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
            where TVertex : class, IVertex
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null, bool isElse = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task TransitionTypeAddedAsync<TEvent, TTransition>(string stateMachineName, int stateMachineVersion,
            string sourceVertexName, string targetVertexName = null, bool isElse = false) where TTransition : class, ITransition<TEvent>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task DoBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName,
            string behaviorType, IEnumerable<Type> eventTypes)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task OnEntryBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName,
            string behaviorType)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task OnExitBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName,
            string behaviorType)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task GuardBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
            string? targetVertexName, bool isElse, string behaviorName, string behaviorType)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task EffectBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
            string? targetVertexName, bool isElse, string behaviorName, string behaviorType)
            => Task.CompletedTask;
    }
}