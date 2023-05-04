using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateSubmachineBuilderBase<TReturn>
    {
        #region Submachine
        TReturn AddSubmachine(string submachineName, Dictionary<string, object> submachineInitialValues = null);
        #endregion
    }
}
