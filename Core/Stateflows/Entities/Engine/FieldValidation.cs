using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Validators;

namespace Stateflows.Entities.Engine;

internal static class FieldValidation
{
    public static async Task<EventValidation> ValidateAsync<T>(
        T value, MemberInfo member, object entity, IServiceProvider services)
    {
        var errors = new List<ValidationResult>();
        var context = new ValidationContext(entity, services, null) { MemberName = member.Name, DisplayName = member.Name };
        var valid = Validator.TryValidateValue(value, context, errors, member.GetCustomAttributes<ValidationAttribute>());
        foreach (var validator in services.GetServices<IStateflowsValidator>())
        {
            if (value == null && validator is AttributeValidator)
            {
                continue;
            }
            valid &= await validator.ValidateAsync(value, errors);
        }

        if (value != null)
        {
            // Registered validators check the root value; DataAnnotations on child objects also apply.
            ValidateChildren(value, member.Name, services, new HashSet<object>(ReferenceEqualityComparer.Instance), errors);
        }

        return new EventValidation(valid && errors.Count == 0, errors);
    }

    private static void ValidateChildren(object value, string path, IServiceProvider services,
        HashSet<object> visited, List<ValidationResult> errors)
    {
        if (IsScalar(value) || !visited.Add(value))
        {
            return;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Value != null)
                {
                    ValidateChild(entry.Value, $"{path}[{entry.Key}]", services, visited, errors);
                }
            }
            return;
        }

        if (value is IEnumerable items)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (item != null)
                {
                    ValidateChild(item, $"{path}[{index}]", services, visited, errors);
                }
                index++;
            }
            return;
        }

        foreach (var property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(property => property.CanRead && property.GetIndexParameters().Length == 0))
        {
            if (property.GetValue(value) is { } child)
            {
                ValidateChild(child, $"{path}.{property.Name}", services, visited, errors);
            }
        }
    }

    private static void ValidateChild(object value, string path, IServiceProvider services,
        HashSet<object> visited, List<ValidationResult> errors)
    {
        if (IsScalar(value) || visited.Contains(value))
        {
            return;
        }

        var childErrors = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value, services, null), childErrors, true);
        errors.AddRange(childErrors.Select(error => new ValidationResult(error.ErrorMessage,
            error.MemberNames.Select(name => $"{path}.{name}").DefaultIfEmpty(path))));
        ValidateChildren(value, path, services, visited, errors);
    }

    private static bool IsScalar(object value)
    {
        var type = value.GetType();
        return Type.GetTypeCode(type) != TypeCode.Object ||
            value is Guid or TimeSpan or DateOnly or TimeOnly;
    }
}
