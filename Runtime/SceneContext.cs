using System;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{

	
	public class SceneContext : MonoBehaviour
	{

		public string SceneName { get; private set; }

		public Dictionary<Type, List<object>> SceneComponents { get; private set; }

		void Awake()
		{
			SceneName       = gameObject.scene.name;
			SceneComponents = new Dictionary<Type, List<object>>();
			foreach (var rootObj in gameObject.scene.GetRootGameObjects())
			{
				foreach (MonoBehaviour component in rootObj.GetComponentsInChildren<MonoBehaviour>(true))
				{
					var type = component.GetType();
					if (!SceneComponents.ContainsKey(type))
					{
						SceneComponents[type] = new List<object>();
					}

					SceneComponents[type].Add(component);
				}
			}

			var ioTracker = DCExtensionMethods.GetStaticIOTracker();
			ioTracker.SceneContexts.Add(this);
			ioTracker.InjectScene(SceneName);
		}

	}


}