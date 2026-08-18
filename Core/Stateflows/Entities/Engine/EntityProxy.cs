using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Models;

namespace Stateflows.Entities.Engine
{
    internal class EntityProxy<TTemplate> : DispatchProxy
        where TTemplate : class
    {
        private Dictionary<string, object> values = null!;
        private Dictionary<string, (string FieldName, Type ValueType, bool IsComputed)> fieldMap = null!;
        private TTemplate proxy = null!;

        private static readonly Dictionary<string, PropertyInfo> TemplateProperties = typeof(TTemplate)
            .GetInterfaces()
            .Prepend(typeof(TTemplate))
            .SelectMany(i => i.GetProperties())
            .Where(property => property.GetIndexParameters().Length == 0)
            .ToDictionary(property => property.Name);

        private readonly List<string> readFields = [];
        public IReadOnlyList<string> ReadFields => readFields;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var method = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            var arguments = args ?? [];

            if (method.DeclaringType == typeof(object))
            {
                return method.Name switch
                {
                    nameof(ToString) => $"{typeof(TTemplate).FullName} proxy",
                    nameof(GetHashCode) => RuntimeHelpers.GetHashCode(proxy),
                    nameof(Equals) => ReferenceEquals(proxy, arguments.SingleOrDefault()),
                    _ => null,
                };
            }

            if (method.Name.StartsWith("get_"))
            {
                var propName = method.Name.Substring(4);

                var property = GetFieldProperty(propName);
                readFields.Add(propName);

                if (fieldMap.TryGetValue(propName, out var field))
                {
                    var key = field.FieldName.GetFieldKey();
                    if (values.TryGetValue(key, out var stored))
                    {
                        return ConvertValue(stored, field.ValueType);
                    }

                    return GetDefaultValue(field.ValueType);
                }

                throw new InvalidOperationException(
                    $"Field property '{typeof(TTemplate).FullName}.{property.Name}' is not registered in entity model."
                );
            }

            if (method.Name.StartsWith("set_") && arguments.Length == 1)
            {
                var propName = method.Name.Substring(4);
                var property = GetFieldProperty(propName);

                if (fieldMap.TryGetValue(propName, out var field))
                {
                    if (field.IsComputed)
                        throw new InvalidOperationException($"Cannot set value of computed field '{field.FieldName}'");

                    values[field.FieldName.GetFieldKey()] = arguments[0];

                    return null;
                }

                throw new InvalidOperationException(
                    $"Field property '{typeof(TTemplate).FullName}.{property.Name}' is not registered in entity model."
                );
            }

            if (method.GetCustomAttribute<MutationAttribute>() != null)
            {
                throw new InvalidOperationException(
                    $"Mutation method '{typeof(TTemplate).FullName}.{method.Name}' cannot be invoked through entity proxy."
                );
            }

            if (DefaultInterfaceImplementationInvoker.HasDefaultImplementation(method))
            {
                return DefaultInterfaceImplementationInvoker.Invoke(proxy, method, arguments);
            }

            throw new InvalidOperationException(
                $"Method '{typeof(TTemplate).FullName}.{method.Name}' does not have a default implementation and cannot be invoked through entity proxy."
            );
        }

        private static PropertyInfo GetFieldProperty(string propertyName)
        {
            if (!TemplateProperties.TryGetValue(propertyName, out var property))
            {
                throw new InvalidOperationException(
                    $"Property '{typeof(TTemplate).FullName}.{propertyName}' is not defined on entity template."
                );
            }

            if (property.GetCustomAttribute<FieldAttribute>() == null)
            {
                if (property.GetCustomAttribute<ProjectionAttribute>() != null)
                {
                    throw new InvalidOperationException(
                        $"Projection property '{typeof(TTemplate).FullName}.{property.Name}' cannot be accessed through entity proxy."
                    );
                }

                throw new InvalidOperationException(
                    $"Only properties marked with [Field] can be accessed through entity proxy. Property '{typeof(TTemplate).FullName}.{property.Name}' is not marked with [Field]."
                );
            }

            return property;
        }

        private static object? ConvertValue(object stored, Type targetType)
        {
            if (stored is null) return null;

            var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (targetType.IsAssignableFrom(stored.GetType()) || effectiveType.IsAssignableFrom(stored.GetType())) return stored;

            if (stored is JsonElement element)
                return JsonSerializer.Deserialize(element.GetRawText(), targetType);

            if (effectiveType.IsEnum)
            {
                try
                {
                    return stored is string enumName
                        ? Enum.Parse(effectiveType, enumName, ignoreCase: true)
                        : Enum.ToObject(effectiveType, stored);
                }
                catch
                {
                    return GetDefaultValue(targetType);
                }
            }

            try { return Convert.ChangeType(stored, effectiveType); }
            catch { return GetDefaultValue(targetType); }
        }

        private static object? GetDefaultValue(Type type)
            => (Nullable.GetUnderlyingType(type) ?? type).IsValueType && Nullable.GetUnderlyingType(type) == null
                ? Activator.CreateInstance(type)
                : null;

        internal static (EntityProxy<TTemplate>, TTemplate) Create(Dictionary<string, object> values, EntityModel model)
        {
            if (!typeof(TTemplate).IsInterface)
                throw new InvalidOperationException(
                    $"Entity template type '{typeof(TTemplate).Name}' must be an interface. " +
                    $"Define it as 'interface I{typeof(TTemplate).Name} {{ ... }}'.");

            var proxy = Create<TTemplate, EntityProxy<TTemplate>>();
            var p = (EntityProxy<TTemplate>)(object)proxy;
            p.proxy = proxy;
            p.values = values;
            p.fieldMap = model.Fields.Values
                .ToDictionary(
                    f => f.Name,
                    f => (f.Name, f.ValueType, f.IsComputed)
                );
            return (p, proxy);
        }
    }
}

