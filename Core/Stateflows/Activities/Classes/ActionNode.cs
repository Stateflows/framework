using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class ActionNode : ActivityNode
    {
        public abstract Task ExecuteAsync();
    }
}
