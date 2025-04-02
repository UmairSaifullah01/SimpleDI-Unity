// using System;
//
// namespace THEBADDEST.SimpleDependencyInjection
// {
//     /// <summary>
//     /// Represents a binding configuration for dependency injection.
//     /// </summary>
//     public class Binding
//     {
//         public DependencyContainer DependencyContainer { get; set; }
//         public DependencyContainer.DependencyFactory Factory { get; set; }
//         public Type InterfaceType { get; set; }
//         public Type ImplementationType { get; set; }
//         public Lifetime Lifetime { get; set; } = Lifetime.Transient;
//
//         /// <summary>
//         /// Registers the binding in the dependency container.
//         /// </summary>
//         public void Bind()
//         {
//             DependencyContainer.Bind(InterfaceType, ImplementationType, Factory, Lifetime);
//         }
//     }
// }