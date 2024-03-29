﻿using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Stateflows.Common.Extensions
{
    public static class TypeExtensions
    {
        public static string GetReadableName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            else
            {
                var typeName = type.GetGenericTypeDefinition().FullName.Split('`').First();
                var typeNames = string.Join(", ", type.GetGenericArguments().Select(t => t.FullName));
                return $"{typeName}<{typeNames}>";
            }
        }

        public static object GetUninitializedInstance(this Type type)
            => FormatterServices.GetUninitializedObject(type);

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;

                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        public static Type GetGenericParameterOf(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;

                if (generic == cur)
                {
                    return toCheck.GetGenericArguments().First();
                }

                toCheck = toCheck.BaseType;
            }

            return null;
        }
    }
}
