using System;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Attribute used to mark dependencies for injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Optional name for named dependencies
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether the dependency is optional
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// Creates a new InjectAttribute
        /// </summary>
        /// <param name="name">Optional name for named dependencies</param>
        /// <param name="optional">Whether the dependency is optional</param>
        public InjectAttribute(string name = null, bool optional = false)
        {
            Name = name;
            Optional = optional;
        }
    }
}