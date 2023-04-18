using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineContext, IStateMachineInspectionContext
    {
        public StateMachineId Id => Context.Id;

        public StateMachineContext(RootContext context) : base(context)
        {
            GlobalValues = new ContextValues(Context.GlobalValues);
        }

        public IStateMachineInspection Inspection => Context.Executor.Observer.Inspection;

        public IContextValues GlobalValues { get; }

        public void Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => _ = Context.Send(@event);
    }
}
