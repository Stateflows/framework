using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEvents<out TReturn> :
        ICompositeStateInitialization<TReturn>,
        ICompositeStateFinalization<TReturn>
    {
        // #region AddOnInitialize
        // /// <summary>
        // /// Adds initialization handler to current state.<br/>
        // /// Use the following pattern to implement handler:
        // /// <code>async c => {
        // ///     // handler logic here; action context is available via c parameter
        // /// }</code>
        // /// </summary>
        // /// <param name="actionAsync">Action handler</param>
        // TReturn AddOnInitialize(Func<IStateActionContext, Task> actionAsync);
        //
        // /// <summary>
        // /// Adds synchronous initialization handler to current composite state.<br/>
        // /// Use the following pattern to implement handler:
        // /// <code>c => {
        // ///     // handler logic here; action context is available via c parameter
        // /// }</code>
        // /// </summary>
        // /// <param name="action">Synchronous action handler</param>
        // [DebuggerHidden]
        // public TReturn AddOnInitialize(Action<IStateActionContext> action)
        //     => AddOnInitialize(action
        //         .AddStateMachineInvocationContext(((IGraphBuilder)this).Graph)
        //         .ToAsync()
        //     );
        // #endregion
        //
        // #region AddOnFinalize
        // /// <summary>
        // /// Adds finalization handler to current state.<br/>
        // /// Use the following pattern to implement handler:
        // /// <code>async c => {
        // ///     // handler logic here; action context is available via c parameter
        // /// }</code>
        // /// </summary>
        // /// <param name="actionAsync">Action handler</param>
        // TReturn AddOnFinalize(Func<IStateActionContext, Task> actionAsync);
        //
        // /// <summary>
        // /// Adds synchronous finalization handler to current composite state.<br/>
        // /// Use the following pattern to implement handler:
        // /// <code>c => {
        // ///     // handler logic here; action context is available via c parameter
        // /// }</code>
        // /// </summary>
        // /// <param name="action">Synchronous action handler</param>
        // [DebuggerHidden]
        // public TReturn AddOnFinalize(Action<IStateActionContext> action)
        //     => AddOnFinalize(action
        //         .AddStateMachineInvocationContext(((IGraphBuilder)this).Graph)
        //         .ToAsync()
        //     );
        // #endregion
    }
}
