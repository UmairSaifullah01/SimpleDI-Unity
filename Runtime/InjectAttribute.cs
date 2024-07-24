using System;


namespace THEBADDEST.SimpleDependencyInjection
{


	/// <summary>
	/// Attribute used to mark properties or fields that should be injected with dependencies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InjectAttribute : Attribute
	{

	}


}