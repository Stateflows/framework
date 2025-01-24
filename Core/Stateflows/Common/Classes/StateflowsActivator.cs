using Stateflows.Common.Attributes;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Stateflows.Common.Classes
{
    public class StateflowsServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _innerProvider;

        public StateflowsServiceProvider(IServiceProvider innerProvider)
        {
            _innerProvider = innerProvider;
        }

        public object GetService(Type serviceType)
        {
            if (
                !serviceType.IsClass ||
                !serviceType.GetConstructors().Any(constructor =>
                    constructor.GetParameters().Any(parameter =>
                        parameter.ParameterType.IsSubclassOfRawGeneric(typeof(BaseValueAccessor<>))
                    )
                )
            )
            {
                return _innerProvider.GetService(serviceType);
            }
        
            var constructor = serviceType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        
            if (constructor != null)
            {
                var parameters = constructor.GetParameters();
                var parameterValues = new object[parameters.Length];
        
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (!parameter.ParameterType.IsSubclassOfRawGeneric(typeof(BaseValueAccessor<>)))
                    {
                        parameterValues[i] = _innerProvider.GetService(parameter.ParameterType);
        
                        continue;
                    }
        
                    var customAttribute = parameter.GetCustomAttribute<ValueNameAttribute>();
        
                    if (customAttribute != null)
                    {
                        // Resolve service by name
                        parameterValues[i] = Activator.CreateInstance(parameter.ParameterType, customAttribute.Name);
                    }
                    else
                    {
                        throw new StateflowsDefinitionException($"ValueNameAttribute not found for parameter {parameter.Name} in constructor {constructor.Name} of service {serviceType.Name}");
                    }
                }
        
                return Activator.CreateInstance(serviceType, parameterValues);
            }
        
            return null;
        }
    }
}
