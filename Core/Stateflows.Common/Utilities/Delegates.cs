using System.Threading.Tasks;

namespace Stateflows.Common.Utilities
{
    public delegate Task ActionAsync();

    public delegate Task ActionAsync<T>(T param);

    public delegate Task<bool> PredicateAsync();

    public delegate Task<bool> PredicateAsync<T>(T param);
}
