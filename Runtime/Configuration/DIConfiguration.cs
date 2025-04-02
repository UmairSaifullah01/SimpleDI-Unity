using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public abstract class DIConfiguration : ScriptableObject
	{
		public virtual void Configure(DependencyContainer container)
		{
			container.LoadConfiguration(this);
		}
	}


}