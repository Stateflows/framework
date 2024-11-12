namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateComposition<out TReturn>
    {
        TReturn MakeComposite(CompositeStateBuildAction compositeStateBuildAction);
    }
}
