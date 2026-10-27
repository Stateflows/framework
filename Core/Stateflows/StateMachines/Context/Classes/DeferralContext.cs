using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class DeferralContext<TEvent> : StateActionContext, IDeferralContext<TEvent>
    {
        public DeferralContext(RootContext context, Vertex vertex) : base(context, vertex, Constants.Deferral)
        { }

        public TEvent Event => new EventContext<TEvent>(Context).Event;
        public Guid EventId => Context.EventHolder.Id;
        public IDictionary<string, EventHeader> EventHeaders => Context.EventHolder.Headers;
    }
}
