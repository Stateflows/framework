using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Testing.StateMachines.Sequence;
using Stateflows.Common.Classes;
using Stateflows.Testing;

namespace StateMachine.IntegrationTests.Utils
{
    public abstract class StateflowsTestClass
    {
        private IServiceCollection serviceCollection;
        private IServiceCollection ServiceCollection => serviceCollection ??= new ServiceCollection();

        private IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider => serviceProvider ??= ServiceCollection.BuildServiceProvider();

        protected IStateMachineLocator StateMachineLocator => ServiceProvider.GetRequiredService<IStateMachineLocator>();

        protected IActivityLocator ActivityLocator => ServiceProvider.GetRequiredService<IActivityLocator>();

        protected ExecutionSequenceObserver ExecutionSequence => ServiceProvider.GetRequiredService<ExecutionSequenceObserver>();

        private TestingHost testingHost;
        private TestingHost TestingHost => testingHost ??= ServiceProvider.GetRequiredService<TestingHost>();

        public virtual void Initialize()
        {
            ServiceCollection.AddStateflows(b => InitializeStateflows(b));
            ServiceCollection.AddSingleton<IExecutionSequenceBuilder, ExecutionSequence>();
            ServiceCollection.AddSingleton<TestingHost>();
            ServiceCollection.AddSingleton<IHostApplicationLifetime>(services => services.GetRequiredService<TestingHost>());
            ServiceCollection.AddLogging(builder => builder.AddConsole());
            ServiceCollection
                .AddTransient(typeof(Input<>))
                .AddTransient(typeof(SingleInput<>))
                .AddTransient(typeof(OptionalInput<>))
                .AddTransient(typeof(OptionalSingleInput<>))
                .AddTransient(typeof(Output<>));

            var hostedServices = ServiceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            Task.WaitAll(hostedServices.Select(s => s.StartAsync(new CancellationToken())).ToArray());

            TestingHost.StartApplication();

            ContextValues.GlobalValues.Clear();
            ContextValues.StateValues.Clear();
            ContextValues.SourceStateValues.Clear();
            ContextValues.TargetStateValues.Clear();
        }

        public virtual void Cleanup()
        {
            TestingHost.StopApplication();

            serviceCollection = null;
            serviceProvider = null;
        }

        protected abstract void InitializeStateflows(IStateflowsBuilder builder);
    }
}