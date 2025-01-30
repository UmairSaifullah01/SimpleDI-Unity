using System;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{
	/// <summary>
	/// Attribute for marking fields, properties, or methods that should be injected.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class InjectAttribute : Attribute { }

	/// <summary>
	/// Attribute for marking classes that should be automatically registered in the container.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class InjectableAttribute : PropertyAttribute
	{
		public Lifetime                              Lifetime { get; }
		
		public InjectableAttribute(Lifetime lifetime = SimpleDependencyInjection.Lifetime.Transient)
		{
			Lifetime = lifetime;
		}
		
	}


}