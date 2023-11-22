using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Builders
{
    internal class FlowBuilder<TToken> :
        IFlowBuilder<TToken>,
        IFlowBuilderWithWeight<TToken>,
        IFlowBuilder
        where TToken : Token, new()
    {
        public Edge Edge { get; set; }
       
        public FlowBuilder(Edge edge)
        {
            Edge = edge;
        }

        public IFlowBuilderWithWeight<TToken> AddGuard(GuardDelegateAsync<TToken> guardAsync)
        {
            var logic = new Logic<PipelineActionAsync>()
            {
                Name = Constants.Guard
            };

            logic.Actions.Add(async c =>
            {
                try
                {
                    var result = new List<Token>();
                    var tokens = c.Tokens.OfType<TToken>().ToArray();
                    foreach (var token in tokens)
                    {
                        if (
                            await guardAsync(
                                new FlowContext<TToken>(
                                    (c as IRootContext).Context,
                                    (c as IRootContext).NodeScope,
                                    Edge,
                                    token
                                )
                            )
                        )
                        {
                            result.Add(token);
                        }
                    }

                    return result;
                }
                catch (Exception e)
                {
                    if (Edge.Source.Parent != null)
                    {
                        await Edge.Source.Parent.HandleExceptionAsync(e, c as BaseContext);
                    }

                    return new Token[0];
                }
            });

            Edge.Pipeline.Actions.Add(logic);

            return this;
        }

        public IFlowBuilderWithWeight<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            where TTransformedToken : Token, new()
        {
            var logic = new Logic<PipelineActionAsync>()
            {
                Name = Constants.Guard
            };

            logic.Actions.Add(async c =>
            {
                try
                {
                    var result = new List<Token>();
                    var tokens = c.Tokens.OfType<TToken>().ToArray();
                    foreach (var token in tokens)
                    {
                        result.Add(
                            await transformationAsync(
                                new FlowContext<TToken>(
                                    (c as IRootContext).Context,
                                    (c as IRootContext).NodeScope,
                                    Edge,
                                    token
                                )
                            )
                        );
                    }

                    return result;
                }
                catch (Exception e)
                {
                    if (Edge.Source.Parent != null)
                    {
                        await Edge.Source.Parent.HandleExceptionAsync(e, c as BaseContext);
                    }

                    return new Token[0];
                }

            });

            Edge.Pipeline.Actions.Add(logic);

            return new FlowBuilder<TTransformedToken>(Edge);
        }

        public IFlowBuilderWithWeight<TToken> SetWeight(int weight)
        {
            Edge.Weight = weight;

            return this;
        }

        IFlowBuilderWithWeight IControlFlowBuilderBase<IFlowBuilderWithWeight>.AddGuard(GuardDelegateAsync guardAsync)
            => AddGuard(c => guardAsync(c as IGuardContext)) as IFlowBuilderWithWeight;

        IFlowBuilderWithWeight IFlowWeight<IFlowBuilderWithWeight>.SetWeight(int weight)
            => SetWeight(weight) as IFlowBuilderWithWeight;
    }
}
