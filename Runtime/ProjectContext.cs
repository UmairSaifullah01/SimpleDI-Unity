using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	[CreateAssetMenu(menuName = "THEBADDEST/SimpleDependencyInjection/ProjectContext", fileName = "ProjectContext", order = 0)]
	public class ProjectContext : ScriptableObject
	{

		[SerializeField] List<ScriptableObject>        projectObjects;
		public           Dictionary<Type,List<object>> ProjectObjectsWithType{ get; private set; }

		private void ConvertDictionary()
		{
			ProjectObjectsWithType = projectObjects.GroupBy(obj => obj.GetType()).ToDictionary(grp => grp.Key, grp => grp.Cast<object>().ToList());
		}
		
		[RuntimeInitializeOnLoadMethod]
		static void Initialize()
		{
			
			var projectContext = Resources.Load("ProjectContext") as ProjectContext;
			if (projectContext != null)
			{
				projectContext.ConvertDictionary();
				DCExtensionMethods.GetStaticIOTracker().InjectProject(projectContext);
			}
		}

	}


}