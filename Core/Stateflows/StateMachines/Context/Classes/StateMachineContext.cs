using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineInspectionContext
    {
        public StateMachineId Id => Context.Id;

        public StateMachineContext(RootContext context) : base(context)
        {
            Values = new ContextValues(Context.GlobalValues);
        }

        public IStateMachineInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)
            where TEvent : Event
            => _ = Context.Send(@event);
    }
}
