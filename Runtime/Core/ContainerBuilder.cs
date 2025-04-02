using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Builder class for configuring and creating dependency injection containers
    /// </summary>
    public class ContainerBuilder
    {
        private readonly List<IBindingConfiguration> _bindings = new();
        private readonly Dictionary<Type, List<IDecoratorBindingConfiguration>> _decorators = new();
        private readonly Dictionary<Type, List<ICollectionBindingConfiguration>> _collections = new();
        private readonly HashSet<Type> _registeredTypes = new();
        private readonly HashSet<Type> _registeredDecorators = new();
        private readonly HashSet<Type> _registeredCollections = new();

        /// <summary>
        /// Registers a type binding
        /// </summary>
        public ContainerBuilder Register<TService, TImplementation>(Lifetime lifetime = Lifetime.Transient, string name = null)
            where TService : class
            where TImplementation : class, TService
        {
            var binding = new TypeBindingConfiguration(
                typeof(TService),
                typeof(TImplementation),
                lifetime,
                name
            );
            _bindings.Add(binding);
            _registeredTypes.Add(typeof(TService));
            return this;
        }

        /// <summary>
        /// Registers a factory binding
        /// </summary>
        public ContainerBuilder RegisterFactory<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient, string name = null)
            where TService : class
        {
            var binding = new FactoryBindingConfiguration(
                typeof(TService),
                factory,
                lifetime,
                name
            );
            _bindings.Add(binding);
            _registeredTypes.Add(typeof(TService));
            return this;
        }

        /// <summary>
        /// Registers an instance binding
        /// </summary>
        public ContainerBuilder RegisterInstance<TService>(TService instance, string name = null)
            where TService : class
        {
            var binding = new InstanceBindingConfiguration(
                typeof(TService),
                instance,
                name
            );
            _bindings.Add(binding);
            _registeredTypes.Add(typeof(TService));
            return this;
        }

        /// <summary>
        /// Registers a decorator
        /// </summary>
        public ContainerBuilder RegisterDecorator<TService, TDecorator>(int order = 0)
            where TService : class
            where TDecorator : class, TService
        {
            var decorator = new DecoratorBindingConfiguration(
                typeof(TService),
                typeof(TDecorator),
                order
            );

            if (!_decorators.ContainsKey(typeof(TService)))
            {
                _decorators[typeof(TService)] = new List<IDecoratorBindingConfiguration>();
            }

            _decorators[typeof(TService)].Add(decorator);
            _registeredDecorators.Add(typeof(TDecorator));
            return this;
        }

        /// <summary>
        /// Registers a collection binding
        /// </summary>
        public ContainerBuilder RegisterCollection<TService, TImplementation>(bool allowEmpty = false)
            where TService : class
            where TImplementation : class, TService
        {
            var collection = new CollectionBindingConfiguration(
                typeof(TService),
                typeof(TImplementation),
                allowEmpty
            );

            if (!_collections.ContainsKey(typeof(TService)))
            {
                _collections[typeof(TService)] = new List<ICollectionBindingConfiguration>();
            }

            _collections[typeof(TService)].Add(collection);
            _registeredCollections.Add(typeof(TImplementation));
            return this;
        }

        /// <summary>
        /// Builds and returns a new container instance
        /// </summary>
        public IContainer Build()
        {
            ValidateBindings();
            return new DependencyContainer(
                _bindings,
                _decorators,
                _collections,
                _registeredTypes,
                _registeredDecorators,
                _registeredCollections
            );
        }

        private void ValidateBindings()
        {
            // Validate circular dependencies
            ValidateCircularDependencies();

            // Validate decorator chain
            ValidateDecoratorChain();

            // Validate collection bindings
            ValidateCollectionBindings();
        }

        private void ValidateCircularDependencies()
        {
            // Implementation for circular dependency detection
        }

        private void ValidateDecoratorChain()
        {
            // Implementation for decorator chain validation
        }

        private void ValidateCollectionBindings()
        {
            // Implementation for collection binding validation
        }
    }
}