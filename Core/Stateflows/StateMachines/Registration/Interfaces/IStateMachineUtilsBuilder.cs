using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines;

/// <summary>Configures machine-local middleware and utilities without exposing structural definition operations.
/// </summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
public interface IStateMachineUtilsBuilder : IStateMachineUtils<IStateMachineUtilsBuilder>;