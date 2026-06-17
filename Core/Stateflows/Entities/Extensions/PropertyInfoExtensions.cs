using System;
using System.ComponentModel;
using System.Reflection;

namespace Stateflows.Entities;

internal static class PropertyInfoExtensions
{
    internal static object GetDefaultValueForProperty(this PropertyInfo property)
    {
        var defaultAttr = (DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute));
        if (defaultAttr != null)
            return defaultAttr.Value;

        var propertyType = property.PropertyType;
        propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
    }
}