using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Activities
{
    public struct FlowGuardGlobalValueExpression
    {
        private readonly string ValueName;

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

    public struct TransitionGuardGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

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
                return c => ((ContextValuesCollection)c.Behavior.Values)!.HasAnyMatchingAsync(new Regex($"{self.NamespaceName}[.](.*)"));
            }
        }
    }

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
        public static Task<bool> Empty(IActivityFlowContext context)
            => Allow(context);

        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        public static Task<bool> Allow(IActivityFlowContext _)
            => Task.FromResult(true);

        /// <summary>
        /// Declares negative guard that denies transition unconditionally.
        /// </summary>
        public static Task<bool> Deny(IActivityFlowContext _)
            => Task.FromResult(false);
        
        /// <summary>
        /// Provides declarative guards based on global values.
        /// </summary>
        public static FlowGuardGlobalSelector Global
            => new FlowGuardGlobalSelector();
    }
}