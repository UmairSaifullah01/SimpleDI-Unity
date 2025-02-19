using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        private readonly Dictionary<Type, List<Dependency>> _bindings = new Dictionary<Type, List<Dependency>>(); // Dictionary to store dependency bindings
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>(); // Dictionary to store singleton instances
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>(); // Dictionary to store scoped instances
        private readonly List<object> _transientInstances = new List<object>(); // List to store transient instances

        private readonly object _singletonLock = new object(); // Lock for thread-safe singleton creation

        private DependencyContainer() { }

        /// <summary>
        /// Gets the static dependency container instance.
        /// </summary>
        public static DependencyContainer GlobalContainer => DCExtensionMethods.GetStaticContainer();
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
    }
    
    
}