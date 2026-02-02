namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IOutputBase<out TReturn>
    {
        TReturn AddOutput();
    }
}
