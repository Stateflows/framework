using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Models;
using Stateflows.Common.Context.Classes;

namespace Stateflows.Actions.Classes
{
    internal class ActionContextHolder : IActionContextHolder
    {
        private readonly RootContext rootContext;
        private readonly IStateflowsLockHandle lockHandle;
        private readonly IStateflowsStorage stateflowsStorage;
        public ActionContextHolder(RootContext rootContext, IStateflowsLockHandle lockHandle, IStateflowsStorage stateflowsStorage)
        {
            this.rootContext = rootContext;
            this.lockHandle = lockHandle;
            this.stateflowsStorage = stateflowsStorage;
        }

        public async ValueTask DisposeAsync()
        {
            await rootContext.Executor.DehydrateAsync(rootContext.EventHolder);
            
            await stateflowsStorage.DehydrateAsync(rootContext.Context);
            
            await lockHandle.DisposeAsync();
        }

        public ActionId ActionId => rootContext.Id;
        public BehaviorStatus BehaviorStatus => rootContext.Context.Status;
        
        public IBehaviorContext GetActionContext()
            => new BehaviorContext(rootContext.Context, rootContext.ServiceProvider);
    }
}