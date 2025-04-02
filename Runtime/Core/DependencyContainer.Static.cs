using System;
using UnityEngine;

namespace THEBADDEST.SimpleDependencyInjection
{
    public partial class DependencyContainer
    {
        private static readonly object _containerLock = new object();
        private static DependencyContainer _container;
        private static InjectableObjectTracker injectableObjectTracker;

        static DependencyContainer()
        {
            _container = DependencyContainer.Create();
            injectableObjectTracker = new InjectableObjectTracker();
        }

        /// <summary>
        /// Gets the static dependency container instance.
        /// </summary>
        public static DependencyContainer Global => GetStaticContainer();
        /// <summary>
        /// Creates a new instance of DependencyContainer.
        /// </summary>
        /// <returns>The created DependencyContainer instance.</returns>
        public static DependencyContainer Create()
        {
            return new DependencyContainer();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Preload()
        {
            // Force the static constructor to run
            var container = GetStaticContainer();
            var tracker = GetStaticIOTracker();
        }

        /// <summary>
        /// Gets the static dependency container instance.
        /// </summary>
        /// <returns>The static dependency container.</returns>
        static DependencyContainer GetStaticContainer()
        {
            lock (_containerLock)
            {
                return _container;
            }
        }

        /// <summary>
        /// Gets the static instance of the InjectableObjectTracker.
        /// </summary>
        /// <returns>The static InjectableObjectTracker instance.</returns>
        internal static InjectableObjectTracker GetStaticIOTracker()
        {
            return injectableObjectTracker;
        }
    }
}