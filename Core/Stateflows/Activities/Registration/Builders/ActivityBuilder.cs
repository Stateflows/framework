using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Registration.Extensions;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivityBuilder :
        BaseActivityBuilder,
        IActivityBuilder
    {
        new public Graph Result
        {
            get => Node as Graph;
            set => Node = value;
        }

        public ActivityBuilder(string name, Node parentNode, IServiceCollection services)
            : base(parentNode, services)
        {
            Result = new Graph(name);
        }

        public IActivityBuilder AddInitializer(string initializerName, ActivityEventActionAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityEventActionAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IActivityBuilder AddOnInitialize(Func<IActivityInitializationContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            var initializerName = EventInfo<InitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var context = new ActivityInitializationContext(c.Context, c.NodeScope, c.Context.Event as InitializationRequest);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    //await c.Executor.Observer.OnActivityInitializeExceptionAsync(context, e);
                }
            });

            //if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            //{
            //    initializer = new Logic<ActivityEventActionAsync>()
            //    {
            //        Name = Constants.Initialize
            //    };

            //    Result.Initializers.Add(initializerName, initializer);
            //}

            //initializer.Actions.Add(async c =>
            //{
            //    var context = new ActivityInitializationContext(c.Event as InitializationRequest, c);
            //    try
            //    {
            //        await actionAsync(context);
            //    }
            //    catch (Exception e)
            //    {
            //        //await c.Executor.Observer.OnActivityInitializeExceptionAsync(context, e);
            //    }
            //});

            //return this;
        }

        public IActivityBuilder AddOnInitialize<TInitializationRequest>(Func<IActivityInitializationContext<TInitializationRequest>, Task> actionAsync)
            where TInitializationRequest : InitializationRequest, new()
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddActivityInvocationContext(Result);

            var initializerName = EventInfo<TInitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var context = new ActivityInitializationContext<TInitializationRequest>(c.Context, c.Context.Executor.NodeScope, c.Context.Event as TInitializationRequest);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    //await c.Executor.Observer.OnActivityInitializeExceptionAsync(context, e);
                }
            });

            //if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            //{
            //    initializer = new Logic<ActivityEventActionAsync>()
            //    {
            //        Name = Constants.Initialize
            //    };

            //    Result.Initializers.Add(initializerName, initializer);
            //}

            //initializer.Actions.Add(async c =>
            //{
            //    var context = new ActivityInitializationContext<TInitializationRequest>(c.Event as TInitializationRequest, c);
            //    try
            //    {
            //        await actionAsync(context);
            //    }
            //    catch (Exception e)
            //    {
            //        //await c.Executor.Observer.OnActivityInitializeExceptionAsync(context, e);
            //    }
            //});

            //return this;
        }

        IActivityBuilder IActivity<IActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IActivity<IActivityBuilder>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IActivityBuilder;

        //IActivityBuilder IActivityEvents<IActivityBuilder>.AddOnInitialize(Func<IActivityInitializationContext, Task> actionAsync)
        //    => AddOnInitialize(actionAsync) as IActivityBuilder;

        IActivityBuilder IActivityEvents<IActivityBuilder>.AddOnFinalize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IActivityBuilder;

        IActivityBuilder IInitial<IActivityBuilder>.AddInitial(InitialBuilderAction buildAction)
            => AddInitial(buildAction) as IActivityBuilder;

        IActivityBuilder IFinal<IActivityBuilder>.AddFinal()
            => AddFinal() as IActivityBuilder;

        IActivityBuilder IInput<IActivityBuilder>.AddInput(InputBuilderAction buildAction)
            => AddInput(buildAction) as IActivityBuilder;

        IActivityBuilder IOutput<IActivityBuilder>.AddOutput()
            => AddOutput() as IActivityBuilder;

        //IActivityBuilder IExceptionHandler<IActivityBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
        //    => AddExceptionHandler<TException>(exceptionHandler) as IActivityBuilder;

        IActivityBuilder IActivity<IActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IActivityBuilder;

        IActivityBuilder IActivity<IActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction)as IActivityBuilder;
    }
}
