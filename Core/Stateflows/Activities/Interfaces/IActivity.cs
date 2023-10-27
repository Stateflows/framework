using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Interfaces
{
    public interface IActivity : IBehavior
    {
        Task<IEnumerable<Token>> Execute(IEnumerable<Token> input);

        Task<T> Execute<T>(IDictionary<string, object> parameters);

        Task Execute(IDictionary<string, object> parameters);

        Task Cancel();
    }
}
