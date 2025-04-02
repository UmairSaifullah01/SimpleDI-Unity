using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	[CreateAssetMenu(menuName = "THEBADDEST/SimpleDependencyInjection/ProjectContext", fileName = "ProjectContext", order = 0)]
	public class ProjectContext : ScriptableObject
	{

		[SerializeField] bool                          enabled = false;
		[SerializeField] List<ScriptableObject>        projectObjectsBeforeSceneLoaded;
		[SerializeField] List<ScriptableObject>        projectObjectsAfterSceneLoaded;
		public           Dictionary<Type,List<object>> ProjectObjectsWithType{ get; private set; }

		private void ConvertDictionaryBeforeSceneLoaded()
		{
			ProjectObjectsWithType = projectObjectsBeforeSceneLoaded.GroupBy(obj => obj.GetType()).ToDictionary(grp => grp.Key, grp => grp.Cast<object>().ToList());
		}
		private void ConvertDictionaryAfterSceneLoaded()
		{
			ProjectObjectsWithType = projectObjectsAfterSceneLoaded.GroupBy(obj => obj.GetType()).ToDictionary(grp => grp.Key, grp => grp.Cast<object>().ToList());
		}
		
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void LoadBeforeSceneLoad()
		{
			var projectContext = Resources.Load("ProjectContext") as ProjectContext;
			if (projectContext != null)
			{
				if(!projectContext.enabled) return;
				
				projectContext.ConvertDictionaryBeforeSceneLoaded();
				DependencyContainer.GetStaticIOTracker().InjectProject(projectContext);
			}
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void LoadAfterSceneLoad()
		{
			var projectContext = Resources.Load("ProjectContext") as ProjectContext;
			if (projectContext != null)
			{
				if(!projectContext.enabled) return;
				
				projectContext.ConvertDictionaryAfterSceneLoaded();
				DependencyContainer.GetStaticIOTracker().InjectProject(projectContext);
			}
		}

	}


}