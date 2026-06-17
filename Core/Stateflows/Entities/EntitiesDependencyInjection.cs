using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Registration;
using Stateflows.Entities.Registration.Builders;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public static class EntitiesDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, EntitiesRegister> Registers = new();

        internal static void Cleanup(IStateflowsBuilder builder)
        {
            lock (Registers)
            {
                if (Registers.TryGetValue(builder, out var register) && !register.Entities.Any())
                {
                    var serviceDescriptor = builder.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IEntitiesRegister));
                    builder.ServiceCollection.Remove(serviceDescriptor);
                }
            }
        }

        internal static void Build(IStateflowsBuilder builder)
        {
            var stateflowsBuilder = (StateflowsBuilder)builder;
            lock (Registers)
            {
                if (
                    builder.ServiceCollection.IsServiceRegistered<IEntitiesRegister>() &&
                    Registers.TryGetValue(builder, out var register)
                )
                {
                    var defaultResource = stateflowsBuilder.ResourceNames[string.Empty];

                    foreach (var registration in register.Entities.Values)
                    {
                        var behaviorClass = new BehaviorClass(BehaviorType.Entity, registration.Name);
                        stateflowsBuilder.ResourcesByBehaviorClass[behaviorClass] = defaultResource;
                    }
                }
            }
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddEntities(this IStateflowsBuilder stateflowsBuilder,
            EntitiesBuildAction buildAction = null)
            => AddEntities(stateflowsBuilder, buildAction, null, null);

        [DebuggerHidden]
        internal static IStateflowsBuilder AddEntities(this IStateflowsBuilder stateflowsBuilder,
            EntitiesBuildAction buildAction, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            var register = stateflowsBuilder.EnsureEntitiesServices();
            buildAction?.Invoke(new EntitiesBuilder(register, ownerClass, parentClass));

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        internal static EntitiesRegister EnsureEntitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            lock (Registers)
            {
                if (!Registers.TryGetValue(stateflowsBuilder, out var register))
                {
                    register = new EntitiesRegister();
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddSingleton(register)
                        .AddSingleton<IEntitiesRegister>(register)
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        ;
                }

                return register;
            }
        }
    }
}


