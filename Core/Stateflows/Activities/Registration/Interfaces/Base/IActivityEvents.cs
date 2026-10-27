using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Registers activity initialization and finalization callbacks.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
    public interface IActivityEvents<out TReturn>
    {
        /// <summary>Adds a default initializer that may accept or reject initialization.</summary>
        /// <param name="actionAsync">Asynchronous initialization predicate; return true to accept initialization or false
        /// to reject it.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        TReturn AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync);
        
        /// <summary>Adds a default initializer implementation type.</summary>
        /// <typeparam name="TInitializer">Typed Activity lifecycle handler implementing <see cref="IDefaultInitializer" />.
        /// </typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TInitializer>()
            where TInitializer : class, IDefaultInitializer
            => AddDefaultInitializer(async c
                => await (await ((BaseContext)c).NodeScope.GetDefaultInitializerAsync<TInitializer>(c)).OnInitializeAsync()
            );

        /// <summary>Adds an initializer for a typed initialization request.</summary>
        /// <typeparam name="TInitializationEvent">Initialization request payload type supplied to the typed initializer.
        /// </typeparam>
        /// <param name="actionAsync">Asynchronous initialization predicate; return true to accept initialization or false
        /// to reject it.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        TReturn AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        /// <summary>Adds a typed initializer implementation for an initialization request.</summary>
        /// <typeparam name="TInitializationEvent">Initialization request payload type supplied to the typed initializer.
        /// </typeparam>
        /// <typeparam name="TInitializer">Typed Activity lifecycle handler implementing
        /// <see cref="IInitializer&lt;TInitializationEvent&gt;" />.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => AddInitializer<TInitializationEvent>(async c
                => await (await ((BaseContext)c).NodeScope.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent)
            );

        /// <summary>Adds an asynchronous activity finalizer.</summary>
        /// <param name="actionAsync">Asynchronous callback invoked when the Activity finalizes.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        TReturn AddFinalizer(Func<IActivityActionContext, Task> actionAsync);

        /// <summary>Adds a finalizer implementation type.</summary>
        /// <typeparam name="TFinalizer">Typed Activity lifecycle handler implementing <see cref="IFinalizer" />.
        /// </typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities" />
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => AddFinalizer(async c
                => await (await ((BaseContext)c).NodeScope.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync()
            );
    }
}
