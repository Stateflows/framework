﻿using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class StructuredActivityNode : ActivityNode
    {
        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;
    }

    public abstract class StructuredActivityNode<TToken> : StructuredActivityNode
    {
        public new IActionContext<TToken> Context => base.Context as IActionContext<TToken>;
    }
}
