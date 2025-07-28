using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineVisitor : IStateMachineVisitor
    {
        public virtual Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        public virtual Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
            where TStateMachine : class, IStateMachine
            => Task.CompletedTask;

        public virtual Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        public virtual Task DefaultInitializerAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;
        
        public virtual Task FinalizerAddedAsync(string stateMachineName, int stateMachineVersion)
            => Task.CompletedTask;

        public virtual Task VertexAddedAsync(string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string parentVertexName = null)
            => Task.CompletedTask;

        public virtual Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
            where TVertex : class, IVertex
            => Task.CompletedTask;

        public virtual Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null, bool isElse = false)
            => Task.CompletedTask;

        public virtual Task TransitionTypeAddedAsync<TEvent, TTransition>(string stateMachineName, int stateMachineVersion,
            string sourceVertexName, string targetVertexName = null, bool isElse = false) where TTransition : class, ITransition<TEvent>
            => Task.CompletedTask;

        public virtual Task CustomEventAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}