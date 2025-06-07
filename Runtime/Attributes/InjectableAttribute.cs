using System;
using UnityEngine;


namespace THEBADDEST.UnityDI
{


	/// <summary>
	/// Attribute for marking classes that should be automatically registered in the container.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class InjectableAttribute : PropertyAttribute
	{
		public Lifetime                              Lifetime { get; }
		
		public InjectableAttribute(Lifetime lifetime = UnityDI.Lifetime.Transient)
		{
			Lifetime = lifetime;
		}
		
	}


}