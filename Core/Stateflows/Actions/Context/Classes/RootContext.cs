using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context.Classes
{
    public class RootContext
    {
        public ActionId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }

        public RootContext(StateflowsContext context, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            Context = context;
            EventHolder = eventHolder;
            ServiceProvider = serviceProvider;
            Id = new ActionId(Context.Id);
        }

        public Dictionary<string, string> GlobalValues => Context.GlobalValues;

        public EventHolder EventHolder { get; set; }

        public readonly List<Exception> Exceptions = new List<Exception>();

        public async Task Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Context.ContextOwnerId ?? Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event, headers);
            }
        }
    }
}
