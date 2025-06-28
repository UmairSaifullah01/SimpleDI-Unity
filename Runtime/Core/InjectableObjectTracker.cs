using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace THEBADDEST.UnityDI
{
    public class InjectableObjectTracker
    {
        public HashSet<SceneContext> SceneContexts { get; } = new HashSet<SceneContext>();
        public HashSet<ProjectContext> ProjectContexts { get; } = new HashSet<ProjectContext>();
        public List<Type> AllInjectableTypes { get; }

        public InjectableObjectTracker()
        {
            // Cache all injectable types during initialization
            AllInjectableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<InjectableAttribute>() != null)
                .ToList();
        }

        /// <summary>
        /// Injects dependencies into objects in the specified scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to inject dependencies into.</param>
        public void InjectScene(SceneContext sceneContext)
        {
            var desiredSceneContext = sceneContext;
            if (desiredSceneContext != null)
            {
                SceneContexts.Add(sceneContext);
                AddBindings(desiredSceneContext.SceneComponents);
                InjectDependencies(desiredSceneContext.SceneComponents);
            }
        }

        /// <summary>
        /// Injects dependencies into objects in the specified project context.
        /// </summary>
        /// <param name="projectContext">The project context to inject dependencies into.</param>
        public void InjectProject(ProjectContext projectContext)
        {
            if (projectContext == null) return;

            ProjectContexts.Add(projectContext);
            AddBindings(projectContext.ProjectObjectsWithType);
            InjectDependencies(projectContext.ProjectObjectsWithType);
        }

        /// <summary>
        /// Injects dependencies into objects attached to the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object to inject dependencies into.</param>
        /// <param name="children">Whether to inject dependencies into all child objects recursively, or just the specified game object.</param>
        public void InjectObject(GameObject gameObject, bool children = false)
        {
            var objectsWithType = (children ? gameObject.GetComponentsInChildren<MonoBehaviour>() : gameObject.GetComponents<MonoBehaviour>())
                .GroupBy(component => component.GetType())
                .ToDictionary(group => group.Key, group => group.ToList<object>());

            AddBindings(objectsWithType);
            InjectDependencies(objectsWithType);
        }

        /// <summary>
        /// Adds bindings for the specified objects to the global container.
        /// </summary>
        /// <param name="objectsWithType">A dictionary of objects grouped by their type.</param>
        private void AddBindings(Dictionary<Type, List<object>> objectsWithType)
        {
            foreach (var type in AllInjectableTypes)
            {
                if (objectsWithType.TryGetValue(type, out var objects))
                {
                    var attribute = type.GetCustomAttribute<InjectableAttribute>();
                    if (attribute == null) continue;

                    var interfaces = type.GetInterfaces();
                    if (interfaces.Length > 0)
                    {
                        foreach (var interfaceType in interfaces)
                        {
                            objects.ForEach(obj => Container.Global.Bind(interfaceType, type, () => obj, attribute.Lifetime));
                        }
                    }
                    else
                    {
                        objects.ForEach(obj => Container.Global.Bind(type, type, () => obj, attribute.Lifetime));
                    }
                }
            }
        }

        /// <summary>
        /// Injects dependencies into the specified objects.
        /// </summary>
        /// <param name="objectsWithType">A dictionary of objects grouped by their type.</param>
        private void InjectDependencies(Dictionary<Type, List<object>> objectsWithType)
        {
            foreach (var objects in objectsWithType.Values)
            {
                foreach (var obj in objects)
                {
                    Container.Global.InjectDependencies(obj);
                }
            }
        }

        public void RemoveScene(SceneContext sceneContext)
        {
            foreach (var sceneComponent in sceneContext.SceneComponents)
            {
                Container.Global.Unbind(sceneComponent.Key);
                sceneComponent.Value.ForEach(x => Container.Global.Unbind(x.GetType()));
            }
        }
    }
}