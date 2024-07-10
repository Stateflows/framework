using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Interfaces;

namespace Stateflows.Activities
{
    public abstract class StructuredActivityNode : ActivityNode, IStructuredActivityNode
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
