﻿using System;
using UnityEngine;

namespace THEBADDEST.UnityDI
{
	/// <summary>
	/// Extension methods for DependencyContainer to provide a fluent API.
	/// </summary>
	public static class ContainerExtensions
	{
		private static readonly object _containerLock = new object();
		private static Container _container;
		private static InjectableObjectTracker injectableObjectTracker;

		

		/// <summary>
		/// Binds an interface to an implementation using a custom factory.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <typeparam name="TImplementation">The implementation type.</typeparam>
		/// <param name="dc">The dependency container.</param>
		/// <param name="factory">The factory method for creating instances.</param>
		/// <returns>The dependency container.</returns>
		public static Container BindFactory<TInterface, TImplementation>(this Container dc, Container.DependencyFactory factory) where TInterface : class where TImplementation : class, TInterface, new()
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
		public static Binding Bind<TInterface>(this Container dc) where TInterface : class
		{
			return new Binding { Container = dc, InterfaceType = typeof(TInterface) };
		}

		/// <summary>
		/// Specifies the implementation type for the binding.
		/// </summary>
		/// <typeparam name="TImplementation">The implementation type.</typeparam>
		/// <param name="binding">The binding configuration.</param>
		/// <param name="factory">The factory method for creating instances (optional).</param>
		/// <returns>The binding configuration.</returns>
		public static Binding To<TImplementation>(this Binding binding, Container.DependencyFactory factory = null) where TImplementation : class
		{
			binding.ImplementationType = typeof(TImplementation);
			binding.Factory = factory;
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
		
	}
}