using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public abstract class MonoInstaller : MonoBehaviour
	{

		protected DependencyContainer container;


		protected virtual void Setup()
		{
			container = DependencyContainer.Create();
			InstallBindings();
		}

		/// <summary>
		/// Method to be implemented by derived classes for binding dependencies.
		/// </summary>
		protected abstract void InstallBindings();

		/// <summary>
		/// Installs the dependencies into the target object.
		/// </summary>
		/// <param name="target">The target object to inject the dependencies into.</param>
		public void Install()
		{
			Setup();
			ResolveAll();
		}

		protected abstract void ResolveAll();

	}


}