using UnityEngine;


namespace THEBADDEST.UnityDI
{


	public abstract class DIConfiguration : ScriptableObject
	{
		public virtual void Configure(Container container)
		{
			container.LoadConfiguration(this);
		}
	}


}