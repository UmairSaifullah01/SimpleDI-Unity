using System;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public enum SceneDepth
	{
		None,
		WholeScene,
		Children,
		SpecificObjects,
		SpecificObjectsAfterAwake
	}
	
	[DefaultExecutionOrder(-2)]
	public class SceneContext : MonoBehaviour
	{

		[SerializeField] SceneDepth      sceneDepth = SceneDepth.Children;
		[SerializeField] MonoBehaviour[] sceneObjects;

		public Dictionary<Type, List<object>> SceneComponents { get; private set; }

		void Awake()
		{
			switch (sceneDepth)
			{
				case SceneDepth.None:
					return;

				case SceneDepth.WholeScene:
					 InjectWholeScene();
					break;

				case SceneDepth.Children:
					InjectChildren();
					break;

				case SceneDepth.SpecificObjects:
					InjectSpecificObjects();
					break;

				case SceneDepth.SpecificObjectsAfterAwake:
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			var ioTracker = DCExtensionMethods.GetStaticIOTracker();
			ioTracker.InjectScene(this);
		}

		void Start()
		{
			if (sceneDepth == SceneDepth.SpecificObjectsAfterAwake)
			{
				InjectSpecificObjects();
				var ioTracker = DCExtensionMethods.GetStaticIOTracker();
				ioTracker.InjectScene(this);
			}
		}

		void OnDestroy()
		{
			var ioTracker = DCExtensionMethods.GetStaticIOTracker();
			ioTracker.RemoveScene(this);
		}

		void InjectSpecificObjects()
		{
			SceneComponents = new Dictionary<Type, List<object>>();
			foreach (MonoBehaviour component in sceneObjects)
			{
				var type = component.GetType();
				if (!SceneComponents.ContainsKey(type))
				{
					SceneComponents[type] = new List<object>();
				}

				SceneComponents[type].Add(component);
			}
		}
		void InjectChildren()
		{
			SceneComponents = new Dictionary<Type, List<object>>();
			foreach (MonoBehaviour component in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
			{
				var type = component.GetType();
				if (!SceneComponents.ContainsKey(type))
				{
					SceneComponents[type] = new List<object>();
				}

				SceneComponents[type].Add(component);
			}
		}
		void InjectWholeScene()
		{
			
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
		}

	}


}