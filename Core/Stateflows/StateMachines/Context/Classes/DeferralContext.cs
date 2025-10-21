using System;
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

        public TEvent Event => ((EventHolder<TEvent>)Context.EventHolder).Payload;
        public Guid EventId => ((EventHolder<TEvent>)Context.EventHolder).Id;
    }
}
