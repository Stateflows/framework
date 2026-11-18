using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Interfaces;
using Stateflows.Payloads;

namespace Stateflows.Activities;

internal sealed class PayloadTokenCollection(IReadOnlyList<string> names, bool enumerateOutput = false) :
    IInputTokens,
    IInputToken,
    IOptionalInputTokens,
    IOptionalInputToken,
    IOutputTokens,
    IInputOutputTokens,
    IInputOutputToken,
    IOptionalInputOutputTokens,
    IOptionalInputOutputToken
{
    private IEnumerable<Payload> Input
        => InputTokens.Tokens
            .Where(holder => holder.BoxedPayload is Payload payload && names.Contains(payload.Name, StringComparer.Ordinal))
            .Select(holder => (Payload)holder.BoxedPayload);

    private List<Payload> Output
        => OutputTokens.Tokens
            .Where(holder => holder.BoxedPayload is Payload payload && names.Contains(payload.Name, StringComparer.Ordinal))
            .Select(holder => (Payload)holder.BoxedPayload)
            .ToList();

    public Payload Token => Input.First();
    public bool IsAvailable => Input.Any();
    public Payload TokenOrDefault => Input.FirstOrDefault();

    public void PassAllOn() => AddRange(Input);
    public void PassOn() => Add(Token);

    public int Count => Output.Count;
    public bool IsReadOnly => false;

    public void Add(Payload item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!names.Contains(item.Name, StringComparer.Ordinal))
        {
            throw new ArgumentException($"Payload token '{item.Name}' is not declared for this parameter.", nameof(item));
        }

        OutputTokens.Tokens.Add(new TokenHolder<Payload> { Payload = item });
    }

    public void AddRange(IEnumerable<Payload> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Clear()
        => OutputTokens.Tokens.RemoveAll(holder => holder.BoxedPayload is Payload payload &&
            names.Contains(payload.Name, StringComparer.Ordinal));

    public bool Contains(Payload item) => Output.Contains(item);
    public void CopyTo(Payload[] array, int arrayIndex) => Output.CopyTo(array, arrayIndex);

    public bool Remove(Payload item)
    {
        var index = OutputTokens.Tokens.FindIndex(holder => holder.BoxedPayload is Payload payload &&
            names.Contains(payload.Name, StringComparer.Ordinal) &&
            EqualityComparer<Payload>.Default.Equals(payload, item));
        if (index < 0)
        {
            return false;
        }

        OutputTokens.Tokens.RemoveAt(index);
        return true;
    }

    public IEnumerator<Payload> GetEnumerator()
        => (enumerateOutput ? Output : Input).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class PayloadTokenParameterResolver : IModelElementParameterResolver
{
    private static readonly HashSet<Type> SupportedInterfaces =
    [
        typeof(IInputTokens), typeof(IInputToken), typeof(IOptionalInputTokens), typeof(IOptionalInputToken),
        typeof(IOutputTokens), typeof(IInputOutputTokens), typeof(IInputOutputToken),
        typeof(IOptionalInputOutputTokens), typeof(IOptionalInputOutputToken)
    ];

    private static readonly HashSet<Type> SupportedGenericDefinitions =
    [
        typeof(IInputTokens<>), typeof(IInputToken<>), typeof(IOptionalInputTokens<>), typeof(IOptionalInputToken<>),
        typeof(IOutputTokens<>), typeof(IInputOutputTokens<>), typeof(IInputOutputToken<>),
        typeof(IOptionalInputOutputTokens<>), typeof(IOptionalInputOutputToken<>)
    ];

    public bool CanResolve(ParameterInfo parameter)
    {
        if (parameter.GetCustomAttribute<TokenNameAttribute>() != null ||
            SupportedInterfaces.Contains(parameter.ParameterType))
        {
            return true;
        }

        var isPayloadGenericCollection = parameter.ParameterType.IsGenericType &&
            SupportedGenericDefinitions.Contains(parameter.ParameterType.GetGenericTypeDefinition()) &&
            PayloadNameSet.IsPayloadType(parameter.ParameterType.GenericTypeArguments[0]);
        if (isPayloadGenericCollection && parameter.GetCustomAttribute<TokenNameAttribute>() == null &&
            parameter.Member.DeclaringType?.IsGenericType == true &&
            (parameter.Member.DeclaringType.GetGenericTypeDefinition() == typeof(OutputPinNode<>) ||
             parameter.Member.DeclaringType.GetGenericTypeDefinition() == typeof(AcceptAndOutputEventActionNode<>)))
        {
            return false;
        }

        return isPayloadGenericCollection;
    }

    public object Resolve(IServiceProvider serviceProvider, ParameterInfo parameter)
    {
        var attribute = parameter.GetCustomAttribute<TokenNameAttribute>();
        if (!SupportedInterfaces.Contains(parameter.ParameterType))
        {
            throw new InvalidOperationException(
                $"TokenName can only be applied to non-generic payload token collection parameters; '{parameter.ParameterType}' is not supported.");
        }
        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"Parameter '{parameter.Name}' of type '{parameter.ParameterType.Name}' requires a TokenName attribute.");
        }

        var names = PayloadNameSet.Validate(attribute.Names, nameof(attribute.Names));
        return new PayloadTokenCollection(names, parameter.ParameterType == typeof(IOutputTokens));
    }
}
