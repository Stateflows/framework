﻿using System;
using System.Runtime.Serialization;

namespace Stateflows.Common.Extensions
{
    internal static class TypeExtensions
    {
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
    }
}
