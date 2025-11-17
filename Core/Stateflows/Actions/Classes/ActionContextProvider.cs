using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Engine;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions.Classes
{
    internal class ActionContextProvider : IActionContextProvider
    {
        private readonly ActionsRegister register;
        private readonly IStateflowsStorage stateflowsStorage;
        private readonly IStateflowsLock stateflowsLock;
        private readonly IServiceProvider serviceProvider;
        
        public ActionContextProvider(
            ActionsRegister register,
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
        
        public async Task<(bool Success, IActionContextHolder ContextHolder)> TryProvideAsync(ActionId stateMachineId)
        {
            var lockHandle = await stateflowsLock.AquireLockAsync(stateMachineId);
            
            var stateflowsContext = await stateflowsStorage.HydrateAsync(stateMachineId);
            
            var key = stateflowsContext.Version != 0
                ? $"{stateMachineId.Name}.{stateflowsContext.Version}"
                : $"{stateMachineId.Name}.current";
            
            if (!register.Actions.TryGetValue(key, out var actionModel))
            {
                await lockHandle.DisposeAsync();
                
                return (false, null);
            }

            var eventHolder = new EventHolder<bool>();
            
            var executor = new Executor(register, stateflowsContext, serviceProvider, actionModel);
            var rootContext = new RootContext(stateflowsContext, executor, eventHolder, serviceProvider);
            await executor.HydrateAsync(eventHolder);

            return (true, new ActionContextHolder(rootContext, lockHandle, stateflowsStorage));
        }
    }
}