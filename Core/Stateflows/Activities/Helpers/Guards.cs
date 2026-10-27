using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Activities
{
    /// <summary>Builds guards that test a named global value.</summary>
    /// <remarks>Each guard evaluates the value store when the flow is considered; missing values do not satisfy equality or membership checks.</remarks>
    public struct FlowGuardGlobalValueExpression
    {
        private readonly string ValueName;

        /// <summary>Creates an expression for a named global value.</summary>
        /// <param name="valueName">Name of the value in the global value store.</param>
        public FlowGuardGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Provides guard that checks if specified global value is set. 
        /// </summary>
        public Func<IActivityActionContext, Task<bool>> IsSet
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.IsSetAsync(self.ValueName);
            }
        }

        /// <summary>
        /// Provides guard that checks if specified global value is not set. 
        /// </summary>
        public Func<IActivityActionContext, Task<bool>> IsNotSet
        {
            get
            {
                var self = this;
                return async c => !await c.Behavior.Values.IsSetAsync(self.ValueName);
            }
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to given value.
        /// </summary>
        /// <param name="value">Value to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>An asynchronous guard that succeeds when the value exists and equals <paramref name="value"/>.</returns>
        public Func<IActivityActionContext, Task<bool>> IsEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is not equal to given value.
        /// </summary>
        /// <param name="value">Value to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>An asynchronous guard that succeeds when the value exists and differs from <paramref name="value"/>.</returns>
        public Func<IActivityActionContext, Task<bool>> IsNotEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && !result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to one of given values.
        /// </summary>
        /// <param name="values">Values to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>An asynchronous guard that succeeds when the value exists and matches at least one supplied value.</returns>
        public Func<IActivityActionContext, Task<bool>> IsOneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && values.Contains(result.Value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to none of given values.
        /// </summary>
        /// <param name="values">Values to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>An asynchronous guard that succeeds when the value exists and matches none of the supplied values.</returns>
        public Func<IActivityActionContext, Task<bool>> IsNoneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && !values.Contains(result.Value);
            };
        }
    }

    /// <summary>Selects a named value within a global value set for declarative guards.</summary>
    public struct TransitionGuardGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

        /// <summary>Creates an expression for a named global value set.</summary>
        /// <param name="namespaceName">Name of the global value set.</param>
        public TransitionGuardGlobalNamespaceExpression(string namespaceName)
        {
            this.NamespaceName = namespaceName;
        }

        /// <summary>
        /// Provides declarative guards based on the global value set's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value set's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public FlowGuardGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new FlowGuardGlobalValueExpression($"{self.NamespaceName}.{valueName}");
        }
        
        /// <summary>
        /// Provides guard that checks if specified namespace has any values. 
        /// </summary>
        public Func<IActivityActionContext, Task<bool>> HasAnyValues
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.HasAnyPrefixedAsync($"{self.NamespaceName}.");
            }
        }
    }

    /// <summary>Starts declarative guard expressions over global values.</summary>
    public struct FlowGuardGlobalSelector
    {
        /// <summary>
        /// Provides declarative guards based on the global value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public FlowGuardGlobalValueExpression Value(string valueName)
            => new FlowGuardGlobalValueExpression(valueName);
        
        /// <summary>
        /// Provides declarative guards based on the global value set with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the global value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalNamespaceExpression Namespace(string namespaceName)
            => new TransitionGuardGlobalNamespaceExpression(namespaceName);
    }
    
    /// <summary>
    /// Helper static class that provides predefined, declarative-style guards for transitions of all kinds. 
    /// </summary>
    public static class Guards
    {
        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        /// <param name="context">The flow context supplied by the activity runtime.</param>
        /// <returns>A task that completes with <see langword="true"/>.</returns>
        public static Task<bool> Empty(IActivityFlowContext context)
            => Allow(context);

        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        /// <param name="_">The flow context, unused by this unconditional guard.</param>
        /// <returns>A task that completes with <see langword="true"/>.</returns>
        public static Task<bool> Allow(IActivityFlowContext _)
            => Task.FromResult(true);

        /// <summary>
        /// Declares negative guard that denies transition unconditionally.
        /// </summary>
        /// <param name="_">The flow context, unused by this unconditional guard.</param>
        /// <returns>A task that completes with <see langword="false"/>.</returns>
        public static Task<bool> Deny(IActivityFlowContext _)
            => Task.FromResult(false);
        
        /// <summary>
        /// Provides declarative guards based on global values.
        /// </summary>
        public static FlowGuardGlobalSelector Global
            => new FlowGuardGlobalSelector();
    }
}