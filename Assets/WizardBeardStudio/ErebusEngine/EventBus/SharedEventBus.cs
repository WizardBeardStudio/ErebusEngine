using System;
using System.Collections.Generic;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.EventBus
{
    /// <summary>
    /// Global Singleton type to broadcast events to <see cref="Action{T}"/> callbacks.
    /// Callbacks are stored in a private static Dictionary with <see cref="Type"/> keys.
    /// </summary>
    public class SharedEventBus : Singleton<SharedEventBus>
    {
        private static Dictionary<Type, List<object>> _subscribers = new();

        public void Subscribe<T>(Action<T> callback)
        {
            Type eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<object>();
            }
            
            _subscribers[eventType].Add(callback);
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            Type evenType = typeof(T);

            if (_subscribers.ContainsKey(evenType))
            {
                _subscribers[evenType].Remove(callback);
            }
        }

        public void Publish<T>(T e)
        {
            Type eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                foreach (var subscriber in _subscribers[eventType])
                {
                    (subscriber as Action<T>)?.Invoke(e);
                }
            }
        }
    }
}