using System.Threading.Tasks;
using Stateflows.StateMachines.Models;

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

        // public virtual Task ElseTransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     => Task.CompletedTask;
        //
        // public virtual Task TransitionGuardTypeAddedAsync<TEvent, TGuard>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TGuard : class, ITransitionGuard<TEvent>
        //     => Task.CompletedTask;
        //
        // public virtual Task TransitionEffectTypeAddedAsync<TEvent, TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, ITransitionEffect<TEvent>
        //     => Task.CompletedTask;
        //
        // public virtual Task ElseTransitionEffectTypeAddedAsync<TEvent, TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, ITransitionEffect<TEvent>
        //     => Task.CompletedTask;
        //
        // public virtual Task DefaultTransitionGuardTypeAddedAsync<TGuard>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TGuard : class, IDefaultTransitionGuard
        //     => Task.CompletedTask;
        //
        // public virtual Task DefaultTransitionEffectTypeAddedAsync<TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, IDefaultTransitionEffect
        //     => Task.CompletedTask;
        //
        // public virtual Task ElseDefaultTransitionEffectTypeAddedAsync<TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, IDefaultTransitionEffect
        //     => Task.CompletedTask;
    }
}