namespace Stateflows.Common;

/// <summary>An element that accepts a configuration object during model registration.</summary>
/// <typeparam name="TConfiguration">The configuration object type.</typeparam>
public interface IConfigurable<in TConfiguration> : IAbstractElement
{
    /// <summary>Sets the configuration object used by this element.</summary>
    public TConfiguration Configuration { set; }
}