namespace Stateflows.Common.Interfaces;

public interface IElementBuilderBase<out TElement, out TReturn>
    where TElement : class, IAbstractElement
{
    TReturn Configure(System.Action<TElement> action);
}

public interface IElementBuilder<out TElement> : IElementBuilderBase<TElement, IElementBuilder<TElement>>
    where TElement : class, IAbstractElement;

public static class ElementBuilderExtensions
{
    public static IElementBuilder<TElement> AddConfiguration<TElement, TConfiguration>(this IElementBuilder<TElement> builder, TConfiguration configuration)
        where TElement : class, IAbstractElement, IConfigurable<TConfiguration>
        => builder.Configure(e => e.Configuration = configuration);
    
    public static TReturn AddConfiguration<TElement, TConfiguration, TReturn>(this IElementBuilderBase<TElement, TReturn> builder, TConfiguration configuration)
        where TElement : class, IAbstractElement, IConfigurable<TConfiguration>
        => builder.Configure(e => e.Configuration = configuration);
}