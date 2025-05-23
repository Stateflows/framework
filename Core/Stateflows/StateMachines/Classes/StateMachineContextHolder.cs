using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Classes
{
    internal class StateMachineContextHolder : IStateMachineContextHolder
    {
        private readonly Graph graph;
        private readonly RootContext rootContext;
        private readonly IStateflowsLockHandle lockHandle;
        private readonly IStateflowsStorage stateflowsStorage;
        public StateMachineContextHolder(
            Graph graph, RootContext rootContext, IStateflowsLockHandle lockHandle, IStateflowsStorage stateflowsStorage)
        {
            this.graph = graph;
            this.rootContext = rootContext;
            this.lockHandle = lockHandle;
            this.stateflowsStorage = stateflowsStorage;
        }

        public async ValueTask DisposeAsync()
        {
            rootContext.Executor.Dehydrate();
            
            await stateflowsStorage.DehydrateAsync(rootContext.Context);
            
            await lockHandle.DisposeAsync();
        }

        public StateMachineId StateMachineId => rootContext.Id;
        public BehaviorStatus BehaviorStatus => rootContext.Context.Status;
        private IEnumerable<string> expectedEventNames;
        public IEnumerable<string> ExpectedEventNames => expectedEventNames ??= rootContext.Executor.GetExpectedEventNames();
        public IReadOnlyTree<string> CurrentStates => rootContext.StatesTree;
        
        public IStateMachineContext GetStateMachineContext()
            => new StateMachineContext(rootContext);

        public IStateContext GetStateContext(string stateName)
            => new StateContext(graph.AllVertices[stateName], rootContext);
    }
}