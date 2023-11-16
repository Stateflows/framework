﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Models
{
    internal delegate Task<IEnumerable<Token>> PipelineActionAsync(IPipelineContext context);

    internal delegate Task ActivityActionAsync(IActionContext context);

    internal delegate Task ActivityEventActionAsync(BaseContext context);

    internal delegate Task<bool> GuardAsync<TToken>(BaseContext context)
        where TToken : Token, new();

    internal delegate Task<TTarget> TransformAsync<TSource, TTarget>(BaseContext context)
        where TSource : Token
        where TTarget : Token;
}