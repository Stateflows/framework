using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineVisitor
    {
        Task StateMachineAddingAsync(string stateMachineName, int stateMachineVersion, bool hasDefaultInstance = false);
        
        Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion);

        Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
            where TStateMachine : class, IStateMachine;

        Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion);

        Task DefaultInitializerAddedAsync(string stateMachineName, int stateMachineVersion);

        Task FinalizerAddedAsync(string stateMachineName, int stateMachineVersion);

        Task VertexAddedAsync(string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string? parentVertexName = null);

        Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
            where TVertex : class, IVertex;

        Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false);

        Task TransitionTypeAddedAsync<TEvent, TTransition>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false)
            where TTransition : class, ITransition<TEvent>;

        Task CustomEventAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, BehaviorStatus[] supportedStatuses);

        Task DoBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType, IEnumerable<Type> eventTypes);

        Task OnEntryBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType);

        Task OnExitBehaviorAddedAsync(string stateMachineName, int stateMachineVersion, string stateName, string behaviorName, string behaviorType);
        
        Task GuardBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName, bool isElse, string behaviorName, string behaviorType);
        
        Task EffectBehaviorAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string? targetVertexName, bool isElse, string behaviorName, string behaviorType);
    }
}
