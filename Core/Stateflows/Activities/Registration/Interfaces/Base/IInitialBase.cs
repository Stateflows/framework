namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IInitialBase<out TReturn>
    {
        TReturn AddInitial(InitialBuildAction buildAction);
    }
}
