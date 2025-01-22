using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityEvents<out TReturn>
    {
        TReturn AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync);
        
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TInitializer>()
            where TInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(c
                => ((BaseContext)c).NodeScope.GetDefaultInitializer<TInitializer>(c)?.OnInitializeAsync()
            );

        TReturn AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(c
                => ((BaseContext)c).NodeScope.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent)
            );

        TReturn AddFinalizer(Func<IActivityActionContext, Task> actionAsync);

        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(c
                => ((BaseContext)c).NodeScope.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync()
            );
    }
}
