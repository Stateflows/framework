using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Models;

namespace Stateflows.Activities.Classes
{
    internal class ActivityContextHolder : IActivityContextHolder
    {
        private readonly Graph graph;
        private readonly RootContext rootContext;
        private readonly IStateflowsLockHandle lockHandle;
        private readonly IStateflowsStorage stateflowsStorage;
        public ActivityContextHolder(
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

        public ActivityId ActivityId => rootContext.Id;
        public BehaviorStatus BehaviorStatus => rootContext.Context.Status;
        private IEnumerable<string> expectedEventNames;
        // public IEnumerable<string> ExpectedEventNames => expectedEventNames ??= rootContext.Executor.GetExpectedEventNamesAsync();
        public async Task<IEnumerable<string>> GetExpectedEventNamesAsync()
            => expectedEventNames ??= await rootContext.Executor.GetExpectedEventNamesAsync();
        public IReadOnlyTree<string> ActiveNodes => rootContext.Executor.GetNodesTree();
        
        public IActivityContext GetActivityContext()
            => new ActivityContext(rootContext, rootContext.Executor.NodeScope);

        public IActivityNodeContext GetNodeContext(string nodeName)
            => new ActivityNodeContext(rootContext, rootContext.Executor.NodeScope, graph.AllNodes[nodeName]);
    }
}