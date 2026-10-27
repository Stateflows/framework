namespace Stateflows.Common.Interfaces;

/// <summary>Configures an abstract model element through its builder.</summary>
/// <typeparam name="TElement">The element type being configured.</typeparam>
/// <param name="builder">The builder used to configure the element.</param>
public delegate void ElementBuildAction<in TElement>(IElementBuilder<TElement> builder)
    where TElement : class, IAbstractElement;