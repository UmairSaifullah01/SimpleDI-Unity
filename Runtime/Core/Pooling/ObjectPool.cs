using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Manages object pooling for transient objects
    /// </summary>
    internal class ObjectPool
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _pools = new();
        private readonly ConcurrentDictionary<Type, Func<object>> _factories = new();
        private readonly ConcurrentDictionary<object, Type> _objectTypes = new();
        private readonly int _maxPoolSize;

        public ObjectPool(int maxPoolSize = 1000)
        {
            _maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one
        /// </summary>
        public object Get(Type type, Func<object> factory)
        {
            _factories.TryAdd(type, factory);
            var pool = _pools.GetOrAdd(type, _ => new ConcurrentBag<object>());

            if (pool.TryTake(out var obj))
            {
                return obj;
            }

            obj = factory();
            _objectTypes.TryAdd(obj, type);
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        public void Return(object obj)
        {
            if (obj == null) return;

            if (!_objectTypes.TryRemove(obj, out var type))
                return;

            if (!_pools.TryGetValue(type, out var pool))
                return;

            if (pool.Count < _maxPoolSize)
            {
                pool.Add(obj);
            }
        }

        /// <summary>
        /// Clears all pools
        /// </summary>
        public void Clear()
        {
            _pools.Clear();
            _factories.Clear();
            _objectTypes.Clear();
        }

        /// <summary>
        /// Gets the current pool size for a type
        /// </summary>
        public int GetPoolSize(Type type)
        {
            return _pools.TryGetValue(type, out var pool) ? pool.Count : 0;
        }

        /// <summary>
        /// Gets the total number of pooled objects
        /// </summary>
        public int GetTotalPooledObjects()
        {
            return _pools.Sum(p => p.Value.Count);
        }
    }
}