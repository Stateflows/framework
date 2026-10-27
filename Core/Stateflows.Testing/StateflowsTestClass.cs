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
    /// <summary>Provides a lightweight host and service-provider lifecycle for Stateflows integration tests.</summary>
    /// <remarks>Derived tests configure behavior definitions in <see cref="InitializeStateflows"/> and call <see cref="Initialize"/> before exercising them.</remarks>
    public abstract class StateflowsTestClass
    {
        private IServiceCollection serviceCollection;
        /// <summary>Gets the service collection populated by the test host.</summary>
        protected IServiceCollection ServiceCollection => serviceCollection ??= new ServiceCollection();

        private IServiceProvider serviceProvider;
        /// <summary>Gets the provider built from <see cref="ServiceCollection"/>.</summary>
        protected IServiceProvider ServiceProvider => serviceProvider ??= ServiceCollection.BuildServiceProvider();

        /// <summary>Gets the registered state-machine locator.</summary>
        protected IStateMachineLocator StateMachineLocator => ServiceProvider.GetRequiredService<IStateMachineLocator>();

        /// <summary>Gets the registered activity locator.</summary>
        protected IActivityLocator ActivityLocator => ServiceProvider.GetRequiredService<IActivityLocator>();
        
        /// <summary>Gets the registered action locator.</summary>
        protected IActionLocator ActionLocator => ServiceProvider.GetRequiredService<IActionLocator>();

        /// <summary>Gets the observer that records state-machine execution callbacks.</summary>
        protected ExecutionSequenceObserver ExecutionSequence => ServiceProvider.GetRequiredService<ExecutionSequenceObserver>();

        private TestingHost testingHost;
        private TestingHost TestingHost => testingHost ??= ServiceProvider.GetRequiredService<TestingHost>();

        /// <summary>Builds and starts the test service provider and its hosted services.</summary>
        /// <remarks>This initializes Stateflows with the legacy runtime, invokes <see cref="InitializeStateflows"/>, starts registered hosted services, and starts the test host.</remarks>
        public virtual void Initialize()
        {
            ServiceCollection.AddStateflows(b => InitializeStateflows(b.AddLegacyRuntime().UseFullNamesFor(TypedElements.All)));
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

        /// <summary>Stops the test host and clears the cached service collection and provider.</summary>
        public virtual void Cleanup()
        {
            TestingHost.StopApplication();

            serviceCollection = null;
            serviceProvider = null;
        }

        /// <summary>Registers the Stateflows behaviors, services, and event definitions required by the derived test.</summary>
        /// <param name="builder">The Stateflows builder configured by the test host.</param>
        protected abstract void InitializeStateflows(IStateflowsBuilder builder);
    }
}