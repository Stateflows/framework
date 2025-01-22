using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEvents<out TReturn>
    {
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);
        
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TDefaultInitializer>()
            where TDefaultInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(c => ((BaseContext)c).Context.Executor.GetDefaultInitializer<TDefaultInitializer>(c)?.OnInitializeAsync());

        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);
        
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(c => ((BaseContext)c).Context.Executor.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent));

        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);
        
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(c => ((BaseContext)c).Context.Executor.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync());

    }
}
