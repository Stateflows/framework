using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEvents<out TReturn>
    {
        /// <summary>
        /// Adds initialization handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionAsync">Action handler</param>
        TReturn AddOnInitialize(Func<IStateActionContext, Task> actionAsync);

        /// <summary>
        /// Adds finalization handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionAsync">Action handler</param>
        TReturn AddOnFinalize(Func<IStateActionContext, Task> actionAsync);
    }
}
