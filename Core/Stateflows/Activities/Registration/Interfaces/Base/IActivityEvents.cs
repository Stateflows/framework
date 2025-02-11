using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityEvents<out TReturn>
    {
        TReturn AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync);
        
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TInitializer>()
            where TInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(async c
                => await (await ((BaseContext)c).NodeScope.GetDefaultInitializerAsync<TInitializer>(c)).OnInitializeAsync()
            );

        TReturn AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(async c
                => await (await ((BaseContext)c).NodeScope.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent)
            );

        TReturn AddFinalizer(Func<IActivityActionContext, Task> actionAsync);

        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(async c
                => await (await ((BaseContext)c).NodeScope.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync()
            );
    }
}
