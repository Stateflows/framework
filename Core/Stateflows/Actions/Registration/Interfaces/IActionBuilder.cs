using Stateflows.Actions.Registration.Interfaces.Base;

namespace Stateflows.Actions.Registration.Interfaces;

public interface IActionBuilder : IActionUtils<IActionBuilder>, IActionObservability<IActionBuilder>;
