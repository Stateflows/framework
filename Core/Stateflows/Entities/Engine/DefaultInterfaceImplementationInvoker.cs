using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Stateflows.Entities.Engine
{
    internal static class DefaultInterfaceImplementationInvoker
    {
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>> Invokers = [];

        public static bool HasDefaultImplementation(MethodInfo? method)
            =>
                method != null &&
                method.DeclaringType?.IsInterface == true &&
                !method.IsAbstract &&
                !method.IsStatic;

        public static object? Invoke(object target, MethodInfo method, object?[]? arguments)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!HasDefaultImplementation(method))
            {
                throw new InvalidOperationException($"Method '{method.DeclaringType?.FullName}.{method.Name}' does not have a default interface implementation.");
            }

            arguments ??= [];

            var parameters = method.GetParameters();
            if (parameters.Length != arguments.Length)
            {
                throw new TargetParameterCountException(
                    $"Method '{method.DeclaringType?.FullName}.{method.Name}' expects {parameters.Length} arguments, but received {arguments.Length}."
                );
            }

            return Invokers.GetOrAdd(method, BuildInvoker)(target, arguments);
        }

        private static Func<object, object?[], object?> BuildInvoker(MethodInfo method)
        {
            if (method.GetParameters().Any(parameter => parameter.ParameterType.IsByRef))
            {
                throw new InvalidOperationException(
                    $"Method '{method.DeclaringType?.FullName}.{method.Name}' cannot be invoked because by-ref parameters are not supported."
                );
            }

            var dynamicMethod = new DynamicMethod(
                $"Invoke_{method.DeclaringType?.Name}_{method.Name}",
                typeof(object),
                [typeof(object), typeof(object?[])],
                typeof(DefaultInterfaceImplementationInvoker).Module,
                true
            );

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, method.DeclaringType!);

            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);

                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, parameterType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, parameterType);
                }
            }

            il.Emit(OpCodes.Call, method);

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return (Func<object, object?[], object?>)dynamicMethod.CreateDelegate(typeof(Func<object, object?[], object?>));
        }
    }
}




