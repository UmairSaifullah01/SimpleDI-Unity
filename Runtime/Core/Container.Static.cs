using System;
using UnityEngine;

namespace THEBADDEST.UnityDI
{
    public partial class Container
    {
        private static readonly object _containerLock = new object();
        private static Container _container;
        private static InjectableObjectTracker injectableObjectTracker;

        static Container()
        {
            _container = Container.Create();
            injectableObjectTracker = new InjectableObjectTracker();
        }

        /// <summary>
        /// Gets the static container instance.
        /// </summary>
        public static Container Global => GetStaticContainer();
        /// <summary>
        /// Creates a new instance of Container.
        /// </summary>
        /// <returns>The created Container instance.</returns>
        public static Container Create()
        {
            return new Container();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Preload()
        {
            // Force the static constructor to run
            var container = GetStaticContainer();
            var tracker = IOTracker();
        }

        /// <summary>
        /// Gets the static container instance.
        /// </summary>
        /// <returns>The static container.</returns>
        static Container GetStaticContainer()
        {
            lock (_containerLock)
            {
                return _container;
            }
        }
        
        /// <summary>
        /// Gets the static InjectableObjectTracker instance, which is used to track and inject dependencies into objects
        /// found in the scene, prefabs, and asset database.
        /// </summary>
        /// <returns>The static InjectableObjectTracker instance.</returns>
        internal static InjectableObjectTracker IOTracker()
        {
            return injectableObjectTracker;
        }
    }
}