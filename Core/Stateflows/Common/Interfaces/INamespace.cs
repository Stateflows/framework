using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface INamespace
    {
        IValue<T> GetValue<T>(string key);
        
        INamespace GetNamespace(string namespaceName);

        Task ClearAsync();
    }
}
