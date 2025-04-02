using System;


namespace THEBADDEST.SimpleDependencyInjection
{
	/// <summary>
	/// Attribute for marking fields, properties, or methods that should be injected.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class InjectAttribute : Attribute { }


}