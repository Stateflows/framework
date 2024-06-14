using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class BaseActionNode : ActivityNode
    {
        public abstract Task ExecuteAsync();
    }

    public abstract class ActionNode : BaseActionNode
    { }
}
