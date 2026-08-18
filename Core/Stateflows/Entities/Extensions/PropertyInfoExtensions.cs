using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Stateflows.Entities;

internal static class PropertyInfoExtensions
{
    internal static bool TryGetValidatedDefaultValueForProperty(this PropertyInfo property, out object? defaultValue, out string? validationError)
    {
        var defaultAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultAttribute == null)
        {
            defaultValue = null;
            validationError = null;
            return false;
        }

        if (TryConvertDefaultValue(defaultAttribute.Value, property.PropertyType, out defaultValue))
        {
            validationError = null;
            return true;
        }

        validationError = $"Default value '{FormatValue(defaultAttribute.Value)}' declared on property '{property.DeclaringType?.FullName}.{property.Name}' is not compatible with property type '{property.PropertyType.FullName}'.";
        defaultValue = null;
        return false;
    }

    internal static object? GetDefaultValueForProperty(this PropertyInfo property)
    {
        if (property.TryGetValidatedDefaultValueForProperty(out var defaultValue, out var validationError))
        {
            return defaultValue;
        }

        if (validationError != null)
        {
            throw new InvalidOperationException(validationError);
        }

        var propertyType = property.PropertyType;
        propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
    }

    private static bool TryConvertDefaultValue(object? value, Type targetType, out object? convertedValue)
    {
        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value == null)
        {
            convertedValue = null;
            return !effectiveType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
        }

        if (targetType.IsInstanceOfType(value) || effectiveType.IsInstanceOfType(value))
        {
            convertedValue = value;
            return true;
        }

        if (effectiveType.IsEnum)
        {
            if (value is string stringValue)
            {
                try
                {
                    convertedValue = Enum.Parse(effectiveType, stringValue, ignoreCase: true);
                    return true;
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    convertedValue = Enum.ToObject(effectiveType, value);
                    return true;
                }
                catch
                {
                }
            }
        }

        var targetConverter = TypeDescriptor.GetConverter(effectiveType);
        if (targetConverter.CanConvertFrom(value.GetType()))
        {
            try
            {
                convertedValue = targetConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                return true;
            }
            catch
            {
            }
        }

        var valueConverter = TypeDescriptor.GetConverter(value);
        if (valueConverter.CanConvertTo(effectiveType))
        {
            try
            {
                convertedValue = valueConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, effectiveType);
                return true;
            }
            catch
            {
            }
        }

        try
        {
            convertedValue = Convert.ChangeType(value, effectiveType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            convertedValue = null;
            return false;
        }
    }

    private static string FormatValue(object? value)
        => value == null
            ? "null"
            : value is string stringValue
                ? stringValue
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? value.GetType().Name;
}