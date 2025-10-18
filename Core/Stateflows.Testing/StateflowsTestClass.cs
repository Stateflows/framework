using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Testing;
using Stateflows.Testing.StateMachines.Sequence;

namespace StateMachine.IntegrationTests.Utils
{
    public abstract class StateflowsTestClass
    {
        private IServiceCollection serviceCollection;
        protected IServiceCollection ServiceCollection => serviceCollection ??= new ServiceCollection();

        private IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider => serviceProvider ??= ServiceCollection.BuildServiceProvider();

        protected IStateMachineLocator StateMachineLocator => ServiceProvider.GetRequiredService<IStateMachineLocator>();

        protected IActivityLocator ActivityLocator => ServiceProvider.GetRequiredService<IActivityLocator>();
        
        protected IActionLocator ActionLocator => ServiceProvider.GetRequiredService<IActionLocator>();

        protected ExecutionSequenceObserver ExecutionSequence => ServiceProvider.GetRequiredService<ExecutionSequenceObserver>();

        private TestingHost testingHost;
        private TestingHost TestingHost => testingHost ??= ServiceProvider.GetRequiredService<TestingHost>();

        public virtual void Initialize()
        {
            ServiceCollection.AddStateflows(b => InitializeStateflows(b.UseFullNamesFor(TypedElements.None)));
            ServiceCollection.AddSingleton<IExecutionSequenceBuilder, ExecutionSequence>();
            ServiceCollection.AddSingleton<TestingHost>();
            ServiceCollection.AddSingleton<IHostApplicationLifetime>(services => services.GetRequiredService<TestingHost>());
            ServiceCollection.AddSingleton<ExecutionSequenceObserver>();
            ServiceCollection.AddLogging(builder => builder.AddConsole());

            var hostedServices = ServiceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            Task.WaitAll(hostedServices.Select(s => s.StartAsync(new CancellationToken())).ToArray());

            TestingHost.StartApplication();

            ContextValues.Clear();
            
            _ = OutputTokens.GetAll();
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