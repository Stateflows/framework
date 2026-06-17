using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Stateflows.Entities.Models;

namespace Stateflows.Entities.Engine
{
    internal class EntityProxy<TTemplate> : DispatchProxy
        where TTemplate : class
    {
        private Dictionary<string, object> values;
        private Dictionary<string, (string FieldName, Type ValueType, bool IsComputed)> propertyMap;

        private readonly List<string> readFields = [];
        public IReadOnlyList<string> ReadFields => readFields;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name.StartsWith("get_"))
            {
                var propName = targetMethod.Name.Substring(4);
                
                readFields.Add(propName);
                
                if (propertyMap.TryGetValue(propName, out var field))
                {
                    var key = field.FieldName.GetFieldKey();
                    if (values.TryGetValue(key, out var stored))
                        return ConvertValue(stored, field.ValueType);
                }

                return GetDefaultValue(targetMethod.ReturnType);
            }

            if (targetMethod.Name.StartsWith("set_") && args?.Length == 1)
            {
                var propName = targetMethod.Name.Substring(4);
                if (propertyMap.TryGetValue(propName, out var field))
                {
                    if (field.IsComputed)
                        throw new InvalidOperationException($"Cannot set value of computed field '{field.FieldName}'");
                    
                    values[field.FieldName.GetFieldKey()] = args[0];
                }

                return null;
            }

            return null;
        }

        private static object ConvertValue(object stored, Type targetType)
        {
            if (stored is null) return null;
            if (targetType.IsAssignableFrom(stored.GetType())) return stored;

            if (stored is JsonElement element)
                return JsonSerializer.Deserialize(element.GetRawText(), targetType);

            try { return Convert.ChangeType(stored, targetType); }
            catch { return GetDefaultValue(targetType); }
        }

        private static object GetDefaultValue(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;

        internal static (EntityProxy<TTemplate>, TTemplate) Create(Dictionary<string, object> values, EntityModel model)
        {
            if (!typeof(TTemplate).IsInterface)
                throw new InvalidOperationException(
                    $"Entity template type '{typeof(TTemplate).Name}' must be an interface. " +
                    $"Define it as 'interface I{typeof(TTemplate).Name} {{ ... }}'.");

            var proxy = Create<TTemplate, EntityProxy<TTemplate>>();
            var p = (EntityProxy<TTemplate>)(object)proxy;
            p.values = values;
            p.propertyMap = model.Fields.Values
                // .Where(f => f.PropertyName != null)
                .ToDictionary(
                    f => f.Name,
                    f => (f.Name, f.ValueType, f.IsComputed)
                );
            return (p, proxy);
        }
    }
}

