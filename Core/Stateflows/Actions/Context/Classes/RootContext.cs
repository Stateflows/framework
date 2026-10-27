using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Engine;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context.Classes
{
    internal class RootContext
    {
        public BehaviorId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }

        public RootContext(StateflowsContext context, Executor executor, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            Context = context;
            Executor = executor;
            EventHolder = eventHolder;
            ServiceProvider = serviceProvider;
            Id = Context.Id;
        }

        public Executor Executor { get; set; }
        
        public EventHolder EventHolder { get; set; }

        public readonly List<Exception> Exceptions = [];

        public async Task Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Context.ContextParentId ?? Id, out var behavior))
            {
                await behavior.SendAsync(@event, headers);
            }
        }
    }
}
