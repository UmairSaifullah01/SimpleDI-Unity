using UnityEngine;

namespace THEBADDEST.UnityDI.UIDemo
{
    /// <summary>
    /// Installer for the UI demo.
    /// </summary>
    public class UIDemoInstaller : MonoInstaller
    {
        [SerializeField] private ScreenManager screenManager;
        [SerializeField] private MainMenuScreen mainMenuScreen;

        protected override void InstallBindings()
        {
            // Bind the screen manager as a singleton
            Container.Bind<IScreenManager, ScreenManager>(() => screenManager);
        }

        protected override void ResolveAll()
        {
            // Resolve dependencies
            Container.Resolve<IScreenManager>();
        }
    }
}