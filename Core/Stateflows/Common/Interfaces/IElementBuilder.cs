namespace Stateflows.Common.Interfaces;

/// <summary>Provides a fluent configuration operation for an abstract model element.</summary>
/// <typeparam name="TElement">The model element being configured.</typeparam>
/// <typeparam name="TReturn">The builder type returned to support fluent configuration.</typeparam>
public interface IElementBuilderBase<out TElement, out TReturn>
    where TElement : class, IAbstractElement
{
    /// <summary>Applies a configuration callback to the element.</summary>
    /// <param name="action">The callback that configures the element.</param>
    /// <returns>The builder returned for continued configuration.</returns>
    TReturn Configure(System.Action<TElement> action);
}

/// <summary>Configures an abstract model element with a builder that returns itself.</summary>
/// <typeparam name="TElement">The model element being configured.</typeparam>
public interface IElementBuilder<out TElement> : IElementBuilderBase<TElement, IElementBuilder<TElement>>
    where TElement : class, IAbstractElement;

/// <summary>Provides extension methods for configuring model elements.</summary>
public static class ElementBuilderExtensions
{
    /// <summary>Assigns a configuration object to a configurable element.</summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TConfiguration">The configuration object type.</typeparam>
    /// <param name="builder">The builder receiving the configuration.</param>
    /// <param name="configuration">The configuration object to assign.</param>
    /// <returns>The same builder for fluent configuration.</returns>
    public static IElementBuilder<TElement> AddConfiguration<TElement, TConfiguration>(this IElementBuilder<TElement> builder, TConfiguration configuration)
        where TElement : class, IAbstractElement, IConfigurable<TConfiguration>
        => builder.Configure(e => e.Configuration = configuration);
    
    /// <summary>Assigns a configuration object and returns the builder's configured result type.</summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TConfiguration">The configuration object type.</typeparam>
    /// <typeparam name="TReturn">The builder result type.</typeparam>
    /// <param name="builder">The builder receiving the configuration.</param>
    /// <param name="configuration">The configuration object to assign.</param>
    /// <returns>The value returned by the builder's configuration operation.</returns>
    public static TReturn AddConfiguration<TElement, TConfiguration, TReturn>(this IElementBuilderBase<TElement, TReturn> builder, TConfiguration configuration)
        where TElement : class, IAbstractElement, IConfigurable<TConfiguration>
        => builder.Configure(e => e.Configuration = configuration);
}