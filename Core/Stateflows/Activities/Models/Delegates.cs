using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Models
{
    internal delegate Task<TokenHolder> TokenPipelineActionAsync(TokenPipelineContext context);

    internal delegate Task ActivityActionAsync(Context.Interfaces.IActionContext context);

    internal delegate Task<bool> ActivityPredicateAsync(BaseContext context);

    internal delegate Task ActivityEventActionAsync(BaseContext context);

    internal delegate Task<bool> GuardAsync<TToken>(BaseContext context);

    internal delegate Task<TTarget> TransformAsync<TSource, TTarget>(BaseContext context);
}