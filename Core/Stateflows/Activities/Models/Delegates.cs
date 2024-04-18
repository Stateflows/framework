using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Models
{
    internal delegate Task<object> TokenPipelineActionAsync(TokenPipelineContext context);

    internal delegate Task ActivityActionAsync(IActionContext context);

    internal delegate Task<bool> ActivityPredicateAsync(BaseContext context);

    internal delegate Task ActivityEventActionAsync(BaseContext context);

    internal delegate Task<bool> GuardAsync<TToken>(BaseContext context);

    internal delegate Task<TTarget> TransformAsync<TSource, TTarget>(BaseContext context);
}