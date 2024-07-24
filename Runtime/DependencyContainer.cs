using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


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

			public Type              ImplementationType { get; set; } // The implementation type of the dependency
			public DependencyFactory Factory            { get; set; } // The factory method for creating instances
			public bool              IsSingleton        { get; set; } // Flag indicating if the dependency should be treated as a singleton

		}

		private Dictionary<Type, List<Dependency>> _bindings   = new Dictionary<Type, List<Dependency>>(); // Dictionary to store dependency bindings
		private Dictionary<Type, object>           _singletons = new Dictionary<Type, object>();           // Dictionary to store singleton instances
		private List<object>                       _instances  = new List<object>();                       // List to store instances

		private DependencyContainer()
		{
		}

		/// <summary>
		/// Creates a new instance of DependencyContainer.
		/// </summary>
		/// <returns>The created DependencyContainer instance.</returns>
		public static DependencyContainer Create()
		{
			return new DependencyContainer();
		}

		/// <summary>
		/// Creates a static instance of DependencyContainer.
		/// </summary>
		/// <returns>The created DependencyContainer instance.</returns>
		public static DependencyContainer CreateStatic()
		{
			return DCExtensionMethods.GetStaticContainer();
		}

		/// <summary>
		/// Binds an interface to an implementation with optional factory and singleton settings.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <typeparam name="TImplementation">The implementation type.</typeparam>
		/// <param name="factory">Custom factory method for creating instances (optional).</param>
		/// <param name="singleInstance">Flag indicating if the dependency should be treated as a singleton.</param>
		/// <returns>The current DependencyContainer instance.</returns>
		public DependencyContainer Bind<TInterface, TImplementation>(DependencyFactory factory = null, bool singleInstance = false) where TInterface : class where TImplementation : class, TInterface, new()
		{
			return Bind(typeof(TInterface), typeof(TImplementation), factory, singleInstance);
		}

		public DependencyContainer Bind(Type interfaceType, Type implementationType, DependencyFactory factory = null, bool singleInstance = false)
		{
			var dependency = new Dependency { ImplementationType = implementationType, IsSingleton = singleInstance, Factory = factory ?? DefaultFactory(implementationType) };
			if (!_bindings.ContainsKey(interfaceType))
			{
				_bindings.Add(interfaceType, new List<Dependency>());
			}

			if (_bindings[interfaceType] == null)
			{
				_bindings[interfaceType] = new List<Dependency>();
			}

			_bindings[interfaceType].Add(dependency);
			if (singleInstance && !_singletons.ContainsKey(interfaceType))
			{
				_singletons.Add(interfaceType, dependency.Factory.Invoke());
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
			if (_singletons.TryGetValue(interfaceType, out var instance))
			{
				InjectDependencies(instance);
				return instance;
			}

			if (_bindings.TryGetValue(interfaceType, out var dependencies))
			{
				int dependencyIndex = 0;
				if (implementationType != null)
				{
					dependencyIndex = dependencies.FindIndex(x => x.ImplementationType == implementationType);
				}

				instance = dependencies[dependencyIndex].Factory?.Invoke();
				if (dependencies[dependencyIndex].IsSingleton)
				{
					_singletons[interfaceType] = instance;
				}
				else
				{
					if (!_instances.Contains(instance))
					{
						_instances.Add(instance);
					}
				}

				InjectDependencies(instance);
				return instance;
			}

			throw new Exception($"Unable to resolve dependency. Make sure it is registered correctly.");
		}

		public TConcrete ResolveConcrete<TConcrete>() where TConcrete : class
		{
			var instance = _instances.Find(x => x.GetType() == typeof(TConcrete));
			if (instance == null)
			{
				DependencyFactory dependencyFactory = DefaultFactory(typeof(TConcrete));
				instance = dependencyFactory.Invoke();
				_instances.Add(instance);
			}

			InjectDependencies(instance);
			return (TConcrete)instance;
		}

		private object[] ResolveArray(Type interfaceType)
		{
			List<object> collection = new List<object>();
			if (_bindings.TryGetValue(interfaceType, out var dependencies))
			{
				foreach (Dependency dependency in dependencies)
				{
					collection.Add(Resolve(interfaceType, dependency.ImplementationType));
				}

				return collection.ToArray();
			}

			throw new Exception($"Unable to resolve dependency. Make sure it is registered correctly.");
		}

		/// <summary>
		/// Injects dependencies into properties marked with the [Inject] attribute on the target object.
		/// </summary>
		/// <param name="target">The target object to inject dependencies into.</param>
		/// <returns>The current DependencyContainer instance.</returns>
		public DependencyContainer InjectDependencies(object target)
		{
			var targetType = target.GetType();
			var properties = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var property in properties)
			{
				if (property.GetCustomAttribute<InjectAttribute>() != null)
				{
					var propertyType = property.FieldType;
					if (propertyType.IsArray)
					{
						try
						{
							Type arrayToItemType = propertyType.GetElementType();
							var  resolveArray    = ResolveArray(arrayToItemType);
							if (arrayToItemType != null)
							{
								Array array = Array.CreateInstance(arrayToItemType, resolveArray.Length);
								for (int i = 0; i < resolveArray.Length; i++)
								{
									array.SetValue(resolveArray[i], i);
								}

								property.SetValue(target, array);
							}
						}
						catch (Exception e)
						{
							Debug.LogError($"Failed to inject dependency into property '{property.Name}' on object of type '{targetType}'. Error: {e.Message}");
						}
					}
					else
					{
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

			return this;
		}

		private DependencyFactory DefaultFactory(Type type)
		{
			var constructorInfo = type.GetConstructor(Type.EmptyTypes);
			if (constructorInfo == null)
			{
				throw new Exception($"No parameterless constructor found for type '{type}'.");
			}

			return () => constructorInfo.Invoke(null);
		}

	}


}