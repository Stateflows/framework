namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IInput<out TReturn>
    {
        TReturn AddInput(InputBuildAction buildAction);
    }
}
