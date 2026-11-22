using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>No-op base for finalized-definition visitors; type callbacks do not forward to general callbacks.</summary>
public abstract class StateMachineVisitor : IStateMachineVisitor
{
    public virtual Task StateMachineAddingAsync(string stateMachineId, string stateMachineName, int stateMachineVersion, BehaviorClass? ownerClass, BehaviorClass? parentClass, bool hasDefaultInstance) => Task.CompletedTask;
    public virtual Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineId, string stateMachineName, int stateMachineVersion) where TStateMachine : class, IStateMachine => Task.CompletedTask;
    public virtual Task StateMachineAddedAsync(string stateMachineId, string stateMachineName, int stateMachineVersion) => Task.CompletedTask;
    public virtual Task VertexAddedAsync(string vertexId, string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string? parentVertexId, string? regionId, int? regionIndex) => Task.CompletedTask;
    public virtual Task VertexTypeAddedAsync<TVertex>(string vertexId, string stateMachineName, int stateMachineVersion) where TVertex : class, IVertex => Task.CompletedTask;
    public virtual Task RegionAddedAsync(string regionId, string stateMachineName, int stateMachineVersion, string parentVertexId, int regionIndex) => Task.CompletedTask;
    public virtual Task TransitionAddedAsync<TEvent>(string transitionId, string stateMachineName, int stateMachineVersion, string sourceVertexId, string? targetVertexId, bool isElse, bool isLocal, bool polymorphicTriggers, IReadOnlyList<string> triggerNames) => Task.CompletedTask;
    public virtual Task TransitionTypeAddedAsync<TEvent, TTransition>(string transitionId, string stateMachineName, int stateMachineVersion) where TTransition : class, ITransition<TEvent> => Task.CompletedTask;
    public virtual Task InitializerAddedAsync<TEvent>(string initializerId, string stateMachineName, int stateMachineVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;
    public virtual Task DefaultInitializerAddedAsync(string initializerId, string stateMachineName, int stateMachineVersion) => Task.CompletedTask;
    public virtual Task FinalizerAddedAsync(string finalizerId, string stateMachineName, int stateMachineVersion) => Task.CompletedTask;
    public virtual Task ActionAddedAsync(string actionId, string stateMachineName, int stateMachineVersion, string ownerElementId, StateMachineOperation operationKind, int actionIndex) => Task.CompletedTask;
    public virtual Task ActionTypeAddedAsync<TImplementation>(string actionId, string stateMachineName, int stateMachineVersion) where TImplementation : class => Task.CompletedTask;
    public virtual Task ActionBehaviorAddedAsync(string actionId, string stateMachineName, int stateMachineVersion, BehaviorClass actionBehaviorClass) => Task.CompletedTask;
    public virtual Task GuardAddedAsync(string guardId, string stateMachineName, int stateMachineVersion, string ownerElementId, string? parentGuardId, int guardIndex, GuardKind guardKind) => Task.CompletedTask;
    public virtual Task GuardTypeAddedAsync<TImplementation>(string guardId, string stateMachineName, int stateMachineVersion) where TImplementation : class => Task.CompletedTask;
    public virtual Task GuardBehaviorAddedAsync(string guardId, string stateMachineName, int stateMachineVersion, BehaviorClass guardBehaviorClass) => Task.CompletedTask;
    public virtual Task DeferralAddedAsync<TEvent>(string deferralId, string stateMachineName, int stateMachineVersion, string vertexId, IReadOnlyList<string> eventNames) => Task.CompletedTask;
    public virtual Task HostedBehaviorAddedAsync(string hostedBehaviorId, string stateMachineName, int stateMachineVersion, string vertexId, BehaviorClass hostedBehaviorClass) => Task.CompletedTask;
    public virtual Task HostedBehaviorTypeAddedAsync<TImplementation>(string hostedBehaviorId, string stateMachineName, int stateMachineVersion) where TImplementation : class => Task.CompletedTask;
    public virtual Task HostedBehaviorEventAddedAsync<TEvent>(string hostedBehaviorEventId, string stateMachineName, int stateMachineVersion, string hostedBehaviorId, IReadOnlyList<string> eventNames) => Task.CompletedTask;
}
