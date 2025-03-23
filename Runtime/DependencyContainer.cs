using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


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
    public class DependencyContainer
    {
        /// <summary>
        /// Delegate for creating dependency instances.
        /// </summary>
        public delegate object DependencyFactory();

        /// <summary>
        /// Struct representing a dependency registration.
        /// </summary>
        public struct Dependency
        {
            public Type ImplementationType { get; set; } // The implementation type of the dependency
            public DependencyFactory Factory { get; set; } // The factory method for creating instances
            public Lifetime Lifetime { get; set; } // The lifetime of the dependency
        }

        /// <summary>
        /// Struct representing a conditional dependency registration.
        /// </summary>
        public struct ConditionalDependency
        {
            public Dependency Dependency { get; set; }
            public Func<bool> Condition { get; set; }
        }

        private readonly Dictionary<Type, List<Dependency>> _bindings = new Dictionary<Type, List<Dependency>>(); // Dictionary to store dependency bindings
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>(); // Dictionary to store singleton instances
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>(); // Dictionary to store scoped instances
        private readonly List<object> _transientInstances = new List<object>(); // List to store transient instances

        private readonly object _singletonLock = new object(); // Lock for thread-safe singleton creation

        private readonly HashSet<Type> _validatingTypes = new HashSet<Type>();
        private readonly Dictionary<Type, List<Type>> _dependencyGraph = new Dictionary<Type, List<Type>>();
        private readonly ILogger _logger;

        private readonly Dictionary<string, object> _aliases = new Dictionary<string, object>();
        private readonly Dictionary<Type, List<ConditionalDependency>> _conditionalBindings = new Dictionary<Type, List<ConditionalDependency>>();

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly Version _currentVersion = new Version(1, 0, 0);

        private readonly Dictionary<Type, List<Type>> _decorators = new Dictionary<Type, List<Type>>();

        public DependencyContainer(ILogger logger = null)
        {
            _logger = logger ?? new UnityLogger();
        }

        /// <summary>
        /// Gets the static dependency container instance.
        /// </summary>
        public static DependencyContainer Global => DCExtensionMethods.GetStaticContainer();
        /// <summary>
        /// Creates a new instance of DependencyContainer.
        /// </summary>
        /// <returns>The created DependencyContainer instance.</returns>
        public static DependencyContainer Create()
        {
            return new DependencyContainer();
        }

        /// <summary>
        /// Binds an interface to an implementation with optional factory and lifetime settings.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="factory">Custom factory method for creating instances (optional).</param>
        /// <param name="lifetime">The lifetime of the dependency (default: Transient).</param>
        /// <returns>The current DependencyContainer instance.</returns>
        public DependencyContainer Bind<TInterface, TImplementation>(DependencyFactory factory = null, Lifetime lifetime = Lifetime.Transient)
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            return Bind(typeof(TInterface), typeof(TImplementation), factory, lifetime);
        }


        /// <summary>
        /// Removes the binding of an interface to an implementation.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <returns>The current DependencyContainer instance.</returns>
        public DependencyContainer Unbind<TInterface>()
        {
            return Unbind(typeof(TInterface));
        }

        /// <summary>
        /// Removes the binding of an interface to an implementation.
        /// </summary>
        /// <param name="interfaceType">The interface type to unbind.</param>
        /// <returns>The current DependencyContainer instance.</returns>
        public DependencyContainer Unbind(Type interfaceType)
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
        /// Binds an interface to an implementation with optional factory and lifetime settings.
        /// </summary>
        /// <param name="interfaceType">The interface type to bind.</param>
        /// <param name="implementationType">The implementation type to bind to the interface.</param>
        /// <param name="factory">Custom factory method for creating instances (optional).</param>
        /// <param name="lifetime">The lifetime of the dependency (default: Transient).</param>
        /// <returns>The current DependencyContainer instance.</returns>
        public DependencyContainer Bind(Type interfaceType, Type implementationType, DependencyFactory factory = null, Lifetime lifetime = Lifetime.Transient)
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
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <returns>The resolved dependency instance.</returns>
        public TInterface Resolve<TInterface>() where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public TInterface Resolve<TInterface>(string implementationType) where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface), Type.GetType(implementationType));
        }

        public TInterface Resolve<TInterface>(Type implementationType) where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface), implementationType);
        }

        private object Resolve(Type interfaceType, Type implementationType = null)
        {
            if (_singletons.TryGetValue(interfaceType, out var singletonInstance))
            {
                InjectDependencies(singletonInstance);
                return singletonInstance;
            }

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
        /// Injects dependencies into fields, properties, and methods marked with the [Inject] attribute on the target object.
        /// </summary>
        /// <param name="target">The target object to inject dependencies into.</param>
        /// <returns>The current DependencyContainer instance.</returns>
        public DependencyContainer InjectDependencies(object target)
        {
            InjectFields(target);
            InjectProperties(target);
            InjectMethods(target);
            return this;
        }

        /// <summary>
        /// Injects dependencies into fields marked with the [Inject] attribute.
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
                        Debug.LogError($"Failed to inject dependency into field '{field.Name}' on object of type '{targetType}'. Error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Injects dependencies into properties marked with the [Inject] attribute.
        /// </summary>
        private void InjectProperties(object target)
        {
            var targetType = target.GetType();
            var properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<InjectAttribute>() != null)
                {
                    var propertyType = property.PropertyType;
                    try
                    {
                        var resolvedDependency = Resolve(propertyType);
                        property.SetValue(target, resolvedDependency);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to inject dependency into property '{property.Name}' on object of type '{targetType}'. Error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Injects dependencies into methods marked with the [Inject] attribute.
        /// </summary>
        private void InjectMethods(object target)
        {
            var targetType = target.GetType();
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    var parameters = method.GetParameters();
                    var resolvedParameters = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        resolvedParameters[i] = Resolve(parameters[i].ParameterType);
                    }

                    try
                    {
                        method.Invoke(target, resolvedParameters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to inject dependencies into method '{method.Name}' on object of type '{targetType}'. Error: {e.Message}");
                    }
                }
            }
        }

        private object[] ResolveArray(Type elementType)
        {
            if (_bindings.TryGetValue(elementType, out var dependencies))
            {
                return dependencies.Select(dependency => Resolve(elementType, dependency.ImplementationType)).ToArray();
            }
            throw new Exception($"Unable to resolve array of type '{elementType}'. No bindings found.");
        }

        private DependencyFactory DefaultFactory(Type type)
        {
            return () => CreateInstanceWithConstructorInjection(type);
        }

        private object CreateInstanceWithConstructorInjection(Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new Exception($"No public constructor found for type '{type}'.");
            }

            var constructor = constructors[0]; // Use the first constructor (can be improved to choose the most suitable one)
            var parameters = constructor.GetParameters();
            var resolvedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                resolvedParameters[i] = Resolve(parameters[i].ParameterType);
            }

            return constructor.Invoke(resolvedParameters);
        }

        public void ValidateDependencies()
        {
            _validatingTypes.Clear();
            _dependencyGraph.Clear();

            foreach (var binding in _bindings)
            {
                ValidateDependency(binding.Key);
            }
        }

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

        public class CircularDependencyException : Exception
        {
            public CircularDependencyException(string message) : base(message) { }
        }

        public DependencyContainer WithAlias<T>(string alias, T instance)
        {
            _aliases[alias] = instance;
            return this;
        }

        public DependencyContainer BindConditional<TInterface, TImplementation>(
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

        public void ValidateVersion(Version requiredVersion)
        {
            if (requiredVersion > _currentVersion)
            {
                throw new VersionCompatibilityException(
                    $"Required version {requiredVersion} is higher than current version {_currentVersion}");
            }
        }

        public class VersionCompatibilityException : Exception
        {
            public VersionCompatibilityException(string message) : base(message) { }
        }

        public void RegisterDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

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
                    _logger.LogError($"Error disposing {disposable.GetType().Name}: {ex.Message}");
                }
            }

            _disposables.Clear();
            _singletons.Clear();
            _scopedInstances.Clear();
            _transientInstances.Clear();
            _bindings.Clear();
            _aliases.Clear();
            _conditionalBindings.Clear();
        }

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
                _logger.LogError($"Error resolving dependency {dependency.ImplementationType.Name}: {ex.Message}");
                throw;
            }
        }

        public DependencyContainer Decorate<TInterface, TDecorator>()
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

        private object ApplyDecorators(Type interfaceType, object instance)
        {
            if (!_decorators.TryGetValue(interfaceType, out var decoratorTypes))
            {
                return instance;
            }

            object decoratedInstance = instance;
            foreach (var decoratorType in decoratorTypes)
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

            return decoratedInstance;
        }
    }


}