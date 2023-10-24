using System;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorActionContext : BaseContext, IBehaviorActionContext
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;

        public BehaviorActionContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        { }
    }
}
