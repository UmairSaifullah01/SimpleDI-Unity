using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Installer class for setting up weapon dependencies.
    /// </summary>
    public class WeaponInstaller : MonoInstaller
    {
        [SerializeField] private Gun gun;
        [SerializeField] private Sword sword;
        [SerializeField] private Player player;

        protected override void InstallBindings()
        {
            // Bind weapons to the container
            Container.Bind<IWeapon, Gun>(() => gun);
            Container.Bind<IWeapon, Sword>(() => sword);
        }

        protected override void ResolveAll()
        {
            // Resolve all weapon dependencies
            Container.InjectDependencies(player);
        }
    }
}