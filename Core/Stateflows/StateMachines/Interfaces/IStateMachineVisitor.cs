using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Receives state-machine definition metadata while registrations are visited, rather than observing runtime state changes.</summary>
    /// <remarks>Implementations can build diagrams, catalogs, or integration metadata.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public interface IStateMachineVisitor
    {
        /// <summary>Visits a transition whose trigger is represented by an event definition, including named structural events.</summary>
        /// <param name="definition">The trigger's event identity and structural metadata.</param>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="sourceVertexName">The registered source vertex name.</param>
        /// <param name="targetVertexName">The target vertex name, or null for an internal transition.</param>
        /// <param name="isElse">Whether this is the fallback when ordinary guards do not select a transition.</param>
        /// <returns>A task completing when the metadata is processed.</returns>
        Task TransitionAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
            string sourceVertexName, string targetVertexName = null, bool isElse = false)
            => Task.CompletedTask;

        /// <summary>Visits an event definition exposed by an embedded do behavior.</summary>
        /// <param name="definition">The event metadata exposed by the embedded behavior.</param>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="stateName">The state hosting the behavior.</param>
        /// <param name="behaviorName">The embedded behavior class name.</param>
        /// <param name="behaviorType">The embedded behavior kind.</param>
        /// <returns>A task completing when the metadata is processed.</returns>
        Task DoBehaviorAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
            string stateName, string behaviorName, string behaviorType) => Task.CompletedTask;

        /// <summary>Begins visiting a machine definition before its elements are reported.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="hasDefaultInstance">Whether a default instance is registered.</param>
        /// <returns>A task completing before element visitation proceeds.</returns>
        Task StateMachineAddingAsync(string stateMachineName, int stateMachineVersion, bool hasDefaultInstance = false);
        
        /// <summary>Completes visitation of a machine definition after its elements have been reported.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <returns>A task completing when definition visitation is complete.</returns>
        Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion);

        /// <summary>Reports the CLR type associated with a machine definition.</summary>
        /// <typeparam name="TStateMachine">The typed definition.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <returns>A task completing when the type metadata is processed.</returns>
        Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
            where TStateMachine : class, IStateMachine;

        /// <summary>Reports an initializer accepting a particular initialization event type.</summary>
        /// <typeparam name="TInitializationEvent">The accepted initialization payload.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <returns>A task completing when the initializer metadata is processed.</returns>
        Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion);

        /// <summary>Reports the machine's default initializer.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <returns>A task completing when the initializer metadata is processed.</returns>
        Task DefaultInitializerAddedAsync(string stateMachineName, int stateMachineVersion);

        /// <summary>Reports the machine's finalization callback.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <returns>A task completing when the finalizer metadata is processed.</returns>
        Task FinalizerAddedAsync(string stateMachineName, int stateMachineVersion);

        /// <summary>Reports a state or pseudostate and its containment in the definition.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="vertexName">The registered vertex name.</param>
        /// <param name="vertexType">The vertex's state or pseudostate kind.</param>
        /// <param name="parentVertexName">The containing vertex, or null for a top-level vertex.</param>
        /// <returns>A task completing when the vertex metadata is processed.</returns>
        Task VertexAddedAsync(string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string? parentVertexName = null);

        /// <summary>Reports the CLR type associated with a vertex.</summary>
        /// <typeparam name="TVertex">The vertex definition type.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="vertexName">The registered vertex name.</param>
        /// <returns>A task completing when the type metadata is processed.</returns>
        Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
            where TVertex : class, IVertex;

        /// <summary>Reports a CLR-event-triggered transition; completion transitions use <see cref="Completion"/>.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="sourceVertexName">The registered source vertex.</param>
        /// <param name="targetVertexName">The target vertex, or null for an internal transition.</param>
        /// <param name="isElse">Whether the transition is a fallback.</param>
        /// <returns>A task completing when transition metadata is processed.</returns>
        Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false);

        /// <summary>Reports the CLR implementation type associated with a transition.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <typeparam name="TTransition">The transition implementation.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="sourceVertexName">The registered source vertex.</param>
        /// <param name="targetVertexName">The target vertex, or null for an internal transition.</param>
        /// <param name="isElse">Whether the transition is a fallback.</param>
        /// <returns>A task completing when type metadata is processed.</returns>
        Task TransitionTypeAddedAsync<TEvent, TTransition>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false)
            where TTransition : class, ITransition<TEvent>;

        /// <summary>Reports a custom event handled by a machine and the behavior statuses in which it is supported.</summary>
        /// <typeparam name="TEvent">The custom event payload.</typeparam>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="supportedStatuses">The statuses in which the event handler is available.</param>
        /// <returns>A task completing when event metadata is processed.</returns>
        Task CustomEventAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, BehaviorStatus[] supportedStatuses);

        /// <summary>Reports an embedded do behavior and its CLR event types.</summary>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="stateName">The state hosting the behavior.</param>
        /// <param name="behaviorName">The embedded behavior class name.</param>
        /// <param name="behaviorType">The embedded behavior kind.</param>
        /// <param name="eventTypes">Event types exposed by the embedded behavior.</param>
        /// <returns>A task completing when behavior metadata is processed.</returns>
        Task DoBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType, IEnumerable<Type> eventTypes);

        /// <summary>Reports a behavior executed as a state's entry action.</summary>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="stateName">The state whose entry invokes the behavior.</param>
        /// <param name="behaviorName">The invoked behavior class name.</param>
        /// <param name="behaviorType">The invoked behavior kind.</param>
        /// <returns>A task completing when action metadata is processed.</returns>
        Task OnEntryBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType);

        /// <summary>Reports a behavior executed as a state's exit action.</summary>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="stateName">The state whose exit invokes the behavior.</param>
        /// <param name="behaviorName">The invoked behavior class name.</param>
        /// <param name="behaviorType">The invoked behavior kind.</param>
        /// <returns>A task completing when action metadata is processed.</returns>
        Task OnExitBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType);
        
        /// <summary>Reports a behavior used to evaluate a transition guard.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="sourceVertexName">The transition's source vertex.</param>
        /// <param name="targetVertexName">The target, or null for an internal transition.</param>
        /// <param name="isElse">Whether the transition is a fallback.</param>
        /// <param name="behaviorName">The guard behavior class name.</param>
        /// <param name="behaviorType">The guard behavior kind.</param>
        /// <returns>A task completing when guard metadata is processed.</returns>
        Task GuardBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName, bool isElse, string behaviorName, string behaviorType);
        
        /// <summary>Reports a behavior used to execute a transition effect.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="stateMachineName">The containing machine name.</param>
        /// <param name="stateMachineVersion">The definition version.</param>
        /// <param name="sourceVertexName">The transition's source vertex.</param>
        /// <param name="targetVertexName">The target, or null for an internal transition.</param>
        /// <param name="isElse">Whether the transition is a fallback.</param>
        /// <param name="behaviorName">The effect behavior class name.</param>
        /// <param name="behaviorType">The effect behavior kind.</param>
        /// <returns>A task completing when effect metadata is processed.</returns>
        Task EffectBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName, bool isElse, string behaviorName, string behaviorType);
    }
}
