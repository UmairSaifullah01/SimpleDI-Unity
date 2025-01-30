using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace THEBADDEST.SimpleDependencyInjection
{


}
namespace THEBADDEST.SimpleDependencyInjection
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
        public void InjectScene(string sceneName)
        {
            var desiredSceneContext = SceneContexts.FirstOrDefault(context => context.SceneName == sceneName);
            if (desiredSceneContext != null)
            {
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
                            objects.ForEach(obj => DependencyContainer.GlobalContainer.Bind(interfaceType, type, () => obj, attribute.Lifetime));
                        }
                    }
                    else
                    {
                        objects.ForEach(obj => DependencyContainer.GlobalContainer.Bind(type, type, () => obj, attribute.Lifetime));
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
                    DependencyContainer.GlobalContainer.InjectDependencies(obj);
                }
            }
        }
    }
}