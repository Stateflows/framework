using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Testing.StateMachines.Sequence;
using Microsoft.Extensions.Logging;
using Stateflows.Activities;

namespace StateMachine.IntegrationTests.Utils
{
    public abstract class StateflowsTestClass
    {
        private IServiceCollection _serviceCollection;
        protected IServiceCollection ServiceCollection => _serviceCollection ?? (_serviceCollection = new ServiceCollection());

        private IServiceProvider _serviceProvider;
        protected IServiceProvider ServiceProvider => _serviceProvider ?? (_serviceProvider = ServiceCollection.BuildServiceProvider());

        protected IStateMachineLocator StateMachineLocator => ServiceProvider.GetRequiredService<IStateMachineLocator>();

        protected IActivityLocator ActivityLocator => ServiceProvider.GetRequiredService<IActivityLocator>();

        protected ExecutionSequenceObserver ExecutionSequence => ServiceProvider.GetRequiredService<ExecutionSequenceObserver>();

        public virtual void Initialize()
        {
            ServiceCollection.AddStateflows(b => InitializeStateflows(b));
            ServiceCollection.AddSingleton<IExecutionSequenceBuilder, ExecutionSequence>();
            ServiceCollection.AddLogging(builder => builder.AddConsole());

            var hostedServices = ServiceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            Task.WaitAll(hostedServices.Select(s => s.StartAsync(new CancellationToken())).ToArray());
        }

        public virtual void Cleanup()
        {
            _serviceCollection = null;
            _serviceProvider = null;
        }

        protected abstract void InitializeStateflows(IStateflowsBuilder builder);
    }
}