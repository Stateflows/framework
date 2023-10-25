using System.Threading.Tasks;

namespace Stateflows.Common.Utilities
{
    public delegate Task ActionAsync();

    public delegate Task ActionAsync<in T>(T param);

    public delegate Task<bool> PredicateAsync();

    public delegate Task<bool> PredicateAsync<in T>(T param);
}
