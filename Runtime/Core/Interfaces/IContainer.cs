using System;
using System.Collections.Generic;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Core interface for dependency injection container
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Resolves an instance of the specified type
        /// </summary>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolves an instance of the specified type with the given name
        /// </summary>
        T ResolveNamed<T>(string name) where T : class;

        /// <summary>
        /// Resolves all registered implementations of the specified type
        /// </summary>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Checks if the specified type is registered
        /// </summary>
        bool IsRegistered<T>() where T : class;

        /// <summary>
        /// Checks if the specified type is registered with the given name
        /// </summary>
        bool IsRegisteredNamed<T>(string name) where T : class;

        /// <summary>
        /// Creates a new scope
        /// </summary>
        IScope CreateScope();

        /// <summary>
        /// Gets the parent container if this is a scoped container
        /// </summary>
        IContainer Parent { get; }
    }

    /// <summary>
    /// Represents a scoped container
    /// </summary>
    public interface IScope : IContainer
    {
        /// <summary>
        /// Disposes the scope and all its scoped instances
        /// </summary>
        void Dispose();
    }
}