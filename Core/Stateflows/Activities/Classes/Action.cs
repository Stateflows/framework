using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class Action : ActivityNode
    {
        public abstract Task ExecuteAsync();
    }
}
