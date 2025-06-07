using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace THEBADDEST.UnityDI
{


	[CreateAssetMenu(menuName = "THEBADDEST/UnityDI/ProjectContext", fileName = "ProjectContext", order = 0)]
	public class ProjectContext : ScriptableObject
	{

		[SerializeField] bool enabled = false;
		[SerializeField] List<ScriptableObject> projectObjectsBeforeSceneLoaded;
		[SerializeField] List<ScriptableObject> projectObjectsAfterSceneLoaded;
		public Dictionary<Type, List<object>> ProjectObjectsWithType { get; private set; }

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
			if (projectContext == null)
			{
				projectContext=CreateProjectContext();
			}
			if (projectContext != null)
			{
				if (!projectContext.enabled) return;
				projectContext.ConvertDictionaryBeforeSceneLoaded();
				Container.IOTracker().InjectProject(projectContext);
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void LoadAfterSceneLoad()
		{
			var projectContext = Resources.Load("ProjectContext") as ProjectContext;
			if (projectContext == null)
			{
				projectContext=CreateProjectContext();
			}
			if (projectContext != null)
			{
				if (!projectContext.enabled) return;
				projectContext.ConvertDictionaryAfterSceneLoaded();
				Container.IOTracker().InjectProject(projectContext);
			}
		}
		

		static ProjectContext CreateProjectContext()
		{
#if UNITY_EDITOR
			// Create the ProjectContext asset
			var projectContext = ScriptableObject.CreateInstance<ProjectContext>();

			// Ensure the Resources directory exists
			if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
			{
				UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
			}

			// Create the asset in the Resources folder
			UnityEditor.AssetDatabase.CreateAsset(projectContext, "Assets/Resources/ProjectContext.asset");
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
			return projectContext;
#endif
			return null;		
		} 

	}


}