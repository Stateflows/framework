using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

public interface IAbstractGuard<in TInput> : IAbstractElement
{
    Task<bool> GuardAsync(TInput input);
}
    
public interface IAbstractGuard : IAbstractGuard<object>
{
    Task<bool> GuardAsync();
        
    Task<bool> IAbstractGuard<object>.GuardAsync(object input)
        => GuardAsync();
}