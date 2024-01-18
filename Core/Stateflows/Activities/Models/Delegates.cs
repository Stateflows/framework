using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Models
{
    internal delegate Task<Token> TokenPipelineActionAsync(TokenPipelineContext context);

    internal delegate Task ActivityActionAsync(IActionContext context);

    internal delegate Task<bool> ActivityPredicateAsync(BaseContext context);

    internal delegate Task ActivityEventActionAsync(BaseContext context);

    internal delegate Task<bool> GuardAsync<TToken>(BaseContext context)
        where TToken : Token, new();

    internal delegate Task<TTarget> TransformAsync<TSource, TTarget>(BaseContext context)
        where TSource : Token
        where TTarget : Token;
}