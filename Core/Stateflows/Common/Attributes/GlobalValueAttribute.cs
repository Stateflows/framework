using System;
#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Injects a parameter from the global Stateflows value store.</summary>
    /// <param name="name">Optional value key; when omitted, the parameter name is used.</param>
    /// <param name="required">Whether resolving the value is required.</param>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GlobalValueAttribute(string? name = null, bool required = true) :
        ValueAttribute(name, required);

    /// <summary>Alias for <see cref="GlobalValueAttribute"/> used on injected parameters.</summary>
    /// <param name="name">Optional value key; when omitted, the parameter name is used.</param>
    /// <param name="required">Whether resolving the value is required.</param>
    public sealed class FromGlobalValueAttribute(string? name = null, bool required = true) : GlobalValueAttribute(name, required);
}
