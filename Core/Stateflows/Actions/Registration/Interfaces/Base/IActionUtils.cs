namespace Stateflows.Actions.Registration.Interfaces.Base;

public interface IActionUtils<out TReturn>
{
    TReturn SetResourceName(string resourceName);
    TReturn SetIsStateless(bool isStateless);
}
