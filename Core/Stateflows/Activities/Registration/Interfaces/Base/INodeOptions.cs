namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface INodeOptions<out TReturn>
    {
        TReturn SetOptions(NodeOptions nodeOptions);
    }
}
