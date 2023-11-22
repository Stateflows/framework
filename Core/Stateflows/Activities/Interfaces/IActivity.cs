using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Interfaces
{
    public interface IActivity : IBehavior
    {
        Task<IEnumerable<Token>> Execute(IEnumerable<Token> input);

        Task<T> Execute<T>(IDictionary<string, object> parameters);

        Task Execute(IDictionary<string, object> parameters);

        Task<SendResult> Cancel();
    }
}
