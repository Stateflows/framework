using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

public interface IAbstractGuard<in TEvent> : IAbstractElement
{
    Task<bool> GuardAsync(TEvent @event);
}
    
public interface IAbstractGuard : IAbstractGuard<object>
{
    Task<bool> GuardAsync();
        
    Task<bool> IAbstractGuard<object>.GuardAsync(object @event)
        => GuardAsync();
}