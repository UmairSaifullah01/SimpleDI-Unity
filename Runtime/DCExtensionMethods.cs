using System;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	/// <summary>
	/// Represents a binding configuration for dependency injection.
	/// </summary>
	public class Binding
	{

		public DependencyContainer                   DependencyContainer { get; set; }
		public DependencyContainer.DependencyFactory Factory             { get; set; }
		public Type                                  InterfaceType       { get; set; }
		public Type                                  ImplementationType  { get; set; }
		public Lifetime                              Lifetime            { get; set; } = Lifetime.Transient;

		/// <summary>
		/// Registers the binding in the dependency container.
		/// </summary>
		public void Bind()
		{
			DependencyContainer.Bind(InterfaceType, ImplementationType, Factory, Lifetime);
		}

	}

	/// <summary>
	/// Extension methods for DependencyContainer to provide a fluent API.
	/// </summary>
	public static class DCExtensionMethods
	{

		private static readonly object              _containerLock = new object();
		private static          DependencyContainer _container;

		private static InjectableObjectTracker injectableObjectTracker;

		static DCExtensionMethods()
		{
			_container              = DependencyContainer.Create();
			injectableObjectTracker = new InjectableObjectTracker();
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Preload()
		{
			// Force the static constructor of DCExtensionMethods to run
			var container = DCExtensionMethods.GetStaticContainer();
			var tracker   = DCExtensionMethods.GetStaticIOTracker();
		}
		/// <summary>
		/// Binds an interface to an implementation using a custom factory.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <typeparam name="TImplementation">The implementation type.</typeparam>
		/// <param name="dc">The dependency container.</param>
		/// <param name="factory">The factory method for creating instances.</param>
		/// <returns>The dependency container.</returns>
		public static DependencyContainer BindFactory<TInterface, TImplementation>(this DependencyContainer dc, DependencyContainer.DependencyFactory factory) where TInterface : class where TImplementation : class, TInterface, new()
		{
			dc.Bind<TInterface, TImplementation>(factory, Lifetime.Transient);
			return dc;
		}

		/// <summary>
		/// Starts a binding configuration for the specified interface type.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <param name="dc">The dependency container.</param>
		/// <returns>The binding configuration.</returns>
		public static Binding Bind<TInterface>(this DependencyContainer dc) where TInterface : class
		{
			return new Binding { DependencyContainer = dc, InterfaceType = typeof(TInterface) };
		}

		/// <summary>
		/// Specifies the implementation type for the binding.
		/// </summary>
		/// <typeparam name="TImplementation">The implementation type.</typeparam>
		/// <param name="binding">The binding configuration.</param>
		/// <param name="factory">The factory method for creating instances (optional).</param>
		/// <returns>The binding configuration.</returns>
		public static Binding To<TImplementation>(this Binding binding, DependencyContainer.DependencyFactory factory = null) where TImplementation : class
		{
			binding.ImplementationType = typeof(TImplementation);
			binding.Factory            = factory;
			return binding;
		}

		/// <summary>
		/// Specifies the lifetime of the binding.
		/// </summary>
		/// <param name="binding">The binding configuration.</param>
		/// <param name="lifetime">The lifetime of the dependency (default: Singleton).</param>
		public static void WithLifetime(this Binding binding, Lifetime lifetime = Lifetime.Singleton)
		{
			binding.Lifetime = lifetime;
			binding.Bind();
		}

		/// <summary>
		/// Gets the static dependency container instance.
		/// </summary>
		/// <returns>The static dependency container.</returns>
		public static DependencyContainer GetStaticContainer()
		{
			lock (_containerLock)
			{
				return _container;
			}
		}

		/// <summary>
		/// Gets the static instance of the InjectableObjectTracker.
		/// </summary>
		/// <returns>The static InjectableObjectTracker instance.</returns>
		public static InjectableObjectTracker GetStaticIOTracker()
		{
			return injectableObjectTracker;
		}

	}


}