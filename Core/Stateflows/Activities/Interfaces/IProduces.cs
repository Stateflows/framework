﻿namespace Stateflows.Activities
{
#pragma warning disable S2326 // Unused type parameters should be removed
    public interface IProduces<TToken> : INodeInterface
        where TToken : Token, new()
    { }
#pragma warning restore S2326 // Unused type parameters should be removed
}
