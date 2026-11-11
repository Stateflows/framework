using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities;

/// <summary>Configures activity-wide utility and observability settings independently of graph elements.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/4.5.-Observability" />
public interface IActivityUtilsBuilder :
    IActivityUtils<IActivityUtilsBuilder>;
