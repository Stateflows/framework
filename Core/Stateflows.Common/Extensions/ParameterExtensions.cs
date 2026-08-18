using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stateflows.Common.Extensions
{
    public static class ParameterExtensions
    {
        public static bool IsNullable(this ParameterInfo parameter)
            => parameter.GetCustomAttribute<NullableAttribute>() != null ||
               parameter.ParameterType.GetCustomAttribute<NullableAttribute>() != null;
    }
}
