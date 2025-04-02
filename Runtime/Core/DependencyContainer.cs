using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Enum for defining the lifetime of a dependency.
    /// </summary>
    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    /// <summary>
    /// DependencyContainer class provides dependency injection functionality.
    /// </summary>
    public partial class DependencyContainer : IContainer
    {
        private readonly ConcurrentDictionary<Type, List<IBindingConfiguration>> _bindings;
        private readonly ConcurrentDictionary<Type, List<IDecoratorBindingConfiguration>> _decorators;
        private readonly ConcurrentDictionary<Type, List<ICollectionBindingConfiguration>> _collections;
        private readonly ConcurrentDictionary<Type, object> _singletons;
        private readonly ObjectPool _objectPool;
        private readonly ILogger _logger;
        private readonly HashSet<Type> _registeredTypes;
        private readonly HashSet<Type> _registeredDecorators;
        private readonly HashSet<Type> _registeredCollections;
        private readonly HashSet<Type> _resolvingTypes;
        private bool _disposed;

        public IContainer Parent { get; }

        public DependencyContainer(
            List<IBindingConfiguration> bindings,
            Dictionary<Type, List<IDecoratorBindingConfiguration>> decorators,
            Dictionary<Type, List<ICollectionBindingConfiguration>> collections,
            HashSet<Type> registeredTypes,
            HashSet<Type> registeredDecorators,
            HashSet<Type> registeredCollections,
            IContainer parent = null,
            ILogger logger = null)
        {
            _bindings = new ConcurrentDictionary<Type, List<IBindingConfiguration>>();
            _decorators = new ConcurrentDictionary<Type, List<IDecoratorBindingConfiguration>>(decorators);
            _collections = new ConcurrentDictionary<Type, List<ICollectionBindingConfiguration>>(collections);
            _singletons = new ConcurrentDictionary<Type, object>();
            _objectPool = new ObjectPool();
            _logger = logger ?? new DefaultLogger();
            _registeredTypes = registeredTypes;
            _registeredDecorators = registeredDecorators;
            _registeredCollections = registeredCollections;
            _resolvingTypes = new HashSet<Type>();
            Parent = parent;

            foreach (var binding in bindings)
            {
                if (!_bindings.ContainsKey(binding.ServiceType))
                {
                    _bindings[binding.ServiceType] = new List<IBindingConfiguration>();
                }
                _bindings[binding.ServiceType].Add(binding);
            }
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public T ResolveNamed<T>(string name) where T : class
        {
            return (T)ResolveNamed(typeof(T), name);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return ResolveAll(typeof(T)).Cast<T>();
        }

        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegisteredNamed<T>(string name) where T : class
        {
            return IsRegisteredNamed(typeof(T), name);
        }

        public IScope CreateScope()
        {
            return new Scope(this);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _objectPool.Clear();
            _singletons.Clear();
            _resolvingTypes.Clear();
            ReflectionCache.Clear();

            _disposed = true;
        }

        private object Resolve(Type type)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyContainer));

            if (!_bindings.TryGetValue(type, out var bindings) || !bindings.Any())
            {
                if (Parent != null)
                {
                    // Create a generic method call to Parent.Resolve<T>()
                    var resolveMethod = typeof(IContainer).GetMethod(nameof(IContainer.Resolve))
                        ?.MakeGenericMethod(type);
                    return resolveMethod?.Invoke(Parent, null);
                }

                throw new InvalidOperationException($"No registration found for type {type}");
            }

            var binding = bindings.First();
            return ResolveBinding(binding);
        }

        private object ResolveNamed(Type type, string name)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyContainer));

            if (!_bindings.TryGetValue(type, out var bindings))
            {
                if (Parent != null)
                {
                    // Create a generic method call to Parent.ResolveNamed<T>()
                    var resolveNamedMethod = typeof(IContainer).GetMethod(nameof(IContainer.ResolveNamed))
                        ?.MakeGenericMethod(type);
                    return resolveNamedMethod?.Invoke(Parent, new object[] { name });
                }

                throw new InvalidOperationException($"No registration found for type {type}");
            }

            var binding = bindings.FirstOrDefault(b => b.Name == name);
            if (binding == null)
            {
                if (Parent != null)
                {
                    // Create a generic method call to Parent.ResolveNamed<T>()
                    var resolveNamedMethod = typeof(IContainer).GetMethod(nameof(IContainer.ResolveNamed))
                        ?.MakeGenericMethod(type);
                    return resolveNamedMethod?.Invoke(Parent, new object[] { name });
                }

                throw new InvalidOperationException($"No registration found for type {type} with name {name}");
            }

            return ResolveBinding(binding);
        }

        private IEnumerable<object> ResolveAll(Type type)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyContainer));

            var results = new List<object>();

            if (_bindings.TryGetValue(type, out var bindings))
            {
                results.AddRange(bindings.Select(ResolveBinding));
            }

            if (Parent != null)
            {
                // Create a generic method call to Parent.ResolveAll<T>()
                var resolveAllMethod = typeof(IContainer).GetMethod(nameof(IContainer.ResolveAll))
                    ?.MakeGenericMethod(type);
                var parentResults = (IEnumerable)resolveAllMethod?.Invoke(Parent, null);
                if (parentResults != null)
                {
                    results.AddRange(parentResults.Cast<object>());
                }
            }

            return results;
        }

        private bool IsRegistered(Type type)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyContainer));

            if (_bindings.ContainsKey(type))
                return true;

            if (Parent != null)
            {
                // Create a generic method call to Parent.IsRegistered<T>()
                var isRegisteredMethod = typeof(IContainer).GetMethod(nameof(IContainer.IsRegistered))
                    ?.MakeGenericMethod(type);
                return (bool)isRegisteredMethod?.Invoke(Parent, null);
            }

            return false;
        }

        private bool IsRegisteredNamed(Type type, string name)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyContainer));

            if (_bindings.TryGetValue(type, out var bindings))
            {
                if (bindings.Any(b => b.Name == name))
                    return true;
            }

            if (Parent != null)
            {
                // Create a generic method call to Parent.IsRegisteredNamed<T>()
                var isRegisteredNamedMethod = typeof(IContainer).GetMethod(nameof(IContainer.IsRegisteredNamed))
                    ?.MakeGenericMethod(type);
                return (bool)isRegisteredNamedMethod?.Invoke(Parent, new object[] { name });
            }

            return false;
        }

        private object ResolveBinding(IBindingConfiguration binding)
        {
            if (binding is IInstanceBindingConfiguration instanceBinding)
                return instanceBinding.Instance;

            if (binding is IFactoryBindingConfiguration factoryBinding)
                return factoryBinding.Factory(this);

            if (binding.Lifetime == Lifetime.Singleton)
            {
                return _singletons.GetOrAdd(binding.ServiceType, _ =>
                {
                    var instance = CreateInstance(binding);
                    InjectDependencies(instance);
                    return instance;
                });
            }

            var pooledInstance = _objectPool.Get(binding.ImplementationType, () =>
            {
                var instance = CreateInstance(binding);
                InjectDependencies(instance);
                return instance;
            });

            return ApplyDecorators(binding.ServiceType, pooledInstance);
        }

        private object CreateInstance(IBindingConfiguration binding)
        {
            if (_resolvingTypes.Contains(binding.ImplementationType))
                throw new InvalidOperationException($"Circular dependency detected for type {binding.ImplementationType}");

            _resolvingTypes.Add(binding.ImplementationType);

            try
            {
                var factory = ReflectionCache.CreateFactory(binding.ImplementationType, Resolve);
                return factory();
            }
            finally
            {
                _resolvingTypes.Remove(binding.ImplementationType);
            }
        }

        private void InjectDependencies(object instance)
        {
            var type = instance.GetType();

            // Inject constructor parameters
            var constructor = ReflectionCache.GetConstructor(type);
            var parameters = constructor.GetParameters();
            var constructorArgs = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var injectAttribute = parameter.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute == null) continue;

                try
                {
                    var value = injectAttribute.Name != null
                        ? ResolveNamed(parameter.ParameterType, injectAttribute.Name)
                        : Resolve(parameter.ParameterType);
                    constructorArgs[i] = value;
                }
                catch (InvalidOperationException) when (injectAttribute.Optional)
                {
                    // Skip optional dependencies that can't be resolved
                    continue;
                }
            }

            // Create instance with constructor parameters
            instance = constructor.Invoke(constructorArgs);

            // Inject properties
            var properties = ReflectionCache.GetInjectableProperties(type);
            foreach (var property in properties)
            {
                var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute == null) continue;

                try
                {
                    var value = injectAttribute.Name != null
                        ? ResolveNamed(property.PropertyType, injectAttribute.Name)
                        : Resolve(property.PropertyType);
                    property.SetValue(instance, value);
                }
                catch (InvalidOperationException) when (injectAttribute.Optional)
                {
                    // Skip optional dependencies that can't be resolved
                    continue;
                }
            }

            // Inject methods
            var methods = ReflectionCache.GetInjectableMethods(type);
            foreach (var method in methods)
            {
                var methodParameters = method.GetParameters();
                var values = new List<object>();
                var hasOptionalDependencies = false;

                foreach (var parameter in methodParameters)
                {
                    var injectAttribute = parameter.GetCustomAttribute<InjectAttribute>();
                    if (injectAttribute == null) continue;

                    try
                    {
                        var value = injectAttribute.Name != null
                            ? ResolveNamed(parameter.ParameterType, injectAttribute.Name)
                            : Resolve(parameter.ParameterType);
                        values.Add(value);
                    }
                    catch (InvalidOperationException) when (injectAttribute.Optional)
                    {
                        hasOptionalDependencies = true;
                        continue;
                    }
                }

                if (!hasOptionalDependencies || values.Count > 0)
                {
                    method.Invoke(instance, values.ToArray());
                }
            }
        }

        private object ApplyDecorators(Type serviceType, object instance)
        {
            if (!_decorators.TryGetValue(serviceType, out var decorators))
                return instance;

            var orderedDecorators = decorators.OrderBy(d => d.Order);
            foreach (var decorator in orderedDecorators)
            {
                var decoratorInstance = CreateInstance(decorator);
                InjectDependencies(decoratorInstance);

                // Set the decorated service on the decorator
                var setMethod = decoratorInstance.GetType().GetMethod("SetAnalyticsService");
                if (setMethod != null)
                {
                    setMethod.Invoke(decoratorInstance, new[] { instance });
                }

                instance = decoratorInstance;
            }

            return instance;
        }
    }

    /// <summary>
    /// Implementation of IScope
    /// </summary>
    internal class Scope : IScope
    {
        private readonly DependencyContainer _container;
        private bool _disposed;

        public IContainer Parent => _container;

        public Scope(DependencyContainer container)
        {
            _container = container;
        }

        public T Resolve<T>() where T : class => _container.Resolve<T>();
        public T ResolveNamed<T>(string name) where T : class => _container.ResolveNamed<T>(name);
        public IEnumerable<T> ResolveAll<T>() where T : class => _container.ResolveAll<T>();
        public bool IsRegistered<T>() where T : class => _container.IsRegistered<T>();
        public bool IsRegisteredNamed<T>(string name) where T : class => _container.IsRegisteredNamed<T>(name);
        public IScope CreateScope() => new Scope(_container);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}