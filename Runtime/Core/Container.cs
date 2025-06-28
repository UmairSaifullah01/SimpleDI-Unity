using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace THEBADDEST.UnityDI
{
    /// <summary>
    /// Defines the lifetime scope of a dependency in the container.
    /// - Transient: A new instance is created every time it's requested
    /// - Singleton: Only one instance is created and shared across all requests
    /// - Scoped: One instance is created per scope (e.g., per scene)
    /// </summary>
    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    /// <summary>
    /// Main dependency injection container that manages object creation, dependency resolution, and lifecycle.
    /// This container supports constructor injection, field injection, property injection, and method injection.
    /// It also handles circular dependencies, conditional bindings, and decorator pattern.
    /// </summary>
    public partial class Container
    {
        /// <summary>
        /// Delegate for custom factory methods that create dependency instances.
        /// This allows for custom instantiation logic when needed.
        /// </summary>
        public delegate object DependencyFactory();

        /// <summary>
        /// Represents a dependency registration with its implementation type, factory method, and lifetime scope.
        /// </summary>
        public struct Dependency
        {
            /// <summary>
            /// The concrete type that implements the dependency
            /// </summary>
            public Type ImplementationType { get; set; }

            /// <summary>
            /// Custom factory method for creating instances, if provided
            /// </summary>
            public DependencyFactory Factory { get; set; }

            /// <summary>
            /// The lifetime scope of the dependency (Transient, Singleton, or Scoped)
            /// </summary>
            public Lifetime Lifetime { get; set; }
        }

        /// <summary>
        /// Represents a conditional dependency registration that is only resolved when its condition is met.
        /// </summary>
        public struct ConditionalDependency
        {
            /// <summary>
            /// The dependency configuration
            /// </summary>
            public Dependency Dependency { get; set; }

            /// <summary>
            /// The condition that must be true for this dependency to be resolved
            /// </summary>
            public Func<bool> Condition { get; set; }
        }

        // Storage for all dependency bindings, grouped by interface type
        private readonly Dictionary<Type, List<Dependency>> _bindings = new Dictionary<Type, List<Dependency>>();

        // Storage for singleton instances
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        // Storage for scoped instances
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();

        // Storage for transient instances (for cleanup purposes)
        private readonly List<object> _transientInstances = new List<object>();

        // Lock object for thread-safe singleton creation
        private readonly object _singletonLock = new object();

        // Tracks types being validated to detect circular dependencies
        private readonly HashSet<Type> _validatingTypes = new HashSet<Type>();

        // Maps types to their dependencies for validation
        private readonly Dictionary<Type, List<Type>> _dependencyGraph = new Dictionary<Type, List<Type>>();

        // Logger for container operations
        private readonly ILogger _logger;

        // Storage for named dependencies (aliases)
        private readonly Dictionary<string, object> _aliases = new Dictionary<string, object>();

        // Storage for conditional bindings
        private readonly Dictionary<Type, List<ConditionalDependency>> _conditionalBindings = new Dictionary<Type, List<ConditionalDependency>>();

        // Tracks disposable instances for cleanup
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        // Current version of the container
        private readonly Version _currentVersion = new Version(1, 0, 0);

        // Storage for decorator types
        private readonly Dictionary<Type, List<Type>> _decorators = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Initializes a new instance of the Container class.
        /// </summary>
        /// <param name="logger">Optional logger for container operations</param>
        public Container(ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
        }

        /// <summary>
        /// Binds an interface to its implementation with optional factory and lifetime settings.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to bind</typeparam>
        /// <typeparam name="TImplementation">The implementation type</typeparam>
        /// <param name="factory">Optional custom factory method for creating instances</param>
        /// <param name="lifetime">The lifetime scope of the dependency (default: Transient)</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container Bind<TInterface, TImplementation>(DependencyFactory factory = null, Lifetime lifetime = Lifetime.Transient)
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            return Bind(typeof(TInterface), typeof(TImplementation), factory, lifetime);
        }

        /// <summary>
        /// Removes a binding for the specified interface type.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to unbind</typeparam>
        /// <returns>The current Container instance for method chaining</returns>
        public Container Unbind<TInterface>()
        {
            return Unbind(typeof(TInterface));
        }

        /// <summary>
        /// Removes a binding for the specified interface type.
        /// </summary>
        /// <param name="interfaceType">The interface type to unbind</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container Unbind(Type interfaceType)
        {
            if (_bindings.TryGetValue(interfaceType, out var dependencies))
            {
                _bindings.Remove(interfaceType);

                foreach (var dependency in dependencies)
                {
                    if (dependency.Lifetime == Lifetime.Singleton)
                    {
                        _singletons.Remove(interfaceType);
                    }
                    else if (dependency.Lifetime == Lifetime.Scoped)
                    {
                        _scopedInstances.Remove(interfaceType);
                    }
                    else if (dependency.Lifetime == Lifetime.Transient)
                    {
                        _transientInstances.RemoveAll(instance => instance.GetType() == dependency.ImplementationType);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Binds an interface to its implementation with optional factory and lifetime settings.
        /// </summary>
        /// <param name="interfaceType">The interface type to bind</param>
        /// <param name="implementationType">The implementation type</param>
        /// <param name="factory">Optional custom factory method for creating instances</param>
        /// <param name="lifetime">The lifetime scope of the dependency (default: Transient)</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container Bind(Type interfaceType, Type implementationType, DependencyFactory factory = null, Lifetime lifetime = Lifetime.Transient)
        {
            // Create a new dependency with provided implementation type, factory, and lifetime
            var dependency = new Dependency
            {
                ImplementationType = implementationType,
                Factory = factory ?? DefaultFactory(implementationType),
                Lifetime = lifetime
            };

            // Initialize the binding list for the interface type if it doesn't exist
            if (!_bindings.ContainsKey(interfaceType))
            {
                _bindings[interfaceType] = new List<Dependency>();
            }

            if (lifetime == Lifetime.Singleton)
            {
                _bindings[interfaceType] = new List<Dependency> { dependency };
            }
            else
            {
                // Add the dependency to the bindings list
                _bindings[interfaceType].Add(dependency);
            }

            // Pre-create singleton instances if the lifetime is Singleton
            if (lifetime == Lifetime.Singleton && !_singletons.ContainsKey(interfaceType))
            {
                _singletons[interfaceType] = dependency.Factory.Invoke();
            }

            return this;
        }

        /// <summary>
        /// Resolves a dependency by its interface type.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to resolve</typeparam>
        /// <returns>The resolved dependency instance</returns>
        public TInterface Resolve<TInterface>() where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        /// <summary>
        /// Resolves a dependency by its interface type and implementation type name.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to resolve</typeparam>
        /// <param name="implementationType">The name of the implementation type</param>
        /// <returns>The resolved dependency instance</returns>
        public TInterface Resolve<TInterface>(string implementationType) where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface), Type.GetType(implementationType));
        }

        /// <summary>
        /// Resolves a dependency by its interface type and implementation type.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to resolve</typeparam>
        /// <param name="implementationType">The implementation type</param>
        /// <returns>The resolved dependency instance</returns>
        public TInterface Resolve<TInterface>(Type implementationType) where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface), implementationType);
        }

        /// <summary>
        /// Internal method to resolve a dependency by its interface type and optional implementation type.
        /// </summary>
        private object Resolve(Type interfaceType, Type implementationType = null)
        {
            // Check for existing singleton instance
            if (_singletons.TryGetValue(interfaceType, out var singletonInstance))
            {
                InjectDependencies(singletonInstance);
                return singletonInstance;
            }

            // Try to find and resolve from bindings
            if (_bindings.TryGetValue(interfaceType, out var dependencies))
            {
                int dependencyIndex = 0;
                if (implementationType != null)
                {
                    dependencyIndex = dependencies.FindIndex(x => x.ImplementationType == implementationType);
                    if (dependencyIndex == -1)
                    {
                        throw new Exception($"No binding found for implementation type '{implementationType}' under interface '{interfaceType}'.");
                    }
                }

                var dependency = dependencies[dependencyIndex];
                object instance;

                // Create instance based on lifetime
                switch (dependency.Lifetime)
                {
                    case Lifetime.Singleton:
                        instance = GetOrCreateSingleton(interfaceType, dependency);
                        break;
                    case Lifetime.Scoped:
                        instance = GetOrCreateScopedInstance(interfaceType, dependency);
                        break;
                    default: // Transient
                        instance = dependency.Factory.Invoke();
                        _transientInstances.Add(instance);
                        break;
                }

                InjectDependencies(instance);
                return instance;
            }

            throw new Exception($"Unable to resolve dependency for type '{interfaceType}'. Available bindings: {string.Join(", ", _bindings.Keys)}");
        }

        /// <summary>
        /// Gets or creates a singleton instance for the specified interface type.
        /// </summary>
        private object GetOrCreateSingleton(Type interfaceType, Dependency dependency)
        {
            lock (_singletonLock)
            {
                if (!_singletons.TryGetValue(interfaceType, out var instance))
                {
                    instance = dependency.Factory.Invoke();
                    _singletons[interfaceType] = instance;
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets or creates a scoped instance for the specified interface type.
        /// </summary>
        private object GetOrCreateScopedInstance(Type interfaceType, Dependency dependency)
        {
            if (!_scopedInstances.TryGetValue(interfaceType, out var instance))
            {
                instance = dependency.Factory.Invoke();
                _scopedInstances[interfaceType] = instance;
            }
            return instance;
        }

        /// <summary>
        /// Injects dependencies into the specified target object.
        /// </summary>
        /// <param name="target">The target object to inject dependencies into</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container InjectDependencies(object target)
        {
            InjectFields(target);
            InjectProperties(target);
            InjectMethods(target);
            return this;
        }

        /// <summary>
        /// Injects dependencies into fields marked with [Inject] attribute.
        /// </summary>
        private void InjectFields(object target)
        {
            var targetType = target.GetType();
            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>() != null)
                {
                    var fieldType = field.FieldType;
                    try
                    {
                        if (fieldType.IsArray || (fieldType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(fieldType)))
                        {
                            Type elementType = fieldType.IsArray ? fieldType.GetElementType() : fieldType.GetGenericArguments()[0];
                            var resolvedArray = ResolveArray(elementType);

                            if (fieldType.IsArray)
                            {
                                Array array = Array.CreateInstance(elementType, resolvedArray.Length);
                                for (int i = 0; i < resolvedArray.Length; i++)
                                {
                                    array.SetValue(resolvedArray[i], i);
                                }
                                field.SetValue(target, array);
                            }
                            else
                            {
                                var listType = typeof(List<>).MakeGenericType(elementType);
                                var list = Activator.CreateInstance(listType, resolvedArray);
                                field.SetValue(target, list);
                            }
                        }
                        else
                        {
                            var resolvedDependency = Resolve(fieldType);
                            field.SetValue(target, resolvedDependency);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning($"Failed to inject dependency into field '{field.Name}' on object of type '{targetType}'. Error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Injects dependencies into properties marked with [Inject] attribute.
        /// </summary>
        private void InjectProperties(object target)
        {
            var type = target.GetType();
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<InjectAttribute>() != null && property.CanWrite)
                {
                    try
                    {
                        var value = Resolve(property.PropertyType);
                        property.SetValue(target, value);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning($"Failed to inject dependency into property '{property.Name}' on object of type '{type}'. Error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Injects dependencies into methods marked with [Inject] attribute.
        /// </summary>
        private void InjectMethods(object target)
        {
            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    try
                    {
                        var parameters = method.GetParameters();
                        var args = new object[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            args[i] = Resolve(parameters[i].ParameterType);
                        }

                        method.Invoke(target, args);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning($"Failed to inject dependencies into method '{method.Name}' on object of type '{type}'. Error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Resolves an array of dependencies for the specified element type.
        /// </summary>
        private object[] ResolveArray(Type elementType)
        {
            var bindings = _bindings.Where(kvp => elementType.IsAssignableFrom(kvp.Key)).ToList();
            return bindings.Select(binding => Resolve(binding.Key)).ToArray();
        }

        /// <summary>
        /// Creates the default factory method for a type.
        /// </summary>
        private DependencyFactory DefaultFactory(Type type)
        {
            return () => CreateInstanceWithConstructorInjection(type);
        }

        /// <summary>
        /// Creates an instance of the specified type with constructor injection.
        /// </summary>
        private object CreateInstanceWithConstructorInjection(Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = Resolve(parameters[i].ParameterType);
            }

            return constructor.Invoke(args);
        }

        /// <summary>
        /// Validates all registered dependencies for circular dependencies and other issues.
        /// </summary>
        public void ValidateDependencies()
        {
            _validatingTypes.Clear();
            _dependencyGraph.Clear();

            foreach (var binding in _bindings)
            {
                ValidateDependency(binding.Key);
            }
        }

        /// <summary>
        /// Validates a specific dependency for circular dependencies and other issues.
        /// </summary>
        private void ValidateDependency(Type type)
        {
            if (_validatingTypes.Contains(type))
            {
                throw new CircularDependencyException($"Circular dependency detected for type {type.Name}");
            }

            _validatingTypes.Add(type);

            if (_bindings.TryGetValue(type, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    var implementationType = dependency.ImplementationType;
                    ValidateDependency(implementationType);

                    if (!_dependencyGraph.ContainsKey(type))
                    {
                        _dependencyGraph[type] = new List<Type>();
                    }
                    _dependencyGraph[type].Add(implementationType);
                }
            }

            _validatingTypes.Remove(type);
        }

        /// <summary>
        /// Exception thrown when a circular dependency is detected.
        /// </summary>
        public class CircularDependencyException : Exception
        {
            public CircularDependencyException(string message) : base(message) { }
        }

        /// <summary>
        /// Registers an alias for a dependency instance.
        /// </summary>
        /// <typeparam name="T">The type of the dependency</typeparam>
        /// <param name="alias">The alias name</param>
        /// <param name="instance">The instance to register</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container WithAlias<T>(string alias, T instance)
        {
            _aliases[alias] = instance;
            return this;
        }

        /// <summary>
        /// Registers a conditional binding that is only resolved when the condition is true.
        /// </summary>
        /// <typeparam name="TInterface">The interface type</typeparam>
        /// <typeparam name="TImplementation">The implementation type</typeparam>
        /// <param name="condition">The condition that must be true for this binding to be used</param>
        /// <param name="factory">Optional custom factory method</param>
        /// <param name="lifetime">The lifetime scope of the dependency</param>
        /// <returns>The current Container instance for method chaining</returns>
        public Container BindConditional<TInterface, TImplementation>(
            Func<bool> condition,
            DependencyFactory factory = null,
            Lifetime lifetime = Lifetime.Transient)
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            var dependency = new Dependency
            {
                ImplementationType = typeof(TImplementation),
                Factory = factory ?? DefaultFactory(typeof(TImplementation)),
                Lifetime = lifetime
            };

            var conditionalDependency = new ConditionalDependency
            {
                Dependency = dependency,
                Condition = condition
            };

            if (!_conditionalBindings.ContainsKey(typeof(TInterface)))
            {
                _conditionalBindings[typeof(TInterface)] = new List<ConditionalDependency>();
            }

            _conditionalBindings[typeof(TInterface)].Add(conditionalDependency);
            return this;
        }

        /// <summary>
        /// Resolves a dependency with conditional bindings.
        /// </summary>
        /// <typeparam name="T">The interface type to resolve</typeparam>
        /// <param name="alias">Optional alias for the dependency</param>
        /// <returns>The resolved dependency instance</returns>
        public T ResolveWithCondition<T>(string alias = null) where T : class
        {
            if (!string.IsNullOrEmpty(alias) && _aliases.TryGetValue(alias, out var aliasedInstance))
            {
                return aliasedInstance as T;
            }

            var type = typeof(T);
            if (_conditionalBindings.TryGetValue(type, out var conditionals))
            {
                foreach (var conditional in conditionals)
                {
                    if (conditional.Condition())
                    {
                        return ResolveDependency(conditional.Dependency) as T;
                    }
                }
            }

            return Resolve<T>();
        }

        /// <summary>
        /// Loads configuration from a ScriptableObject.
        /// </summary>
        /// <typeparam name="T">The type of the configuration</typeparam>
        /// <param name="config">The configuration object</param>
        public void LoadConfiguration<T>(T config) where T : ScriptableObject
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType.IsInterface)
                {
                    var value = property.GetValue(config);
                    if (value != null)
                    {
                        WithAlias(property.Name, value);
                    }
                }
            }
        }

        /// <summary>
        /// Validates version compatibility with the container.
        /// </summary>
        /// <param name="requiredVersion">The required version</param>
        public void ValidateVersion(Version requiredVersion)
        {
            if (requiredVersion > _currentVersion)
            {
                throw new VersionCompatibilityException(
                    $"Required version {requiredVersion} is higher than current version {_currentVersion}");
            }
        }

        /// <summary>
        /// Exception thrown when version compatibility check fails.
        /// </summary>
        public class VersionCompatibilityException : Exception
        {
            public VersionCompatibilityException(string message) : base(message) { }
        }

        /// <summary>
        /// Registers a disposable instance for cleanup.
        /// </summary>
        /// <param name="disposable">The disposable instance to register</param>
        public void RegisterDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Cleans up all registered disposable instances and clears the container.
        /// </summary>
        public void Cleanup()
        {
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error disposing {disposable.GetType().Name}: {ex.Message}");
                }
            }

            _disposables.Clear();
            _singletons.Clear();
            _scopedInstances.Clear();
            _transientInstances.Clear();
            _bindings.Clear();
            _aliases.Clear();
            _conditionalBindings.Clear();
            _decorators.Clear();
        }

        /// <summary>
        /// Resolves a dependency using the provided dependency configuration.
        /// </summary>
        private object ResolveDependency(Dependency dependency)
        {
            try
            {
                var instance = dependency.Factory.Invoke();

                if (instance is IDisposable disposable)
                {
                    RegisterDisposable(disposable);
                }

                // Apply decorators if any
                if (_decorators.ContainsKey(dependency.ImplementationType))
                {
                    instance = ApplyDecorators(dependency.ImplementationType, instance);
                }

                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error resolving dependency {dependency.ImplementationType.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Registers a decorator for the specified interface type.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to decorate</typeparam>
        /// <typeparam name="TDecorator">The decorator type</typeparam>
        /// <returns>The current Container instance for method chaining</returns>
        public Container Decorate<TInterface, TDecorator>()
            where TInterface : class
            where TDecorator : class, TInterface
        {
            var interfaceType = typeof(TInterface);
            if (!_decorators.ContainsKey(interfaceType))
            {
                _decorators[interfaceType] = new List<Type>();
            }

            _decorators[interfaceType].Add(typeof(TDecorator));
            return this;
        }

        /// <summary>
        /// Applies all registered decorators to an instance.
        /// </summary>
        private object ApplyDecorators(Type interfaceType, object instance)
        {
            if (!_decorators.TryGetValue(interfaceType, out var decoratorTypes))
            {
                return instance;
            }

            object decoratedInstance = instance;
            foreach (var decoratorType in decoratorTypes)
            {
                try
                {
                    var constructor = decoratorType.GetConstructors()
                        .OrderByDescending(c => c.GetParameters().Length)
                        .First();

                    var parameters = constructor.GetParameters();
                    var resolvedParameters = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (parameter.ParameterType == interfaceType)
                        {
                            resolvedParameters[i] = decoratedInstance;
                        }
                        else
                        {
                            resolvedParameters[i] = Resolve(parameter.ParameterType);
                        }
                    }

                    decoratedInstance = constructor.Invoke(resolvedParameters);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to apply decorator {decoratorType.Name} to {interfaceType.Name}. Error: {ex.Message}");
                    return instance;
                }
            }

            return decoratedInstance;
        }
    }
}