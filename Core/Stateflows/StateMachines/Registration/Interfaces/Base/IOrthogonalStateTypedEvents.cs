using System.Diagnostics;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IOrthogonalStateTypedEvents<out TReturn> : ICompositeStateEvents<TReturn>
    {
        #region AddOnInitialize
        /// <summary>
        /// Adds multiple typed initialization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateInitialization">The type of the state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitialize<TOrthogonalStateInitialization>()
            where TOrthogonalStateInitialization : class, IOrthogonalStateInitialization
            => AddOnInitialize(c => ((BaseContext)c).Context.Executor.GetState<TOrthogonalStateInitialization>(c)?.OnInitializeAsync());

        /// <summary>
        /// Adds multiple typed initialization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">The type of the second state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
        {
            AddOnInitialize<TOrthogonalStateInitialization1>();
            return AddOnInitialize<TOrthogonalStateInitialization2>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">The type of the third state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2, TOrthogonalStateInitialization3>()
            where TOrthogonalStateInitialization1 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization2 : class, IOrthogonalStateInitialization
            where TOrthogonalStateInitialization3 : class, IOrthogonalStateInitialization
        {
            AddOnInitializes<TOrthogonalStateInitialization1, TOrthogonalStateInitialization2>();
            return AddOnInitialize<TOrthogonalStateInitialization3>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization4">The type of the fourth state initialization handler.</typeparam>
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

        /// <summary>
        /// Adds multiple typed initialization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization4">The type of the fourth state initialization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateInitialization5">The type of the fifth state initialization handler.</typeparam>
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
        /// <summary>
        /// Adds multiple typed finalization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateFinalization">The type of the state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalize<TOrthogonalStateFinalization>()
            where TOrthogonalStateFinalization : class, IOrthogonalStateFinalization
            => AddOnFinalize(c => ((BaseContext)c).Context.Executor.GetState<TOrthogonalStateFinalization>(c)?.OnFinalizeAsync());

        /// <summary>
        /// Adds multiple typed finalization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">The type of the second state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
        {
            AddOnFinalize<TOrthogonalStateFinalization1>();
            return AddOnFinalize<TOrthogonalStateFinalization2>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">The type of the third state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2, TOrthogonalStateFinalization3>()
            where TOrthogonalStateFinalization1 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization2 : class, IOrthogonalStateFinalization
            where TOrthogonalStateFinalization3 : class, IOrthogonalStateFinalization
        {
            AddOnFinalizes<TOrthogonalStateFinalization1, TOrthogonalStateFinalization2>();
            return AddOnFinalize<TOrthogonalStateFinalization3>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization4">The type of the fourth state finalization handler.</typeparam>
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

        /// <summary>
        /// Adds multiple typed finalization handlers to the current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization4">The type of the fourth state finalization handler.</typeparam>
        /// <typeparam name="TOrthogonalStateFinalization5">The type of the fifth state finalization handler.</typeparam>
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
