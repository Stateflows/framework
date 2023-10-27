namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IFinal<out TReturn>
    {
        TReturn AddFinal();
    }
}
