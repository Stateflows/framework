using System.Diagnostics;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateTypedEvents<out TReturn> : ICompositeStateEvents<TReturn>
    {
        #region AddOnInitialize
        /// <summary>
        /// Adds multiple typed initialization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateInitialization">The type of the state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitialize<TCompositeStateInitialization>()
            where TCompositeStateInitialization : class, ICompositeStateInitialization
            => AddOnInitialize(async c => await (await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeStateInitialization>(c)).OnInitializeAsync());

        /// <summary>
        /// Adds multiple typed initialization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">The type of the second state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TCompositeStateInitialization1>();
            return AddOnInitialize<TCompositeStateInitialization2>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">The type of the third state initialization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2, TCompositeStateInitialization3>()
            where TCompositeStateInitialization1 : class, ICompositeStateInitialization
            where TCompositeStateInitialization2 : class, ICompositeStateInitialization
            where TCompositeStateInitialization3 : class, ICompositeStateInitialization
        {
            AddOnInitializes<TCompositeStateInitialization1, TCompositeStateInitialization2>();
            return AddOnInitialize<TCompositeStateInitialization3>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization4">The type of the fourth state initialization handler.</typeparam>
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

        /// <summary>
        /// Adds multiple typed initialization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization4">The type of the fourth state initialization handler.</typeparam>
        /// <typeparam name="TCompositeStateInitialization5">The type of the fifth state initialization handler.</typeparam>
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
        /// <summary>
        /// Adds multiple typed finalization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateFinalization">The type of the state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalize<TCompositeStateFinalization>()
            where TCompositeStateFinalization : class, ICompositeStateFinalization
            => AddOnFinalize(async c => await (await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeStateFinalization>(c)).OnFinalizeAsync());

        /// <summary>
        /// Adds multiple typed finalization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">The type of the second state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TCompositeStateFinalization1>();
            return AddOnFinalize<TCompositeStateFinalization2>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">The type of the third state finalization handler.</typeparam>
        [DebuggerHidden]
        TReturn AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2, TCompositeStateFinalization3>()
            where TCompositeStateFinalization1 : class, ICompositeStateFinalization
            where TCompositeStateFinalization2 : class, ICompositeStateFinalization
            where TCompositeStateFinalization3 : class, ICompositeStateFinalization
        {
            AddOnFinalizes<TCompositeStateFinalization1, TCompositeStateFinalization2>();
            return AddOnFinalize<TCompositeStateFinalization3>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization4">The type of the fourth state finalization handler.</typeparam>
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

        /// <summary>
        /// Adds multiple typed finalization handlers to the current composite state.
        /// </summary>
        /// <typeparam name="TCompositeStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization4">The type of the fourth state finalization handler.</typeparam>
        /// <typeparam name="TCompositeStateFinalization5">The type of the fifth state finalization handler.</typeparam>
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
