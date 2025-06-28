using UnityEngine;

namespace THEBADDEST.UnityDI
{
	/// <summary>
	/// Base class for all installers providing common functionality.
	/// </summary>
	public abstract class BaseInstaller
	{
		protected Container Container { get; private set; }

		protected BaseInstaller(Container container = null)
		{
			Container = container ?? Container.Create();
			Setup();
		}

		protected virtual void Setup()
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
		public virtual void Install(object target)
		{
			Container.InjectDependencies(target);
		}

		/// <summary>
		/// Resolves all dependencies. To be implemented by derived classes if needed.
		/// </summary>
		protected virtual void ResolveAll() { }
	}

	/// <summary>
	/// Standard installer class for non-MonoBehaviour scenarios.
	/// </summary>
	public abstract class Installer : BaseInstaller
	{
		protected Installer(Container container = null) : base(container) { }
	}

	/// <summary>
	/// MonoBehaviour-based installer for Unity components.
	/// </summary>
	public abstract class MonoInstaller : MonoBehaviour
	{
		protected Container Container { get; private set; }

		protected virtual void Awake()
		{
			Container = Container.Create();
			Setup();
		}

		protected virtual void Setup()
		{
			InstallBindings();
		}

		/// <summary>
		/// Method to be implemented by derived classes for binding dependencies.
		/// </summary>
		protected abstract void InstallBindings();

		/// <summary>
		/// Installs and resolves all dependencies.
		/// </summary>
		public virtual void Install()
		{
			Setup();
			ResolveAll();
		}

		/// <summary>
		/// Resolves all dependencies. To be implemented by derived classes if needed.
		/// </summary>
		protected abstract void ResolveAll();
	}

	/// <summary>
	/// ScriptableObject-based installer for configuration scenarios.
	/// </summary>
	public abstract class SOInstaller : ScriptableObject
	{
		protected Container Container { get; private set; }

		protected virtual void Setup()
		{
			Container = Container.Create();
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
		public virtual void Install(object target)
		{
			Setup();
			Container.InjectDependencies(target);
		}
	}
}