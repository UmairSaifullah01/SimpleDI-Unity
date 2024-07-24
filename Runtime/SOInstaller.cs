using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	
	public abstract class SOInstaller : ScriptableObject
	{
		protected DependencyContainer _container;

		
		private void Setup()
		{
			_container=DependencyContainer.Create();
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
		public void Install(object target)
		{
			Setup();
			_container.InjectDependencies(target);
		}
	}


}