using System.Diagnostics;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Connects typed composite-state lifecycle implementations to the current definition.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateTypedEvents<out TReturn> : ICompositeStateEvents<TReturn>
    {
        #region AddOnInitialize
        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TCompositeStateInitialization1">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TCompositeStateInitialization1>();
            return AddOnInitialize<TCompositeStateInitialization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TCompositeStateInitialization1">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
            where TCompositeStateInitialization3 : class, ICompositeStateInitialization
        {
            AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2>();
            return AddOnInitialize<TCompositeStateInitialization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TCompositeStateInitialization1">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization4">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3, TCompositeStateInitialization4>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
            where TCompositeStateInitialization3 : class, ICompositeStateInitialization
            where TCompositeStateInitialization4 : class, ICompositeStateInitialization
        {
            AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3>();
            return AddOnInitialize<TCompositeStateInitialization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TCompositeStateInitialization1">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization4">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TCompositeStateInitialization5">Typed composite-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3, TCompositeStateInitialization4, TCompositeStateInitialization5>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
            where TCompositeStateInitialization3 : class, ICompositeStateInitialization
            where TCompositeStateInitialization4 : class, ICompositeStateInitialization
            where TCompositeStateInitialization5 : class, ICompositeStateInitialization
        {
            AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3, TCompositeStateInitialization4>();
            return AddOnInitialize<TCompositeStateInitialization5>();
        }
        #endregion

        #region AddOnFinalize
        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TCompositeStateFinalization1">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TCompositeStateFinalization1>();
            return AddOnFinalize<TCompositeStateFinalization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TCompositeStateFinalization1">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
            where TCompositeStateFinalization3 : class, ICompositeStateFinalization
        {
            AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2>();
            return AddOnFinalize<TCompositeStateFinalization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TCompositeStateFinalization1">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization4">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3, TCompositeStateFinalization4>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
            where TCompositeStateFinalization3 : class, ICompositeStateFinalization
            where TCompositeStateFinalization4 : class, ICompositeStateFinalization
        {
            AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3>();
            return AddOnFinalize<TCompositeStateFinalization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TCompositeStateFinalization1">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization4">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TCompositeStateFinalization5">Typed composite-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3, TCompositeStateFinalization4, TCompositeStateFinalization5>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
            where TCompositeStateFinalization3 : class, ICompositeStateFinalization
            where TCompositeStateFinalization4 : class, ICompositeStateFinalization
            where TCompositeStateFinalization5 : class, ICompositeStateFinalization
        {
            AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3, TCompositeStateFinalization4>();
            return AddOnFinalize<TCompositeStateFinalization5>();
        }
        #endregion
    }
}
