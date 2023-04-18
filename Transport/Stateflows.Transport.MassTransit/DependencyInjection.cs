using System;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.MassTransit.Stateflows;
using Stateflows.Transport.MassTransit.MassTransit;
using Stateflows.Transport.MassTransit.MassTransit.Consumers;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IBusRegistrationConfigurator AddStateflowsTransport(this IBusRegistrationConfigurator configure, string applicationId = null)
        {
            applicationId = "." + applicationId ?? Guid.NewGuid().ToString();

            configure.AddConsumer<PreflightRequestConsumer>().Endpoint(c => c.InstanceId = applicationId);
            configure.AddConsumer<PreflightResponseConsumer>().Endpoint(c => c.InstanceId = applicationId);
            configure.AddConsumer<BehaviorRequestConsumer>().Endpoint(c => c.InstanceId = applicationId);

            configure
                .AddSingleton<Discoverer>()
                .AddSingleton<BehaviorProvider>()
                .AddSingleton<IBehaviorProvider>(provider => provider.GetService<BehaviorProvider>())
                .AddHostedService(provider => provider.GetService<BehaviorProvider>())
                ;

            return configure;
        }
    }
}
