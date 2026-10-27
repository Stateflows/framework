using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Registers machine initialization acceptance predicates and finalization callbacks.</summary>
    /// <typeparam name="TReturn">The machine builder returned after registration.</typeparam>
    /// <remarks>Initializers return true to accept initialization; false prevents entry into the initial configuration.
    /// Default initializers support initialization without a typed request. Finalizers run when the machine finalizes.
    /// </remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IStateMachineEvents<out TReturn>
    {
        /// <summary>Registers a predicate that accepts or rejects default State Machine initialization.</summary>
        /// <param name="actionAsync">Asynchronous predicate receiving the initialization context. Return true to accept
        /// initialization and enter the initial configuration, or false to reject it.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);

        /// <summary>Registers a predicate that accepts or rejects default State Machine initialization.</summary>
        /// <typeparam name="TDefaultInitializer">Typed default-initialization predicate implementing
        /// <see cref="IDefaultInitializer" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TDefaultInitializer>()
            where TDefaultInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(async c => await (await ((BaseContext)c).Context.Executor.GetDefaultInitializerAsync<TDefaultInitializer>(c)).OnInitializeAsync());

        /// <summary>Registers a predicate that accepts or rejects initialization by a specific event type.</summary>
        /// <typeparam name="TInitializationEvent">Initialization event payload type passed to the initializer through its
        /// context.</typeparam>
        /// <param name="actionAsync">Asynchronous predicate receiving the initialization context. Return true to accept
        /// initialization and enter the initial configuration, or false to reject it.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        /// <summary>Registers a predicate that accepts or rejects initialization by a specific event type.</summary>
        /// <typeparam name="TInitializationEvent">Initialization event payload type passed to the initializer through its
        /// context.</typeparam>
        /// <typeparam name="TInitializer">Typed initialization predicate implementing
        /// <see cref="IInitializer&lt;TInitializationEvent&gt;" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(async c => await (await ((BaseContext)c).Context.Executor.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent));

        /// <summary>Registers a handler executed when the State Machine finalizes.</summary>
        /// <param name="actionAsync">Asynchronous handler receiving the machine action context during finalization.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);

        /// <summary>Registers a handler executed when the State Machine finalizes.</summary>
        /// <typeparam name="TFinalizer">Typed machine-finalization handler implementing <see cref="IFinalizer" />.
        /// </typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(async c => await (await ((BaseContext)c).Context.Executor.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync());
    }
}
