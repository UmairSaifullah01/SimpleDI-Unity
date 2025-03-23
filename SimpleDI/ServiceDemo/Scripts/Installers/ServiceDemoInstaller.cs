using UnityEngine;

namespace THEBADDEST.SimpleDependencyInjection.ServiceDemo
{
    /// <summary>
    /// Installer for the service demo.
    /// </summary>
    public class ServiceDemoInstaller : MonoInstaller
    {
        [SerializeField] private AnalyticsService analyticsService;
        [SerializeField] private SaveService saveService;
        [SerializeField] private GameManager gameManager;

        protected override void InstallBindings()
        {
            // Bind services as singletons
            Container.Bind<IAnalyticsService, AnalyticsService>(() => analyticsService);
            Container.Bind<ISaveService, SaveService>(() => saveService);
        }

        protected override void ResolveAll()
        {
            // Resolve dependencies
            Container.InjectDependencies(gameManager);
        }
    }
}