using System;

namespace THEBADDEST.UnityDI
{
    /// <summary>
    /// Represents a binding configuration for dependency injection.
    /// </summary>
    public class Binding
    {
        public Container Container { get; set; }
        public Container.DependencyFactory Factory { get; set; }
        public Type InterfaceType { get; set; }
        public Type ImplementationType { get; set; }
        public Lifetime Lifetime { get; set; } = Lifetime.Transient;

        /// <summary>
        /// Registers the binding in the container.
        /// </summary>
        public void Bind()
        {
            Container.Bind(InterfaceType, ImplementationType, Factory, Lifetime);
        }
    }
}