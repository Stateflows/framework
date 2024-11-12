using System;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Utils;
using Stateflows.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Builders
{
    internal class FlowBuilder<TToken> :
        IObjectFlowBuilder<TToken>,
        IObjectFlowBuilderWithWeight<TToken>,
        IElseObjectFlowBuilder<TToken>,
        IElseObjectFlowBuilderWithWeight<TToken>,
        IControlFlowBuilder,
        IControlFlowBuilderWithWeight,
        IElseControlFlowBuilder,
        IInternal
    {
        public Edge Edge { get; set; }

        public Graph Result => Edge.Graph;

        public IServiceCollection Services { get; private set; }

        public FlowBuilder(Edge edge, IServiceCollection services)
        {
            Edge = edge;
            Services = services;
        }

        public IObjectFlowBuilder<TToken> AddGuard(GuardDelegateAsync<TToken> guardAsync)
        {
            var logic = new Logic<TokenPipelineActionAsync>()
            {
                Name = Constants.Guard
            };

            logic.Actions.Add(async context =>
            {
                try
                {
                    return
                        context.Token is TokenHolder<TToken> token &&
                        await guardAsync(new TokenFlowContext<TToken>(context, token.Payload))
                            ? token
                            : default;
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!(Edge.Source != null && await Edge.Source.HandleExceptionAsync(e, context)))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }
            });

            Edge.TokenPipeline.Actions.Add(logic);

            return this;
        }

        public IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
        {
            var logic = new Logic<TokenPipelineActionAsync>()
            {
                Name = Constants.Guard
            };

            logic.Actions.Add(async context =>
            {
                try
                {
                    return
                        context.Token is TokenHolder<TToken> token
                            ? (await transformationAsync(new TokenFlowContext<TToken>(context, token.Payload))).ToTokenHolder()
                            : default;
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!(Edge.Source != null && await Edge.Source.HandleExceptionAsync(e, context)))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }

            });

            Edge.TokenPipeline.Actions.Add(logic);
            Edge.TargetTokenType = typeof(TTransformedToken);

            return new FlowBuilder<TTransformedToken>(Edge, Services);
        }

        public IObjectFlowBuilderWithWeight<TToken> SetWeight(int weight)
        {
            Edge.Weight = weight;

            return this;
        }

        IControlFlowBuilderWithWeight IControlFlowBuilderBase<IControlFlowBuilderWithWeight>.AddGuard(GuardDelegateAsync guardAsync)
            => AddGuard(c => guardAsync(c as IGuardContext)) as IControlFlowBuilderWithWeight;

        IControlFlowBuilderWithWeight IFlowWeight<IControlFlowBuilderWithWeight>.SetWeight(int weight)
            => SetWeight(weight) as IControlFlowBuilderWithWeight;

        IObjectFlowBuilder<TTransformedToken> IObjectFlowBuilder<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync);

        IObjectFlowBuilderWithWeight<TToken> IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>.AddGuard(GuardDelegateAsync<TToken> guardAsync)
            => AddGuard(guardAsync) as IObjectFlowBuilderWithWeight<TToken>;

        IObjectFlowBuilderWithWeight<TTransformedToken> IObjectFlowBuilderWithWeight<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IObjectFlowBuilderWithWeight<TTransformedToken>;

        IElseObjectFlowBuilderWithWeight<TToken> IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>.SetWeight(int weight)
            => SetWeight(weight) as IElseObjectFlowBuilderWithWeight<TToken>;

        IElseObjectFlowBuilder<TTransformedToken> IElseObjectFlowBuilder<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilder<TTransformedToken>;

        IElseObjectFlowBuilderWithWeight<TTransformedToken> IElseObjectFlowBuilderWithWeight<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilderWithWeight<TTransformedToken>;

        void IFlowWeight.SetWeight(int weight)
            => SetWeight(weight);
    }
}
