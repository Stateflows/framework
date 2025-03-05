using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public struct TransitionGuardGlobalValueExpression
    {
        private readonly string ValueName;

        public TransitionGuardGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Provides guard that checks if specified global value is set. 
        /// </summary>
        public Func<IStateMachineActionContext, Task<bool>> IsSet
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
        public Func<IStateMachineActionContext, Task<bool>> IsNotSet
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
        public Func<IStateMachineActionContext, Task<bool>> IsEqualTo<T>(T value)
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
        public Func<IStateMachineActionContext, Task<bool>> IsNotEqualTo<T>(T value)
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
        public Func<IStateMachineActionContext, Task<bool>> IsOneOf<T>(params T[] values)
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
        public Func<IStateMachineActionContext, Task<bool>> IsNoneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && !values.Contains(result.Value);
            };
        }
    }

    public struct TransitionGuardStateValueExpression
    {
        private readonly string ValueName;
        private readonly bool IsSource;

        public TransitionGuardStateValueExpression(string valueName, bool isSource)
        {
            ValueName = valueName;
            IsSource = isSource;
        }

        private IContextValues GetValueSet(ITransitionContext transitionContext)
            => IsSource
                ? transitionContext.Source.Values
                : transitionContext.Target?.Values;

        /// <summary>
        /// Provides guard that checks if specified state's value is set. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> IsSet
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null
                        ? valueSet.IsSetAsync(self.ValueName)
                        : Task.FromResult(false);
                };
            }
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is not set. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> IsNotSet
        {
            get
            {
                var self = this;
                return async c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null && !await valueSet.IsSetAsync(self.ValueName);
                };
            }
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to given value.
        /// </summary>
        /// <param name="value">Value to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public Func<ITransitionContext, Task<bool>> IsEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is not equal to given value.
        /// </summary>
        /// <param name="value">Value to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public Func<ITransitionContext, Task<bool>> IsNotEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && !result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to one of given values.
        /// </summary>
        /// <param name="values">Values to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public Func<ITransitionContext, Task<bool>> IsOneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && values.Contains(result.Value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to none of given values.
        /// </summary>
        /// <param name="values">Values to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public Func<ITransitionContext, Task<bool>> IsNoneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && !values.Contains(result.Value);
            };
        }
    }

    public struct TransitionGuardGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

        public TransitionGuardGlobalNamespaceExpression(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Provides declarative guards based on the global namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global namespace's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardGlobalValueExpression($"{self.NamespaceName}.{valueName}");
        }

        /// <summary>
        /// Provides declarative guards based on the global namespace's sub-namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the global namespace's sub-namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardGlobalNamespaceExpression($"{self.NamespaceName}.{namespaceName}");
        }
        
        /// <summary>
        /// Provides guard that checks if specified namespace has any values. 
        /// </summary>
        public Func<IStateMachineActionContext, Task<bool>> HasAnyValues
        {
            get
            {
                var self = this;
                return c => ((ContextValuesCollection)c.Behavior.Values)!.HasAnyMatchingAsync(new Regex($"{self.NamespaceName}[.](.*)"));
            }
        }
    }

    public struct TransitionGuardStateNamespaceExpression
    {
        private readonly string NamespaceName;
        private readonly bool IsSource;

        public TransitionGuardStateNamespaceExpression(string namespaceName, bool isSource)
        {
            NamespaceName = namespaceName;
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative guards based on the state namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state namespace's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardStateValueExpression($"{self.NamespaceName}.{valueName}", self.IsSource);
        }

        /// <summary>
        /// Provides declarative guards based on the state namespace's sub-namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the state namespace's sub-namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardStateNamespaceExpression($"{self.NamespaceName}.{namespaceName}", IsSource);
        }

        private IContextValues GetValueSet(ITransitionContext transitionContext)
            => IsSource
                ? transitionContext.Source.Values
                : transitionContext.Target?.Values;
        
        /// <summary>
        /// Provides guard that checks if specified namespace has any values. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> HasAnyValues
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null
                        ? ((ContextValuesCollection)valueSet)!.HasAnyMatchingAsync(new Regex($"{self.NamespaceName}[.](.*)"))
                        : Task.FromResult(false);
                };
            }
        }
    }

    public struct TransitionGuardGlobalSelector
    {
        /// <summary>
        /// Provides declarative guards based on the global value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueExpression Value(string valueName)
            => new TransitionGuardGlobalValueExpression(valueName);
        
        /// <summary>
        /// Provides declarative guards based on the global namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the global namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalNamespaceExpression Namespace(string namespaceName)
            => new TransitionGuardGlobalNamespaceExpression(namespaceName);
    }

    public struct TransitionGuardStateSelector
    {
        private readonly bool IsSource;

        public TransitionGuardStateSelector(bool isSource)
        {
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative guards based on the state's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardStateValueExpression(valueName, self.IsSource);
        }
        
        /// <summary>
        /// Provides declarative guards based on the state's namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the state's namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardStateNamespaceExpression(namespaceName, self.IsSource);
        }
    }
    
    /// <summary>
    /// Helper static class that provides predefined, declarative-style guards for transitions of all kinds. 
    /// </summary>
    public static class Guards
    {
        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        public static Task<bool> Empty<TEvent>(ITransitionContext<TEvent> context)
            => Allow(context);

        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        public static Task<bool> Allow<TEvent>(ITransitionContext<TEvent> _)
            => Task.FromResult(true);

        /// <summary>
        /// Declares negative guard that denies transition unconditionally.
        /// </summary>
        public static Task<bool> Deny<TEvent>(ITransitionContext<TEvent> _)
            => Task.FromResult(false);
        
        /// <summary>
        /// Declares guard that checks if state machine is in given state.
        /// </summary>
        /// <typeparam name="TState">State to be checked against.</typeparam>
        public static Task<bool> InState<TState>(IStateMachineActionContext context)
            where TState : class, IVertex
            => Task.FromResult(context.CurrentState.GetAllNodes().Any(node => node.Value.Name == State<TState>.Name));
        
        /// <summary>
        /// Declares guard that checks if state machine is in given state.
        /// </summary>
        /// <param name="stateName">Name of the state to be checked against.</param>
        public static Func<IStateMachineActionContext, Task<bool>> InState(string stateName)
            => c => Task.FromResult(c.CurrentState.GetAllNodes().Any(node => node.Value.Name == stateName));

        /// <summary>
        /// Provides declarative guards based on global values.
        /// </summary>
        public static TransitionGuardGlobalSelector Global
            => new TransitionGuardGlobalSelector();
        
        /// <summary>
        /// Provides declarative guards based on the source values.
        /// </summary>
        public static TransitionGuardStateSelector Source
            => new TransitionGuardStateSelector(true);
        
        /// <summary>
        /// Provides declarative guards based on the target values.
        /// </summary>
        public static TransitionGuardStateSelector Target
            => new TransitionGuardStateSelector(false);
    }
}