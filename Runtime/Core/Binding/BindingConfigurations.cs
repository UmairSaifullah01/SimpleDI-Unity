using System;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Base class for binding configurations
    /// </summary>
    public abstract class BindingConfigurationBase : IBindingConfiguration
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Lifetime Lifetime { get; }
        public string Name { get; }
        public bool IsConditional { get; }

        protected BindingConfigurationBase(Type serviceType, Type implementationType, Lifetime lifetime, string name = null, bool isConditional = false)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
            Name = name;
            IsConditional = isConditional;
        }
    }

    /// <summary>
    /// Configuration for type-based bindings
    /// </summary>
    public class TypeBindingConfiguration : BindingConfigurationBase
    {
        public TypeBindingConfiguration(Type serviceType, Type implementationType, Lifetime lifetime, string name = null)
            : base(serviceType, implementationType, lifetime, name)
        {
        }
    }

    /// <summary>
    /// Configuration for factory-based bindings
    /// </summary>
    public class FactoryBindingConfiguration : BindingConfigurationBase, IFactoryBindingConfiguration
    {
        public Func<IContainer, object> Factory { get; }

        public FactoryBindingConfiguration(Type serviceType, Func<IContainer, object> factory, Lifetime lifetime, string name = null)
            : base(serviceType, serviceType, lifetime, name)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }
    }

    /// <summary>
    /// Configuration for instance-based bindings
    /// </summary>
    public class InstanceBindingConfiguration : BindingConfigurationBase, IInstanceBindingConfiguration
    {
        public object Instance { get; }

        public InstanceBindingConfiguration(Type serviceType, object instance, string name = null)
            : base(serviceType, serviceType, Lifetime.Singleton, name)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }
    }

    /// <summary>
    /// Configuration for decorator-based bindings
    /// </summary>
    public class DecoratorBindingConfiguration : BindingConfigurationBase, IDecoratorBindingConfiguration
    {
        public Type DecoratorType { get; }
        public int Order { get; }

        public DecoratorBindingConfiguration(Type serviceType, Type decoratorType, int order)
            : base(serviceType, decoratorType, Lifetime.Transient)
        {
            DecoratorType = decoratorType ?? throw new ArgumentNullException(nameof(decoratorType));
            Order = order;
        }
    }

    /// <summary>
    /// Configuration for collection-based bindings
    /// </summary>
    public class CollectionBindingConfiguration : BindingConfigurationBase, ICollectionBindingConfiguration
    {
        public Type CollectionType { get; }
        public bool AllowEmpty { get; }

        public CollectionBindingConfiguration(Type serviceType, Type implementationType, bool allowEmpty)
            : base(serviceType, implementationType, Lifetime.Transient)
        {
            CollectionType = typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(serviceType);
            AllowEmpty = allowEmpty;
        }
    }
}