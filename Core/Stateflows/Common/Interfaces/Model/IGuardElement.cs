using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common;

public interface IGuardElement : IStateMachineGuard, IActivityGuard, IInitializer<object>, IDefaultInitializer
{
    Task<bool> IInitializer<object>.OnInitializeAsync(object initializationEvent)
        => GuardAsync();

    Task<bool> IDefaultInitializer.OnInitializeAsync()
        => GuardAsync();
}

public interface IGuardElement<in TEvent> : IStateMachineGuard<TEvent>, IActivityGuard<TEvent>, IInitializer<TEvent>
{
    Task<bool> IInitializer<TEvent>.OnInitializeAsync(TEvent initializationEvent)
        => GuardAsync(initializationEvent);
}