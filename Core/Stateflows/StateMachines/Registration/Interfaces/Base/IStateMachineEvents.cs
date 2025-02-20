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
        /// <summary>
        /// Adds a default initializer to the state machine.
        /// </summary>
        /// <param name="actionAsync">The asynchronous action to execute during initialization.</param>
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);

        /// <summary>
        /// Adds a default initializer to the state machine.
        /// </summary>
        /// <typeparam name="TDefaultInitializer">The type of the default initializer.</typeparam>
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TDefaultInitializer>()
            where TDefaultInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(async c => await (await ((BaseContext)c).Context.Executor.GetDefaultInitializerAsync<TDefaultInitializer>(c)).OnInitializeAsync());

        /// <summary>
        /// Adds an initializer for a specific initialization event to the state machine.
        /// </summary>
        /// <typeparam name="TInitializationEvent">The type of the initialization event.</typeparam>
        /// <param name="actionAsync">The asynchronous action to execute during initialization.</param>
        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        /// <summary>
        /// Adds an initializer for a specific initialization event to the state machine.
        /// </summary>
        /// <typeparam name="TInitializationEvent">The type of the initialization event.</typeparam>
        /// <typeparam name="TInitializer">The type of the initializer.</typeparam>
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(async c => await (await ((BaseContext)c).Context.Executor.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent));

        /// <summary>
        /// Adds a finalizer to the state machine.
        /// </summary>
        /// <param name="actionAsync">The asynchronous action to execute during finalization.</param>
        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);

        /// <summary>
        /// Adds a finalizer to the state machine.
        /// </summary>
        /// <typeparam name="TFinalizer">The type of the finalizer.</typeparam>
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(async c => await (await ((BaseContext)c).Context.Executor.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync());
    }
}
