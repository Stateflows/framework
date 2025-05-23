using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Classes
{
    internal class StateMachineContextProvider : IStateMachineContextProvider
    {
        private readonly StateMachinesRegister register;
        private readonly IStateflowsStorage stateflowsStorage;
        private readonly IStateflowsLock stateflowsLock;
        private readonly IServiceProvider serviceProvider;
        
        public StateMachineContextProvider(
            StateMachinesRegister register,
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
        
        public async Task<(bool Success, IStateMachineContextHolder ContextHolder)> TryProvideAsync(StateMachineId stateMachineId)
        {
            var lockHandle = await stateflowsLock.AquireLockAsync(stateMachineId);
            
            var stateflowsContext = await stateflowsStorage.HydrateAsync(stateMachineId);
            
            var key = stateflowsContext.Version != 0
                ? $"{stateMachineId.Name}.{stateflowsContext.Version}"
                : $"{stateMachineId.Name}.current";
            
            if (!register.StateMachines.TryGetValue(key, out var graph))
            {
                await lockHandle.DisposeAsync();
                
                return (false, null);
            }

            var eventHolder = new EventHolder<bool>();
            
            var executor = new Executor(register, graph, serviceProvider, stateflowsContext, eventHolder);
            var rootContext = new RootContext(stateflowsContext, executor, eventHolder);
            await executor.HydrateAsync();

            return (true, new StateMachineContextHolder(graph, rootContext, lockHandle, stateflowsStorage));
        }
    }
}