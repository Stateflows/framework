using System;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityContext : BaseContext, IActivityInspectionContext
    {
        public ActivityId Id => Context.Id;

        public ActivityContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            Values = new ContextValues(Context.GlobalValues);
        }

        public IActivityInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => _ = Context.Send(@event);
    }
}
