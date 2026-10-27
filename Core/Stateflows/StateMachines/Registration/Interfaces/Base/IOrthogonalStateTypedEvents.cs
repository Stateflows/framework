using System.Diagnostics;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Registers typed lifecycle implementations for the enclosing orthogonal state.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public interface IOrthogonalStateTypedEvents<out TReturn> : ICompositeStateEvents<TReturn>
    {
        #region AddOnInitialize
        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TOrthogonalStateInitialization1">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
        {
            AddOnInitialize<TOrthogonalStateInitialization1>();
            return AddOnInitialize<TOrthogonalStateInitialization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TOrthogonalStateInitialization1">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization3 : class, IOrthogonalStateInitialization
        {
            AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2>();
            return AddOnInitialize<TOrthogonalStateInitialization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TOrthogonalStateInitialization1">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization4">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3, TOrthogonalStateInitialization4>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization3 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization4 : class, IOrthogonalStateInitialization
        {
            AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3>();
            return AddOnInitialize<TOrthogonalStateInitialization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TOrthogonalStateInitialization1">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization4">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization5">Typed orthogonal-state initialization handler implementing
        /// <see cref="IOrthogonalStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3, TOrthogonalStateInitialization4, TOrthogonalStateInitialization5>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization3 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization4 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization5 : class, IOrthogonalStateInitialization
        {
            AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3, TOrthogonalStateInitialization4>();
            return AddOnInitialize<TOrthogonalStateInitialization5>();
        }
        #endregion

        #region AddOnFinalize
        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TOrthogonalStateFinalization1">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
        {
            AddOnFinalize<TOrthogonalStateFinalization1>();
            return AddOnFinalize<TOrthogonalStateFinalization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TOrthogonalStateFinalization1">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization3 : class, IOrthogonalStateFinalization
        {
            AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2>();
            return AddOnFinalize<TOrthogonalStateFinalization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TOrthogonalStateFinalization1">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization4">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3, TOrthogonalStateFinalization4>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization3 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization4 : class, IOrthogonalStateFinalization
        {
            AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3>();
            return AddOnFinalize<TOrthogonalStateFinalization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TOrthogonalStateFinalization1">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization4">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization5">Typed orthogonal-state finalization handler implementing
        /// <see cref="IOrthogonalStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3, TOrthogonalStateFinalization4, TOrthogonalStateFinalization5>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization3 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization4 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization5 : class, IOrthogonalStateFinalization
        {
            AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3, TOrthogonalStateFinalization4>();
            return AddOnFinalize<TOrthogonalStateFinalization5>();
        }
        #endregion
    }
}
