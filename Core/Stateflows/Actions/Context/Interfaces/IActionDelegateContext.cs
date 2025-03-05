using System;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Actions.Context.Interfaces
{
    public interface IActionDelegateContext : IBehaviorLocator, IExecutionContext, IInput, IOutput
    {
        /// <summary>
        /// Information about action behavior
        /// </summary>
        [Obsolete("Activity context property is obsolete, use Behavior or LockHandle properties instead.")]
        IActionContext Action { get; }
        
        /// <summary>
        /// Information about current behavior
        /// </summary>
        IBehaviorContext Behavior => Action;
    }
}
