using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public static class InstantiateExtensions
	{
		/// <summary>
		/// Instantiates a new object and injects dependencies into it.
		/// </summary>
		public static T InstantiateAndInject<T>(this T original) where T : Object
		{
			var instance = Object.Instantiate(original);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance;
		}

		/// <summary>
		/// Instantiates a new object at a specific position and rotation, and injects dependencies into it.
		/// </summary>
		public static T InstantiateAndInject<T>(this T original, Vector3 position, Quaternion rotation) where T : Object
		{
			var instance = Object.Instantiate(original, position, rotation);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance;
		}

		/// <summary>
		/// Instantiates a new object as a child of a parent transform, and injects dependencies into it.
		/// </summary>
		public static T InstantiateAndInject<T>(this T original, Transform parent) where T : Object
		{
			var instance = Object.Instantiate(original, parent);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance;
		}

		/// <summary>
		/// Instantiates a new object as a child of a parent transform, with world position and rotation, and injects dependencies into it.
		/// </summary>
		public static T InstantiateAndInject<T>(this T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
		{
			var instance = Object.Instantiate(original, position, rotation, parent);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance;
		}

		/// <summary>
		/// Instantiates a new object and injects dependencies into it, returning the result as the specified type.
		/// </summary>
		public static T InstantiateAndInject<T>(this Object original) where T : Object
		{
			var instance = Object.Instantiate(original);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance as T;
		}

		/// <summary>
		/// Instantiates a new object at a specific position and rotation, and injects dependencies into it, returning the result as the specified type.
		/// </summary>
		public static T InstantiateAndInject<T>(this Object original, Vector3 position, Quaternion rotation) where T : Object
		{
			var instance = Object.Instantiate(original, position, rotation);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance as T;
		}

		/// <summary>
		/// Instantiates a new object as a child of a parent transform, and injects dependencies into it, returning the result as the specified type.
		/// </summary>
		public static T InstantiateAndInject<T>(this Object original, Transform parent) where T : Object
		{
			var instance = Object.Instantiate(original, parent);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance as T;
		}

		/// <summary>
		/// Instantiates a new object as a child of a parent transform, with world position and rotation, and injects dependencies into it, returning the result as the specified type.
		/// </summary>
		public static T InstantiateAndInject<T>(this Object original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
		{
			var instance = Object.Instantiate(original, position, rotation, parent);
			DependencyContainer.Global.InjectDependencies(instance);
			return instance as T;
		}
	}


}