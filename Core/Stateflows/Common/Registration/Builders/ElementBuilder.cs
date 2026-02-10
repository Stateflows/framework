using System;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Registration.Builders;

internal class ElementBuilder<TElement>(TElement element) : IElementBuilder<TElement>
    where TElement : class, IAbstractElement
{
    public IElementBuilder<TElement> Configure(Action<TElement> action)
    {
        action.Invoke(element);
        
        return this;
    }
}