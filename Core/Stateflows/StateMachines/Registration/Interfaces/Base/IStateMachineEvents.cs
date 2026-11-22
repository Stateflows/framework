using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Registers machine initialization acceptance predicates and finalization callbacks.</summary>
    /// <typeparam name="TReturn">The machine builder returned after registration.</typeparam>
    /// <remarks>Initializers return true to accept initialization; false prevents entry into the initial configuration.
    /// Default initializers support initialization without a typed request. Finalizers run when the machine finalizes.
    /// </remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
    public interface IStateMachineEvents<out TReturn>
    {
        /// <summary>Registers a predicate that accepts or rejects default State Machine initialization.</summary>
        /// <param name="actionAsync">Asynchronous predicate receiving the initialization context. Return true to accept
        /// initialization and enter the initial configuration, or false to reject it.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);

        /// <summary>Registers a predicate that accepts or rejects default State Machine initialization.</summary>
        /// <typeparam name="TDefaultInitializer">Typed default-initialization predicate implementing
        /// <see cref="IDefaultInitializer" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        [DebuggerHidden]
        public TReturn AddDefaultInitializer<TDefaultInitializer>()
            where TDefaultInitializer : class, IDefaultInitializer
        {
            var result = AddDefaultInitializer(async c => await (await ((BaseContext)c).Context.Executor.GetDefaultInitializerAsync<TDefaultInitializer>(c)).OnInitializeAsync());
            var logic = ((IGraphBuilder)this).Graph.DefaultInitializer;
            logic.Registrations[logic.Actions.Count - 1] = OperationRegistration.Leaf(typeof(TDefaultInitializer));
            return result;
        }

        /// <summary>Registers a predicate that accepts or rejects initialization by a specific event type.</summary>
        /// <typeparam name="TInitializationEvent">Initialization event payload type passed to the initializer through its
        /// context.</typeparam>
        /// <param name="actionAsync">Asynchronous predicate receiving the initialization context. Return true to accept
        /// initialization and enter the initial configuration, or false to reject it.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        /// <summary>Registers a predicate that accepts or rejects initialization by a specific event type.</summary>
        /// <typeparam name="TInitializationEvent">Initialization event payload type passed to the initializer through its
        /// context.</typeparam>
        /// <typeparam name="TInitializer">Typed initialization predicate implementing
        /// <see cref="IInitializer&lt;TInitializationEvent&gt;" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        [DebuggerHidden]
        public TReturn AddInitializer<TInitializationEvent, TInitializer>()
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            var result = AddInitializer<TInitializationEvent>(async c => await (await ((BaseContext)c).Context.Executor.GetInitializerAsync<TInitializer, TInitializationEvent>(c)).OnInitializeAsync(c.InitializationEvent));
            var logic = ((IGraphBuilder)this).Graph.InitializerDeclarations[typeof(TInitializationEvent)];
            logic.Registrations[logic.Actions.Count - 1] = OperationRegistration.Leaf(typeof(TInitializer));
            return result;
        }

        /// <summary>Registers a predicate that accepts or rejects initialization by any of the given payload events.</summary>
        /// <param name="eventNames">Names of registered payloads accepted as initialization events.</param>
        /// <param name="actionAsync">Asynchronous predicate receiving the initialization context; its
        /// <c>InitializationEvent</c> is the received <see cref="Payload" />. Return true to accept initialization.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        public TReturn AddPayloadInitializer(IReadOnlyList<string> eventNames, Func<IStateMachineInitializationContext<Payload>, Task<bool>> actionAsync)
        {
            StateMachineBuilder.AddPayloadInitializer(((IGraphBuilder)this).Graph, eventNames, actionAsync);
            return (TReturn)this;
        }

        /// <summary>Registers an initializer class for any of the given payload events.</summary>
        /// <typeparam name="TInitializer">Initialization predicate implementing <see cref="IInitializer&lt;Payload&gt;" />.</typeparam>
        /// <param name="eventNames">Names of registered payloads accepted as initialization events.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        public TReturn AddPayloadInitializer<TInitializer>(IReadOnlyList<string> eventNames)
            where TInitializer : class, IInitializer<Payload>
        {
            var graph = ((IGraphBuilder)this).Graph;
            StateMachineBuilder.AddPayloadInitializer(graph, eventNames,
                async c => await (await ((BaseContext)c).Context.Executor.GetInitializerAsync<TInitializer, Payload>(c)).OnInitializeAsync(c.InitializationEvent));
            var logic = graph.PayloadInitializerDeclarations[PayloadNameSet.Key(eventNames)].Logic;
            logic.Registrations[logic.Actions.Count - 1] = OperationRegistration.Leaf(typeof(TInitializer));
            return (TReturn)this;
        }

        /// <summary>Registers a handler executed when the State Machine finalizes.</summary>
        /// <param name="actionAsync">Asynchronous handler receiving the machine action context during finalization.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);

        /// <summary>Registers a handler executed when the State Machine finalizes.</summary>
        /// <typeparam name="TFinalizer">Typed machine-finalization handler implementing <see cref="IFinalizer" />.
        /// </typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
        [DebuggerHidden]
        public TReturn AddFinalizer<TFinalizer>()
            where TFinalizer : class, IFinalizer
        {
            var result = AddFinalizer(async c => await (await ((BaseContext)c).Context.Executor.GetFinalizerAsync<TFinalizer>(c)).OnFinalizeAsync());
            var logic = ((IGraphBuilder)this).Graph.Finalize;
            logic.Registrations[logic.Actions.Count - 1] = OperationRegistration.Leaf(typeof(TFinalizer));
            return result;
        }
    }
}
