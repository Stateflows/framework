namespace Stateflows.Common.Interfaces;

public delegate void ElementBuildAction<in TElement>(IElementBuilder<TElement> builder)
    where TElement : class, IAbstractElement;