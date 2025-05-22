using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Classes
{
    internal class ActivityContextProvider : IActivityContextProvider
    {
        private readonly ActivitiesRegister register;
        private readonly IStateflowsStorage stateflowsStorage;
        private readonly IStateflowsLock stateflowsLock;
        private readonly IServiceProvider serviceProvider;
        
        public ActivityContextProvider(
            ActivitiesRegister register,
            IStateflowsStorage stateflowsStorage,
            IStateflowsLock stateflowsLock,
            IServiceProvider serviceProvider
        )
        {
            this.register = register;
            this.stateflowsStorage = stateflowsStorage;
            this.stateflowsLock = stateflowsLock;
            this.serviceProvider = serviceProvider;
        }
        
        public async Task<(bool Success, IActivityContextHolder ContextHolder)> TryProvideAsync(ActivityId activityId)
        {
            var lockHandle = await stateflowsLock.AquireLockAsync(activityId);
            
            var stateflowsContext = await stateflowsStorage.HydrateAsync(activityId);
            
            var key = stateflowsContext.Version != 0
                ? $"{activityId.Name}.{stateflowsContext.Version}"
                : $"{activityId.Name}.current";
            
            if (!register.Activities.TryGetValue(key, out var graph))
            {
                await lockHandle.DisposeAsync();
                
                return (false, null);
            }

            var eventHolder = new EventHolder<bool>();
            
            var executor = new Executor(register, graph, serviceProvider);
            var rootContext = new RootContext(stateflowsContext);
            await executor.HydrateAsync(rootContext);

            return (true, new ActivityContextHolder(graph, rootContext, lockHandle, stateflowsStorage));
        }
    }
}