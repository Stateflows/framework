using System;
using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Payloads;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces.Internal;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Models;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Registers activity initialization and finalization callbacks.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
    public interface IActivityEvents<out TReturn>
    {
        /// <summary>Adds a default initializer that may accept or reject initialization.</summary>
        /// <param name="actionAsync">Asynchronous initialization predicate; return true to accept initialization or false
        /// to reject it.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        TReturn AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync);
        
        /// <summary>Adds a default initializer implementation type.</summary>
        /// <typeparam name="TInitializer">Typed Activity lifecycle handler implementing <see cref="IDefaultInitializer" />.
        /// </typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TInitializer>()
            where TInitializer : class, IDefaultInitializer
            => RegistrationMetadata.Initializer(() => AddDefaultInitializer(async c
                => await (await ((BaseContext)c).NodeScope.GetDefaultInitializerAsync<TInitializer>(c)).OnInitializeAsync()
            ), typeof(TInitializer));

        /// <summary>Adds an initializer for a typed initialization request.</summary>
        /// <typeparam name="TInitializationEvent">Initialization request payload type supplied to the typed initializer.
        /// </typeparam>
        /// <param name="actionAsync">Asynchronous initialization predicate; return true to accept initialization or false
        /// to reject it.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        TReturn AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        /// <summary>Adds a typed initializer implementation for an initialization request.</summary>
        /// <typeparam name="TInitializationEvent">Initialization request payload type supplied to the typed initializer.
        /// </typeparam>
        /// <typeparam name="TInitializer">Typed Activity lifecycle handler implementing
        /// <see cref="IInitializer&lt;TInitializationEvent&gt;" />.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => RegistrationMetadata.Initializer(() => AddInitializer<TInitializationEvent>(async c
                => await (await ((BaseContext)c).NodeScope.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent)
            ), typeof(TInitializer), typeof(TInitializationEvent));

        /// <summary>Adds an initializer accepting any of the given payload events.</summary>
        /// <param name="eventNames">Names of registered payloads accepted as initialization events.</param>
        /// <param name="actionAsync">Asynchronous initialization predicate receiving the context; its
        /// <c>InitializationEvent</c> is the received <see cref="Payload" />. Return true to accept initialization.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        public TReturn AddPayloadInitializer(IReadOnlyList<string> eventNames, Func<IActivityInitializationContext<Payload>, Task<bool>> actionAsync)
        {
            ((ActivityBuilder)((IGraphBuilder)this)).AddPayloadInitializer(eventNames, actionAsync);
            return (TReturn)this;
        }

        /// <summary>Adds an initializer implementation accepting any of the given payload events.</summary>
        /// <typeparam name="TInitializer">Activity lifecycle handler implementing <see cref="IInitializer&lt;Payload&gt;" />.</typeparam>
        /// <param name="eventNames">Names of registered payloads accepted as initialization events.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        public TReturn AddPayloadInitializer<TInitializer>(IReadOnlyList<string> eventNames)
            where TInitializer : class, IInitializer<Payload>
        {
            AddPayloadInitializer(eventNames, async c
                => await (await ((BaseContext)c).NodeScope.GetInitializerAsync<TInitializer, Payload>(c)).OnInitializeAsync(c.InitializationEvent));
            var graph = ((IGraphBuilder)this).Graph;
            var key = PayloadNameSet.Key(eventNames);
            graph.InitializerImplementations[(key, graph.Initializers[key].Actions.Count - 1)] = typeof(TInitializer);
            return (TReturn)this;
        }

        /// <summary>Adds an asynchronous activity finalizer.</summary>
        /// <param name="actionAsync">Asynchronous callback invoked when the Activity finalizes.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        TReturn AddFinalizer(Func<IActivityActionContext, Task> actionAsync);

        /// <summary>Adds a finalizer implementation type.</summary>
        /// <typeparam name="TFinalizer">Typed Activity lifecycle handler implementing <see cref="IFinalizer" />.
        /// </typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.-Activities" />
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
            => RegistrationMetadata.Finalizer(() => AddFinalizer(async c
                => await (await ((BaseContext)c).NodeScope.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync()
            ), typeof(TFinalizer));
    }
}
