using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;

namespace Stateflows.Activities.Extensions
{
    public delegate Event StateActionActivityInitializationBuilder(IStateActionContext context);

    public delegate Event GuardActivityInitializationBuilder<in TEvent>(IGuardContext<TEvent> context)
        where TEvent : Event, new();

    public delegate Event EffectActivityInitializationBuilder<in TEvent>(IEventActionContext<TEvent> context)
        where TEvent : Event, new();

    public delegate void IntegratedActivityBuildAction(IIntegratedActivityBuilder builder);
}