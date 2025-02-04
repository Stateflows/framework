using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
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
                return c => c.StateMachine.Values.IsSetAsync(self.ValueName);
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
                return async c => !await c.StateMachine.Values.IsSetAsync(self.ValueName);
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
                var result = await c.StateMachine.Values.TryGetAsync<T>(self.ValueName);
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
                var result = await c.StateMachine.Values.TryGetAsync<T>(self.ValueName);
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
                var result = await c.StateMachine.Values.TryGetAsync<T>(self.ValueName);
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
                var result = await c.StateMachine.Values.TryGetAsync<T>(self.ValueName);
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

    public struct TransitionGuardGlobalValueSetExpression
    {
        private readonly string ValueSetName;

        public TransitionGuardGlobalValueSetExpression(string valueSetName)
        {
            ValueSetName = valueSetName;
        }

        /// <summary>
        /// Provides declarative guards based on the global value set's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value set's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardGlobalValueExpression($"{self.ValueSetName}.{valueName}");
        }
    }

    public struct TransitionGuardStateValueSetExpression
    {
        private readonly string ValueSetName;
        private readonly bool IsSource;

        public TransitionGuardStateValueSetExpression(string valueSetName, bool isSource)
        {
            ValueSetName = valueSetName;
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative guards based on the state value set's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state value set's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardStateValueExpression($"{self.ValueSetName}.{valueName}", self.IsSource);
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
        /// Provides declarative guards based on the global value set with given name.
        /// </summary>
        /// <param name="valueSetName">Name of the global value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueSetExpression ValueSet(string valueSetName)
            => new TransitionGuardGlobalValueSetExpression(valueSetName);
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
        /// Provides declarative guards based on the state's value set with given name.
        /// </summary>
        /// <param name="valueSetName">Name of the state's value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueSetExpression ValueSet(string valueSetName)
        {
            var self = this;
            return new TransitionGuardStateValueSetExpression(valueSetName, self.IsSource);
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
            => Task.FromResult(context.StateMachine.CurrentState.GetAllNodes().Any(node => node.Value.Name == State<TState>.Name));
        
        /// <summary>
        /// Declares guard that checks if state machine is in given state.
        /// </summary>
        /// <param name="stateName">Name of the state to be checked against.</param>
        public static Func<IStateMachineActionContext, Task<bool>> InState(string stateName)
            => c => Task.FromResult(c.StateMachine.CurrentState.GetAllNodes().Any(node => node.Value.Name == stateName));

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