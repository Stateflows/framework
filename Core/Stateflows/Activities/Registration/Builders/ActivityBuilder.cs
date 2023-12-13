using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Extensions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;

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

        public ActivityBuilder(string name, int version, Node parentNode, IServiceCollection services)
            : base(parentNode, services)
        {
            Result = new Graph(name, version);
        }

        public IActivityBuilder AddInitializer(string initializerName, ActivityPredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityPredicateAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IActivityBuilder AddOnInitialize(Func<IActivityInitializationContext, Task<bool>> actionAsync)
            => AddOnInitialize<InitializationRequest>(c =>
            {
                var baseContext = c as BaseContext;
                var context = new ActivityInitializationContext(baseContext.Context, baseContext.NodeScope, baseContext.Context.Event as InitializationRequest);
                return actionAsync(context);
            });
        //{
        //    actionAsync.ThrowIfNull(nameof(actionAsync));

        //    var initializerName = EventInfo<InitializationRequest>.Name;

        //    return AddInitializer(initializerName, async c =>
        //    {
        //        var result = false;
        //        var context = new ActivityInitializationContext(c.Context, c.NodeScope, c.Context.Event as InitializationRequest);
        //        try
        //        {
        //            result = await actionAsync(context);
        //        }
        //        catch (Exception e)
        //        {
        //            await c.Context.Executor.Inspector.OnActivityInitializationExceptionAsync(context, e);
        //            result = false;
        //        }

        //        return result;
        //    });
        //}

        public IActivityBuilder AddOnInitialize<TInitializationRequest>(Func<IActivityInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            where TInitializationRequest : InitializationRequest, new()
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddActivityInvocationContext(Result);

            var initializerName = EventInfo<TInitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var result = false;
                var context = new ActivityInitializationContext<TInitializationRequest>(c.Context, c.Context.Executor.NodeScope, c.Context.Event as TInitializationRequest);
                try
                {
                    result = await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Context.Executor.Inspector.OnActivityInitializationExceptionAsync(context, e);
                    result = false;
                }

                return result;
            });
        }

        IActivityBuilder IActivity<IActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IActivity<IActivityBuilder>.AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddStructuredActivity(actionNodeName, builderAction) as IActivityBuilder;

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

        IActivityBuilder IActivity<IActivityBuilder>.AddParallelActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddParallelActivity<TToken>(actionNodeName, builderAction) as IActivityBuilder;

        IActivityBuilder IActivity<IActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction)
            => AddIterativeActivity<TToken>(actionNodeName, builderAction) as IActivityBuilder;

        IActivityBuilder IAcceptEvent<IActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync) as IActivityBuilder;

        IActivityBuilder ISendEvent<IActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync) as IActivityBuilder;
    }
}
