using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class StructuredActivity : ActivityNode
    {
        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;
    }
}
