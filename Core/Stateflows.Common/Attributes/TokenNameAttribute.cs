using System;
using System.Collections.Generic;

namespace Stateflows.Common.Attributes;

/// <summary>Declares the payload token names supplied to a model-element constructor parameter.</summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TokenNameAttribute(params string[] names) : Attribute
{
    /// <summary>Gets the payload token names selected by this parameter.</summary>
    public IReadOnlyList<string> Names { get; } = names ?? throw new ArgumentNullException(nameof(names));
}
