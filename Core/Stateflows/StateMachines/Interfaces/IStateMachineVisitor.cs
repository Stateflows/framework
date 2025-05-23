using System.Threading.Tasks;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines
{
    public interface IStateMachineVisitor
    {
        Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion);

        Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
            where TStateMachine : class, IStateMachine;

        Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion);

        Task DefaultInitializerAddedAsync(string stateMachineName, int stateMachineVersion);
        
        Task FinalizerAddedAsync(string stateMachineName, int stateMachineVersion);
        
        Task VertexAddedAsync(string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string parentVertexName = null);
        
        Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
            where TVertex : class, IVertex;

        Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null, bool isElse = false);

        // Task ElseTransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null);
        //
        // Task TransitionGuardTypeAddedAsync<TEvent, TGuard>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TGuard : class, ITransitionGuard<TEvent>;
        //
        // Task TransitionEffectTypeAddedAsync<TEvent, TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, ITransitionEffect<TEvent>;
        //
        // Task ElseTransitionEffectTypeAddedAsync<TEvent, TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, ITransitionEffect<TEvent>;
        //
        // Task DefaultTransitionGuardTypeAddedAsync<TGuard>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TGuard : class, IDefaultTransitionGuard;
        //
        // Task DefaultTransitionEffectTypeAddedAsync<TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, IDefaultTransitionEffect;
        //
        // Task ElseDefaultTransitionEffectTypeAddedAsync<TEffect>(string stateMachineName, int stateMachineVersion, string sourceVertexName, string targetVertexName = null)
        //     where TEffect : class, IDefaultTransitionEffect;
    }
}
