using Stateflows.Activities.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class BaseActionNode : ActivityNode
    { }

    public abstract class ActionNode : BaseActionNode, IActionNode
    {
        public abstract Task ExecuteAsync();
    }
}
