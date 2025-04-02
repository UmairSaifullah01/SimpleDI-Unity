using System;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Base interface for binding configurations
    /// </summary>
    public interface IBindingConfiguration
    {
        Type ServiceType { get; }
        Type ImplementationType { get; }
        Lifetime Lifetime { get; }
        string Name { get; }
        bool IsConditional { get; }
    }

    /// <summary>
    /// Interface for factory binding configurations
    /// </summary>
    public interface IFactoryBindingConfiguration : IBindingConfiguration
    {
        Func<IContainer, object> Factory { get; }
    }

    /// <summary>
    /// Interface for instance binding configurations
    /// </summary>
    public interface IInstanceBindingConfiguration : IBindingConfiguration
    {
        object Instance { get; }
    }

    /// <summary>
    /// Interface for decorator binding configurations
    /// </summary>
    public interface IDecoratorBindingConfiguration : IBindingConfiguration
    {
        Type DecoratorType { get; }
        int Order { get; }
    }

    /// <summary>
    /// Interface for collection binding configurations
    /// </summary>
    public interface ICollectionBindingConfiguration : IBindingConfiguration
    {
        Type CollectionType { get; }
        bool AllowEmpty { get; }
    }
}