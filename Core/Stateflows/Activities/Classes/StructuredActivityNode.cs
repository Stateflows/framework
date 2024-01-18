using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class StructuredActivityNode : ActivityNode
    {
        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;
    }

    public abstract class StructuredActivity<TToken> : StructuredActivityNode
        where TToken : Token, new()
    {
        public new IActionContext<TToken> Context => base.Context as IActionContext<TToken>;
    }
}
