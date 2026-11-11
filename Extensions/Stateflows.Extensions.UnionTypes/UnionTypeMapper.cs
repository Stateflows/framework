using System.Reflection;
using System.Runtime.CompilerServices;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.UnionTypes;

internal sealed class UnionTypeMapper : IStateflowsTypeMapper
{
    public bool TryMapType(Type type, out IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(type);
        types = [];
        if (!type.IsDefined(typeof(UnionAttribute), inherit: false))
            return false;

        var provider = type.GetNestedType("IUnionMembers", BindingFlags.Public | BindingFlags.NonPublic);
        IEnumerable<MethodBase> creationMembers;
        if (provider is { IsInterface: true })
        {
            // Reflection returns an open nested type even for a constructed generic union.
            if (type.IsConstructedGenericType)
                provider = provider.MakeGenericType(type.GetGenericArguments());
            if (!provider.IsNestedPublic || !type.GetInterfaces().Contains(provider))
                throw new InvalidOperationException($"Union '{type}' must implement its public IUnionMembers provider.");

            creationMembers = provider.GetInterfaces().Prepend(provider)
                .SelectMany(memberType => memberType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.Name == "Create" && !method.IsGenericMethod && method.ReturnType == type);
        }
        else
        {
            creationMembers = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        }

        var cases = creationMembers.Select(member => member.GetParameters())
            .Where(parameters => parameters.Length == 1)
            .Select(parameters => parameters[0])
            .Where(parameter => !parameter.ParameterType.IsByRef || parameter.IsIn && !parameter.IsOut)
            .Select(parameter => parameter.ParameterType.IsByRef
                ? parameter.ParameterType.GetElementType()!
                : parameter.ParameterType)
            .Distinct()
            .ToArray();

        if (cases.Length == 0)
            throw new InvalidOperationException($"Union '{type}' has no public union creation members.");

        types = cases;
        return true;
    }
}
