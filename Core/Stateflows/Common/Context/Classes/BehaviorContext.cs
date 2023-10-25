using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Context.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorContext : BaseContext, IBehaviorContext
    {
        public BehaviorId Id => Context.Id;

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            GlobalValues = new ContextValues(Context.GlobalValues);
        }

        public IContextValues GlobalValues { get; }

        public void Send<TEvent>(TEvent @event) where TEvent : Event, new()
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(Id, out var behavior))
            {
                _ = behavior.SendAsync(@event);
            }
        }
    }
}
