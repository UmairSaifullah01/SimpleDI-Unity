using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	/// <summary>
	/// Abstract class for creating installers to bind dependencies and install them into target objects.
	/// </summary>
	public abstract class Installer
	{

		public DependencyContainer _container { get; private set; }

		/// <summary>
		/// Default constructor that creates a new instance of the DependencyContainer.
		/// </summary>
		protected Installer()
		{
			_container = DependencyContainer.Create();
			Setup();
		}


		/// <summary>
		/// Constructor that allows passing an existing DependencyContainer instance.
		/// </summary>
		/// <param name="container">The DependencyContainer instance to use.</param>
		public Installer(DependencyContainer container)
		{
			_container = container;
			Setup();
		}

		private void Setup()
		{
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
			_container.InjectDependencies(target);
		}

	}


}