using System;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Utils;
using Stateflows.Common.Exceptions;

namespace Stateflows.Activities.Registration.Builders
{
    internal class FlowBuilder<TToken> :
        IObjectFlowBuilder<TToken>,
        IObjectFlowBuilderWithWeight<TToken>,
        IElseObjectFlowBuilder<TToken>,
        IElseObjectFlowBuilderWithWeight<TToken>,
        IControlFlowBuilder,
        IControlFlowBuilderWithWeight,
        IElseControlFlowBuilder
    {
        public Edge Edge { get; set; }
       
        public FlowBuilder(Edge edge)
        {
            Edge = edge;
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
                            throw new ExecutionException(e);
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
                            throw new ExecutionException(e);
                        }
                    }
                }

            });

            Edge.TokenPipeline.Actions.Add(logic);
            Edge.TargetTokenType = typeof(TTransformedToken);

            return new FlowBuilder<TTransformedToken>(Edge);
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

        IObjectFlowBuilder<TToken> IObjectFlowTransformationBuilderBase<TToken, IObjectFlowBuilder<TToken>>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IObjectFlowBuilder<TToken>;

        IObjectFlowBuilderWithWeight<TToken> IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>.AddGuard(GuardDelegateAsync<TToken> guardAsync)
            => AddGuard(guardAsync) as IObjectFlowBuilderWithWeight<TToken>;

        IObjectFlowBuilderWithWeight<TToken> IObjectFlowTransformationBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IObjectFlowBuilderWithWeight<TToken>;

        IElseObjectFlowBuilderWithWeight<TToken> IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>.SetWeight(int weight)
            => SetWeight(weight) as IElseObjectFlowBuilderWithWeight<TToken>;

        IElseObjectFlowBuilder<TToken> IObjectFlowTransformationBuilderBase<TToken, IElseObjectFlowBuilder<TToken>>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilder<TToken>;

        IElseObjectFlowBuilderWithWeight<TToken> IObjectFlowTransformationBuilderBase<TToken, IElseObjectFlowBuilderWithWeight<TToken>>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilderWithWeight<TToken>;

        void IFlowWeight.SetWeight(int weight)
            => SetWeight(weight);
    }
}
